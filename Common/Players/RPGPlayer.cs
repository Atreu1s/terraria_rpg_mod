using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using RPGMod.Common.Data;
using RPGMod.UI;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RPGMod.Common.Players;

/// <summary>
/// Состояние RPG на каждого игрока (≈ store/контекст, привязанный к персонажу).
/// Сохраняется в файл персонажа, синхронизируется по сети.
/// </summary>
public class RPGPlayer : ModPlayer
{
    public const int StatCount = 6;

    public int Level = 1;
    /// <summary>Опыт внутри текущего уровня (0 … XPToNext(Level)).</summary>
    public long XP;

    // Поля следующих этапов — объявлены сразу, чтобы формат сохранения не менялся.
    public int ClassId;
    public int[] Invested = new int[StatCount];
    public HashSet<string> Picked = new();
    public bool Awakened;

    public long XPToNext => StatMath.XPToNext(Level);

    /// <summary>Свежие значения для нового персонажа (и защита от общих ссылок между экземплярами).</summary>
    public override void Initialize()
    {
        Level = 1;
        XP = 0;
        ClassId = 0;
        Invested = new int[StatCount];
        Picked = new HashSet<string>();
        Awakened = false;
    }

    // ---------------- Статы ----------------

    /// <summary>Итоговое значение стата: вложено + классовый прирост от уровня.</summary>
    public int GetStat(Stat s) => Invested[(int)s] + ClassGrowth.Points(ClassId, s, Level);

    public int ClassPoints(Stat s) => ClassGrowth.Points(ClassId, s, Level);

    public int TotalStatPoints => Level * Config.RPGConfig.Instance.StatPointsPerLevel;

    /// <summary>Свободные очки статов (может уйти в минус после /rpg level — тогда 0 в UI).</summary>
    public int FreeStatPoints => TotalStatPoints - Invested.Sum();

    public StatBonuses Bonuses => StatMath.Bonuses(
        GetStat(Stat.Str), GetStat(Stat.Dex), GetStat(Stat.Int),
        GetStat(Stat.Spirit), GetStat(Stat.Vit), GetStat(Stat.Luck));

    /// <summary>Вложить до <paramref name="amount"/> очков в стат. Возвращает, сколько вложено.</summary>
    public int Invest(Stat s, int amount)
    {
        int n = Math.Min(amount, FreeStatPoints);
        if (n <= 0)
            return 0;
        Invested[(int)s] += n;
        return n; // синхронизация уйдёт сама через SendClientChanges
    }

    public override void PostUpdateEquips()
    {
        StatBonuses b = Bonuses;

        Player.GetDamage(DamageClass.Melee) += b.MeleeDamage;
        Player.GetAttackSpeed(DamageClass.Melee) += b.MeleeSpeed;

        Player.GetDamage(DamageClass.Ranged) += b.RangedDamage;
        Player.moveSpeed += b.MoveSpeed;

        Player.GetDamage(DamageClass.Magic) += b.MagicDamage;
        Player.statManaMax2 += b.MaxMana;
        Player.manaCost -= b.ManaCost;

        Player.GetDamage(DamageClass.Summon) += b.MinionDamage;
        Player.maxMinions += b.MinionSlots;
        Player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += b.WhipBonus;
        Player.whipRangeMultiplier += b.WhipBonus;

        Player.statLifeMax2 += b.MaxLife;
        Player.endurance += b.DamageReduction;

        Player.GetCritChance(DamageClass.Generic) += b.Crit;

        // Узлы древа — по порядку тиров (важно для процентов от HP/маны).
        if (SkillTree.ByClass.TryGetValue(ClassId, out SkillNode[] nodes))
            foreach (SkillNode node in nodes)
                if (Picked.Contains(node.Id))
                    node.Apply?.Invoke(Player);
    }

    // ---------------- Класс и древо ----------------

    public bool Has(string nodeId) => Picked.Contains(nodeId);

    /// <summary>Требование тира 1–3: (уровень, босс убит).</summary>
    public static int TierLevel(int tier)
    {
        var c = Config.RPGConfig.Instance;
        return tier switch { 0 => c.ClassLevel, 1 => c.Tier1Level, 2 => c.Tier2Level, _ => c.Tier3Level };
    }

    public static bool TierBossDone(int tier) => tier switch
    {
        1 => Main.hardMode,
        2 => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3,
        3 => NPC.downedMoonlord,
        _ => true,
    };

    public bool TierOpen(int tier) => Level >= TierLevel(tier) && TierBossDone(tier);

