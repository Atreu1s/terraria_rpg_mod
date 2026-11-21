using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using RPGMod.Common.Config;
using RPGMod.Common.Systems;

namespace RPGMod.Common.Data;

/// <summary>
/// Узел древа — это данные, а не отдельный класс. Apply вызывается каждый кадр
/// из RPGPlayer.PostUpdateEquips для выбранных узлов (простые статовые бонусы).
/// Условные эффекты тиров 2–3 живут в хуках RPGPlayer/NodeEffects и проверяют Has(id).
/// ID строго из docs/DESIGN.md §4: от них зависят сохранения и имена иконок.
/// </summary>
public record SkillNode(string Id, string ParentId, int Tier, int ClassId, int Index, Action<Player> Apply);

public static class SkillTree
{
    /// <summary>Реестр узлов по ID.</summary>
    public static readonly Dictionary<string, SkillNode> Nodes = new();

    /// <summary>Узлы класса в порядке двоичной кучи: дети i — 2i+1 и 2i+2.</summary>
    public static readonly Dictionary<int, SkillNode[]> ByClass = new();

    // ID в порядке индексов 0–14 (DESIGN §4)
    private static readonly string[] warrior =
    {
        "warrior",
        "berserker", "guardian",
        "blood_rage", "frenzy", "iron_flesh", "retribution",
        "possessed", "unbreakable", "whirlwind", "iron_rhythm", "colossus", "will_shield", "eye_for_eye", "counterstrike",
    };

    private static readonly string[] ranger =
    {
        "ranger",
        "sniper", "gunner",
        "cold_blood", "long_range", "spin_up", "quartermaster",
        "deadshot", "hunters_mark", "explosive_rounds", "armor_breaker", "lead_rain", "overheat", "run_and_gun", "bottomless_mag",
    };

    private static readonly string[] mage =
    {
        "mage",
        "arcanist", "mystic",
        "overload", "elemental", "flow", "meditation",
        "blood_magic", "resonance", "catalyst", "chain_reaction", "mana_shield", "overflow", "enlightenment", "mana_blood",
    };

    private static readonly string[] summoner =
    {
        "summoner",
        "swarm_lord", "beastmaster",
        "pack", "thousand_bites", "whip_master", "bond",
        "legion", "unity", "hive", "devourer", "sic_em", "pack_heal", "alpha", "sacrifice",
    };

    // Always registered so disabling Calamity preserves the selected nodes.
    private static readonly string[] rogue =
    {
        "rogue", "assassin", "trickster", "ambush", "shadow", "juggler", "evasion",
        "executioner", "first_blood", "vanish", "dusk_blade", "ricochet", "bravado", "smoke_bomb", "lucky_break",
    };

