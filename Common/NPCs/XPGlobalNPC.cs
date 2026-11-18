using System;
using RPGMod.Common.Config;
using RPGMod.Common.Data;
using RPGMod.Common.Players;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Common.NPCs;

/// <summary>
/// Начисление опыта за убийство. OnKill выполняется на сервере или в одиночной игре —
/// клиенты в мультиплеере сюда не попадают, поэтому опыт считает только сервер.
/// </summary>
public class XPGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        if (!GivesXP(npc))
            return;

        bool isTableBoss = BossLevels.TryGet(npc.type, out BossEntry boss);

        // Пожиратель миров: награда один раз, когда умирает последний сегмент.
        if (isTableBoss && BossLevels.EaterOfWorldsTypes.Contains(npc.type) && OtherEaterSegmentsAlive(npc))
            return;

        float power = GetPower(npc);
        int monsterLevel = StatMath.MonsterLevel(power);
        var cfg = RPGConfig.Instance;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (!player.active || player.dead && !npc.boss || !npc.playerInteraction[i])
                continue;

            long amount;
            if (isTableBoss)
            {
                amount = StatMath.BossXP(boss.Level, boss.Share);
            }
            else
            {
                int playerLevel = player.GetModPlayer<RPGPlayer>().Level;
                double xp = StatMath.BaseMonsterXP(power)
                          * StatMath.LevelDiffMultiplier(playerLevel, monsterLevel)
                          * EventMultiplier()
                          * cfg.XPMultiplier;

                // Боссы неизвестных модов (не ваниль и не Calamity) — по формуле × N.
                if (npc.boss && npc.ModNPC != null && !BossLevels.IsFromCalamity(npc))
                    xp *= cfg.UnknownBossMultiplier;

                amount = (long)Math.Round(xp);
            }

            Networking.GiveXP(Mod, player, amount);
        }
    }

    /// <summary>Кто вообще не даёт опыта.</summary>
    private static bool GivesXP(NPC npc)
    {
        if (npc.SpawnedFromStatue || npc.townNPC || npc.friendly || npc.CountsAsACritter)
            return false;
        if (npc.lifeMax <= 5 || npc.immortal)
            return false;
        // Черви: опыт только за главный сегмент.
        if (npc.realLife >= 0 && npc.realLife != npc.whoAmI)
            return false;
        return true;
    }

    /// <summary>Сила по базовым статам (ContentSamples), чтобы сложность мира не меняла уровень моба.</summary>
    public static float GetPower(NPC npc)
    {
        int life = npc.lifeMax, damage = npc.damage, defense = npc.defense;
        if (ContentSamples.NpcsByNetId.TryGetValue(npc.netID, out NPC sample))
        {
            life = sample.lifeMax;
            damage = sample.damage;
            defense = sample.defense;
        }
        return StatMath.MonsterPower(life, damage, defense);
    }

    private static double EventMultiplier()
    {
        bool eventActive = Main.pumpkinMoon || Main.snowMoon || Main.eclipse
            || Main.invasionType > 0 || DD2Event.Ongoing;
        return eventActive ? RPGConfig.Instance.EventMultiplier : 1.0;
    }

    private static bool OtherEaterSegmentsAlive(NPC dying)
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC other = Main.npc[i];
            if (i != dying.whoAmI && other.active && BossLevels.EaterOfWorldsTypes.Contains(other.type))
                return true;
        }
        return false;
    }
}