    /// <summary>Сколько тиров открыто (0–3). Тир открыт, только если выполнены уровень и босс.</summary>
    public int TiersUnlocked() => (TierOpen(1) ? 1 : 0) + (TierOpen(2) ? 1 : 0) + (TierOpen(3) ? 1 : 0);

    /// <summary>Очки древа = открытые тиры − выбранные узлы тиров (сам класс не считается).</summary>
    public int TreePoints() => ClassId == ClassIds.None ? 0 : Math.Max(0, TiersUnlocked() - (Picked.Count - 1));

    public bool CanPickClass() => ClassId == ClassIds.None && Level >= TierLevel(0);

    public void PickClass(int classId)
    {
        if (!CanPickClass() || classId <= ClassIds.None || classId >= ClassIds.Count)
            return;
        ClassId = classId;
        Picked.Clear();
        Picked.Add(ClassIds.RootNode(classId));
        lastTiers = TiersUnlocked(); // не показывать тосты «тир открыт» сразу после выбора
    }

    public enum PickState { Picked, Available, Locked, BranchClosed }

    /// <summary>Состояние узла для UI и проверки выбора.</summary>
    public PickState GetState(SkillNode node)
    {
        if (Picked.Contains(node.Id))
            return PickState.Picked;
        if (node.ClassId != ClassId)
            return PickState.Locked;
        SkillNode sib = SkillTree.Sibling(node);
        if (sib != null && Picked.Contains(sib.Id))
            return PickState.BranchClosed;
        // Ветка закрыта и для потомков узлов, у которых выбран соседний.
        SkillNode parent = SkillTree.Get(node.ParentId);
        if (parent != null && GetState(parent) == PickState.BranchClosed)
            return PickState.BranchClosed;
        bool available = parent != null && Picked.Contains(parent.Id)
            && TierOpen(node.Tier) && node.Tier <= TiersUnlocked() && TreePoints() > 0;
        return available ? PickState.Available : PickState.Locked;
    }

    public bool TryPick(string nodeId)
    {
        SkillNode node = SkillTree.Get(nodeId);
        if (node == null || node.Tier == 0 || GetState(node) != PickState.Available)
            return false;
        Picked.Add(node.Id);
        return true;
    }

    // Уведомление «Открыт тир N»
    private int lastTiers = -1;

