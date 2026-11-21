namespace RPGMod.Common;

/// <summary>Результат формул статов. Доли (0.12 = +12%), кроме явно целых полей.</summary>
public struct StatBonuses
{
    public float MeleeDamage, MeleeSpeed;
    public float RogueDamage, RogueSpeed;
    public float RangedDamage, MoveSpeed, AmmoSave;
    public float MagicDamage, ManaCost;
    public int MaxMana;
    public float MinionDamage, WhipBonus;
    public int MinionSlots;
    public int MaxLife, LifeRegen;
    public float DamageReduction;
    /// <summary>Шанс крита в процентных пунктах (5 = +5%).</summary>
    public float Crit;
    public float Luck;
}
