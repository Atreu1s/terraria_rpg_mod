using System;
using RPGMod.Common.Config;

namespace RPGMod.Common;

/// <summary>
/// Чистые формулы без состояния (как utils-модуль в TS). Все коэффициенты — из конфига.
/// </summary>
public static class StatMath
{
    /// <summary>Опыт, нужный для перехода с уровня <paramref name="level"/> на следующий.</summary>
    public static long XPToNext(int level)
    {
        var cfg = RPGConfig.Instance;
        level = Math.Max(1, level);
        double value = cfg.XPCurveBase * Math.Pow(level, cfg.XPCurveExponent);
        return Math.Max(1L, (long)Math.Round(value));
    }

    /// <summary>«Сила» монстра по базовым характеристикам.</summary>
    public static float MonsterPower(int lifeMax, int damage, int defense)
    {
        var cfg = RPGConfig.Instance;
        return cfg.PowerLife * MathF.Sqrt(Math.Max(0, lifeMax))
             + cfg.PowerDamage * Math.Max(0, damage)
             + cfg.PowerDefense * Math.Max(0, defense);
    }

    public static int MonsterLevel(float power)
        => Math.Max(1, (int)Math.Round(power / RPGConfig.Instance.PowerPerLevel));

    /// <summary>Базовый опыт за моба (до множителей разницы уровней и событий).</summary>
    public static double BaseMonsterXP(float power)
    {
        var cfg = RPGConfig.Instance;
        return cfg.XPPowerFactor * Math.Pow(power, cfg.XPPowerExponent);
    }

    /// <summary>
    /// Множитель за разницу уровней (игрок − монстр):
    /// до Grace — 100%, дальше −Penalty за уровень (минимум Min);
    /// за монстра сильнее игрока +Bonus за уровень (максимум +MaxBonus).
    /// </summary>
    public static double LevelDiffMultiplier(int playerLevel, int monsterLevel)
    {
        var cfg = RPGConfig.Instance;
        int diff = playerLevel - monsterLevel;
        if (diff < 0)
            return 1.0 + Math.Min(cfg.LevelDiffMaxBonus, -diff * cfg.LevelDiffBonusPerLevel);
        if (diff <= cfg.LevelDiffGrace)
            return 1.0;
        return Math.Max(cfg.LevelDiffMinMultiplier, 1.0 - (diff - cfg.LevelDiffGrace) * cfg.LevelDiffPenaltyPerLevel);
    }

    /// <summary>Убывающая отдача: X * n / (n + K). Без жёсткого потолка, X — асимптота.</summary>
    public static float Soft(int n, float max)
    {
        if (n <= 0)
            return 0f;
        return max * n / (n + RPGConfig.Instance.SoftCapK);
    }

    /// <summary>Все бонусы статов, посчитанные из итоговых значений (используется и игрой, и UI).</summary>
    public static StatBonuses Bonuses(int str, int dex, int intel, int spirit, int vit, int luck, int cunning = 0)
    {
        var c = RPGConfig.Instance;
        int slots = (spirit >= c.SpiritSlot1 ? 1 : 0) + (spirit >= c.SpiritSlot2 ? 1 : 0) + (spirit >= c.SpiritSlot3 ? 1 : 0);
        return new StatBonuses
        {
            MeleeDamage = Soft(str, c.StrMeleeDamage),
            RogueDamage = Systems.CalamityCompat.RogueAvailable ? Soft(cunning, c.CunningDamage) : 0f,
            RogueSpeed = Systems.CalamityCompat.RogueAvailable ? Soft(cunning, c.CunningSpeed) : 0f,
            MeleeSpeed = Soft(str, c.StrMeleeSpeed),
            RangedDamage = Soft(dex, c.DexRangedDamage),
            MoveSpeed = Soft(dex, c.DexMoveSpeed),
            AmmoSave = Soft(dex, c.DexAmmoSave),
            MagicDamage = Soft(intel, c.IntMagicDamage),
            MaxMana = Math.Min(c.IntManaCap, intel * c.IntManaPerPoint),
            ManaCost = Soft(intel, c.IntManaCost),
            MinionDamage = Soft(spirit, c.SpiritMinionDamage),
            MinionSlots = slots,
            WhipBonus = Soft(spirit, c.SpiritWhip),
            MaxLife = (int)Soft(vit, c.VitMaxLife),
            DamageReduction = Soft(vit, c.VitDamageReduction),
            LifeRegen = vit / Math.Max(1, c.VitRegenPerPoints),
            Crit = Soft(luck, c.LuckCrit),
            Luck = Soft(luck, c.LuckLuck),
        };
    }

    /// <summary>Опыт за босса из таблицы: доля xpToNext его уровня.</summary>
    public static long BossXP(int bossLevel, float share)
    {
        var cfg = RPGConfig.Instance;
        return (long)Math.Round(XPToNext(bossLevel) * cfg.BossXPFraction * share * cfg.XPMultiplier);
    }
}