    public override void OnEnterWorld()
    {
        lastTiers = TiersUnlocked();
    }

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer || Main.dedServ)
            return;
        int tiers = TiersUnlocked();
        if (lastTiers >= 0 && tiers > lastTiers && ClassId != ClassIds.None)
        {
            string title = Language.GetTextValue("Mods.RPGMod.HUD.TierOpenTitle", tiers);
            string sub = Language.GetTextValue("Mods.RPGMod.HUD.TierOpenSubtitle");
            RPGHudSystem.ShowToast(new string('I', tiers), title, sub, RPGHudSystem.ClassColor(ClassId));
            Main.NewText(title, RPGHudSystem.ClassColor(ClassId));
        }
        lastTiers = tiers;
    }

    public override void UpdateLifeRegen()
    {
        Player.lifeRegen += Bonuses.LifeRegen;
    }

    public override void ModifyLuck(ref float luck)
    {
        luck += Bonuses.Luck;
    }

    public override bool CanConsumeAmmo(Item weapon, Item ammo)
    {
        float save = Bonuses.AmmoSave;
        return !(save > 0f && Main.rand.NextFloat() < save);
    }

    // ---------------- Опыт ----------------

    /// <summary>Начислить опыт. Вызывается на клиенте-владельце (или в одиночной игре).</summary>
    public void GainXP(long amount)
    {
        if (amount <= 0 || Player.whoAmI != Main.myPlayer)
            return;

        CombatText.NewText(Player.getRect(), new Color(242, 193, 78),
            Language.GetTextValue("Mods.RPGMod.HUD.XPGain", amount));

        XP += amount;
        int levelsGained = 0;
        while (XP >= XPToNext)
        {
            XP -= XPToNext;
            Level++;
            levelsGained++;
        }

        if (levelsGained > 0)
            OnLevelUp(levelsGained);
    }

    private void OnLevelUp(int levelsGained)
    {
        string title = Language.GetTextValue("Mods.RPGMod.HUD.LevelUpTitle");
        string sub = Language.GetTextValue("Mods.RPGMod.HUD.LevelUpSubtitle",
            levelsGained * Config.RPGConfig.Instance.StatPointsPerLevel);
        RPGHudSystem.ShowToast(Level.ToString(), title, sub, RPGHudSystem.AwakenGold);
        Main.NewText(Language.GetTextValue("Mods.RPGMod.HUD.LevelUpChat", Level), RPGHudSystem.AwakenGold);
    }

    /// <summary>«Сброс статов»: все вложенные очки снова свободны.</summary>
    public void ResetStats() => Array.Clear(Invested);

    /// <summary>
    /// «Сброс древа»: класс, узлы и пробуждение. Возвращает true, если было пробуждение —
    /// тогда вызывающий код (предмет) возвращает Камень пробуждения.
    /// </summary>
    public bool ResetTree()
    {
        bool wasAwakened = Awakened;
        ClassId = ClassIds.None;
        Picked.Clear();
        Awakened = false;
        return wasAwakened;
    }

    /// <summary>Полный сброс прогресса (отладка /rpg reset).</summary>
    public void ResetProgress()
    {
        Level = 1;
        XP = 0;
        ClassId = 0;
        Array.Clear(Invested);
        Picked.Clear();
        Awakened = false;
    }

    // ---------------- Сохранение ----------------

    public override void SaveData(TagCompound tag)
    {
        tag["level"] = Level;
        tag["xp"] = XP;
        tag["class"] = ClassId;
        tag["invested"] = Invested.ToList();
        tag["picked"] = Picked.ToList();
        tag["awakened"] = Awakened;
    }

    public override void LoadData(TagCompound tag)
    {
        // Каждое поле читается отдельно и с дефолтом: старые/битые сохранения не роняют мод.
        Level = Math.Max(1, tag.GetInt("level"));
        XP = Math.Max(0L, tag.GetLong("xp"));
        ClassId = tag.GetInt("class");

        Invested = new int[StatCount];
        if (tag.ContainsKey("invested"))
        {
            var list = tag.GetList<int>("invested");
            for (int i = 0; i < Math.Min(list.Count, StatCount); i++)
                Invested[i] = Math.Max(0, list[i]);
        }

        Picked = new HashSet<string>();
        if (tag.ContainsKey("picked"))
        {
            foreach (string id in tag.GetList<string>("picked"))
            {
                // Неизвестные ID и узлы чужого класса пропускаем (узел мог быть удалён/переименован).
                SkillNode node = SkillTree.Get(id);
                if (node != null && node.ClassId == ClassId)
                    Picked.Add(id);
            }
        }
        if (ClassId <= ClassIds.None || ClassId >= ClassIds.Count)
        {
            ClassId = ClassIds.None;
            Picked.Clear();
        }
        else
        {
            Picked.Add(ClassIds.RootNode(ClassId));
        }

        Awakened = tag.GetBool("awakened");
    }

    // ---------------- Мультиплеер ----------------

    /// <summary>Полная отправка состояния (при входе игрока и при изменениях).</summary>
    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)MessageType.SyncPlayer);
        packet.Write((byte)Player.whoAmI);
        WriteSync(packet);
        packet.Send(toWho, fromWho);
    }

    private void WriteSync(BinaryWriter w)
    {
        w.Write(Level);
        w.Write(XP);
        w.Write(ClassId);
        for (int i = 0; i < StatCount; i++)
            w.Write(Invested[i]);
        w.Write((short)Picked.Count);
        foreach (string id in Picked)
            w.Write(id);
        w.Write(Awakened);
    }

    public void ReceiveSync(BinaryReader r)
    {
        Level = r.ReadInt32();
        XP = r.ReadInt64();
        ClassId = r.ReadInt32();
        for (int i = 0; i < StatCount; i++)
            Invested[i] = r.ReadInt32();
        int count = r.ReadInt16();
        Picked.Clear();
        for (int i = 0; i < count; i++)
            Picked.Add(r.ReadString());
        Awakened = r.ReadBoolean();
    }

    /// <summary>Снимок состояния для сравнения в SendClientChanges (как prevProps в React).</summary>
    public override void CopyClientState(ModPlayer targetCopy)
    {
        var clone = (RPGPlayer)targetCopy;
        clone.Level = Level;
        clone.XP = XP;
        clone.ClassId = ClassId;
        clone.Invested = (int[])Invested.Clone();
        clone.Picked = new HashSet<string>(Picked);
        clone.Awakened = Awakened;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        var old = (RPGPlayer)clientPlayer;
        bool changed = old.Level != Level
            || old.XP != XP
            || old.ClassId != ClassId
            || !old.Invested.SequenceEqual(Invested)
            || !old.Picked.SetEquals(Picked)
            || old.Awakened != Awakened;

        if (changed)
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }
}