    /// <summary>
    /// Простые (безусловные) эффекты: класс и тир 1, плюс постоянные части тиров 2–3.
    /// Проценты урона/скорости — аддитивные (как у брони), проценты HP/маны — от итогового значения.
    /// </summary>
    private static readonly Dictionary<string, Action<Player>> simpleEffects = new()
    {
        ["rogue"] = p =>
        {
            if (CalamityCompat.Rogue is not DamageClass damage) return;
            var c = RPGConfig.Instance;
            p.GetDamage(damage) += c.RogueRootDamage;
            p.GetCritChance(damage) += c.RogueRootCrit;
        },
        ["assassin"] = p =>
        {
            if (CalamityCompat.Rogue is not DamageClass damage) return;
            var c = RPGConfig.Instance;
            p.GetDamage(damage) += c.AssassinDamage;
            p.GetCritChance(damage) += c.AssassinCrit;
            p.statLifeMax2 = (int)(p.statLifeMax2 * (1f - c.AssassinLifePenalty));
        },
        ["trickster"] = p =>
        {
            if (CalamityCompat.Rogue is not DamageClass damage) return;
            var c = RPGConfig.Instance;
            p.GetAttackSpeed(damage) += c.TricksterAttackSpeed;
            p.moveSpeed += c.TricksterMoveSpeed;
            p.GetDamage(damage) -= c.TricksterDamagePenalty;
        },
        // ---- Воин ----
        ["warrior"] = p =>
        {
            p.GetDamage(DamageClass.Melee) += 0.05f;
            p.statLifeMax2 += 20;
        },
        ["berserker"] = p =>
        {
            p.GetDamage(DamageClass.Melee) += 0.12f;
            p.GetAttackSpeed(DamageClass.Melee) += 0.08f;
            p.endurance -= 0.10f;
        },
        ["guardian"] = p =>
        {
            p.statLifeMax2 = (int)(p.statLifeMax2 * 1.10f);
            p.endurance += 0.08f;
            p.GetDamage(DamageClass.Melee) -= 0.08f;
        },

        // ---- Стрелок ----
        ["ranger"] = p =>
        {
            p.GetDamage(DamageClass.Ranged) += 0.05f;
            p.moveSpeed += 0.05f;
        },
        ["sniper"] = p =>
        {
            p.GetDamage(DamageClass.Ranged) += 0.10f;
            p.GetCritChance(DamageClass.Ranged) += 8f;
            p.GetAttackSpeed(DamageClass.Ranged) -= 0.12f;
        },
        ["gunner"] = p =>
        {
            p.GetAttackSpeed(DamageClass.Ranged) += 0.15f;
            p.GetCritChance(DamageClass.Ranged) -= 6f;
            p.GetDamage(DamageClass.Ranged) -= 0.05f;
        },

        // ---- Маг ----
        ["mage"] = p =>
        {
            p.GetDamage(DamageClass.Magic) += 0.05f;
            p.statManaMax2 += 20;
        },
        ["arcanist"] = p =>
        {
            p.GetDamage(DamageClass.Magic) += 0.14f;
            p.statManaMax2 = (int)(p.statManaMax2 * 0.85f);
        },
        ["mystic"] = p =>
        {
            p.manaCost -= 0.20f;
            p.statManaMax2 += 60;
            p.manaRegenBonus += 25;
            p.GetDamage(DamageClass.Magic) -= 0.08f;
        },

        // ---- Призыватель ----
        ["summoner"] = p =>
        {
            p.GetDamage(DamageClass.Summon) += 0.05f;
            p.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.05f;
        },
        ["swarm_lord"] = p =>
        {
            p.maxMinions += 1;
            p.GetDamage(DamageClass.Summon) -= 0.08f;
        },
        ["beastmaster"] = p =>
        {
            p.GetDamage(DamageClass.Summon) += 0.14f;
            p.GetKnockback(DamageClass.Summon) += 0.14f;
            p.statLifeMax2 = (int)(p.statLifeMax2 * 0.90f);
        },
    };

    static SkillTree()
    {
        Register(ClassIds.Warrior, warrior);
        Register(ClassIds.Ranger, ranger);
        Register(ClassIds.Mage, mage);
        Register(ClassIds.Summoner, summoner);
        Register(ClassIds.Rogue, rogue);
    }

    private static void Register(int classId, string[] ids)
    {
        var arr = new SkillNode[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            string parent = i == 0 ? null : ids[(i - 1) / 2];
            simpleEffects.TryGetValue(ids[i], out var apply);
            var node = new SkillNode(ids[i], parent, TierOf(i), classId, i, apply);
            arr[i] = node;
            Nodes[node.Id] = node;
        }
        ByClass[classId] = arr;
    }

    /// <summary>Тир по индексу в куче: 0 → класс, 1–2 → I, 3–6 → II, 7–14 → III.</summary>
    public static int TierOf(int index) => index switch
    {
        0 => 0,
        <= 2 => 1,
        <= 6 => 2,
        _ => 3,
    };

    public static SkillNode Get(string id) => id != null && Nodes.TryGetValue(id, out var n) ? n : null;

    /// <summary>Соседний узел (второй ребёнок того же родителя) или null для корня.</summary>
    public static SkillNode Sibling(SkillNode node)
    {
        if (node.Index == 0)
            return null;
        int sib = node.Index % 2 == 1 ? node.Index + 1 : node.Index - 1;
        return ByClass[node.ClassId][sib];
    }

    public static IEnumerable<SkillNode> Children(SkillNode node)
    {
        var arr = ByClass[node.ClassId];
        int a = node.Index * 2 + 1;
        if (a < arr.Length) yield return arr[a];
        if (a + 1 < arr.Length) yield return arr[a + 1];
    }
}
