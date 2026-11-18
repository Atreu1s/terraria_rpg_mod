using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RPGMod.Common.Config;

/// <summary>
/// Все балансные числа мода. ServerSide: в мультиплеере значения берутся с сервера
/// и синхронизируются клиентам (как общий «store», который нельзя менять локально).
/// Подписи полей — в Localization/*.hjson (Configs.RPGConfig.*).
/// </summary>
public class RPGConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    /// <summary>Короткий доступ из любого места: RPGConfig.Instance.XPMultiplier.</summary>
    public static RPGConfig Instance => ModContent.GetInstance<RPGConfig>();

    // ---------- Опыт за монстров ----------
    [Header("Experience")]

    [Range(0f, 20f)]
    [Increment(0.1f)]
    [DefaultValue(1f)]
    public float XPMultiplier { get; set; } = 1f;

    // power = PowerLife * sqrt(lifeMax) + PowerDamage * damage + PowerDefense * defense
    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.6f)]
    public float PowerLife { get; set; } = 0.6f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.8f)]
    public float PowerDamage { get; set; } = 0.8f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.5f)]
    public float PowerDefense { get; set; } = 0.5f;

    // Уровень монстра = power / PowerPerLevel
    [Range(0.5f, 20f)]
    [Increment(0.1f)]
    [DefaultValue(3f)]
    public float PowerPerLevel { get; set; } = 3f;

    // Опыт = XPPowerFactor * power ^ XPPowerExponent * множители
    [Range(0f, 20f)]
    [Increment(0.1f)]
    [DefaultValue(2f)]
    public float XPPowerFactor { get; set; } = 2f;

    [Range(0.5f, 3f)]
    [Increment(0.05f)]
    [DefaultValue(1.5f)]
    public float XPPowerExponent { get; set; } = 1.5f;

    [Range(0f, 1f)]
    [Increment(0.05f)]
    [DefaultValue(0.5f)]
    public float EventMultiplier { get; set; } = 0.5f;

    // ---------- Разница уровней ----------
    [Header("LevelDifference")]

    [Range(0, 100)]
    [DefaultValue(5)]
    public int LevelDiffGrace { get; set; } = 5;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.1f)]
    public float LevelDiffPenaltyPerLevel { get; set; } = 0.1f;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.1f)]
    public float LevelDiffMinMultiplier { get; set; } = 0.1f;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.05f)]
    public float LevelDiffBonusPerLevel { get; set; } = 0.05f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.5f)]
    public float LevelDiffMaxBonus { get; set; } = 0.5f;

    // ---------- Боссы ----------
    [Header("Bosses")]

    // Доля xpToNext(уровень босса), которую даёт босс из таблицы
    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.5f)]
    public float BossXPFraction { get; set; } = 0.5f;

    // Боссы неизвестных модов: общая формула × это число
    [Range(0f, 50f)]
    [Increment(0.5f)]
    [DefaultValue(5f)]
    public float UnknownBossMultiplier { get; set; } = 5f;

    // ---------- Кривая уровней ----------
    [Header("LevelCurve")]

    // xpToNext(L) = XPCurveBase * L ^ XPCurveExponent
    [Range(1f, 10000f)]
    [Increment(5f)]
    [DefaultValue(300f)]
    public float XPCurveBase { get; set; } = 300f;

    [Range(1f, 4f)]
    [Increment(0.05f)]
    [DefaultValue(2.5f)]
    public float XPCurveExponent { get; set; } = 2.5f;

    // ---------- Класс и тиры (DESIGN §2) ----------
    [Header("Tree")]

    [Range(1, 1000)]
    [DefaultValue(10)]
    public int ClassLevel { get; set; } = 10;

    [Range(1, 1000)]
    [DefaultValue(25)]
    public int Tier1Level { get; set; } = 25;

    [Range(1, 1000)]
    [DefaultValue(50)]
    public int Tier2Level { get; set; } = 50;

    [Range(1, 1000)]
    [DefaultValue(75)]
    public int Tier3Level { get; set; } = 75;

    // ---------- Статы (DESIGN §3) ----------
    // Убывающая отдача: bonus(n) = X * n / (n + SoftCapK), X — значение «до».
    [Header("Stats")]

    [Range(0, 10)]
    [DefaultValue(1)]
    public int StatPointsPerLevel { get; set; } = 1;

    [Range(1f, 1000f)]
    [Increment(5f)]
    [DefaultValue(100f)]
    public float SoftCapK { get; set; } = 100f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.6f)]
    public float StrMeleeDamage { get; set; } = 0.6f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.2f)]
    public float StrMeleeSpeed { get; set; } = 0.2f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.6f)]
    public float DexRangedDamage { get; set; } = 0.6f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.25f)]
    public float DexMoveSpeed { get; set; } = 0.25f;

    [Range(0f, 1f)]
    [Increment(0.05f)]
    [DefaultValue(0.25f)]
    public float DexAmmoSave { get; set; } = 0.25f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.6f)]
    public float IntMagicDamage { get; set; } = 0.6f;

    [Range(0, 10)]
    [DefaultValue(1)]
    public int IntManaPerPoint { get; set; } = 1;

    [Range(0, 1000)]
    [DefaultValue(200)]
    public int IntManaCap { get; set; } = 200;

    [Range(0f, 1f)]
    [Increment(0.05f)]
    [DefaultValue(0.25f)]
    public float IntManaCost { get; set; } = 0.25f;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.6f)]
    public float SpiritMinionDamage { get; set; } = 0.6f;

    // Пороги Духа для +1 слота миньона
    [Range(1, 10000)]
    [DefaultValue(30)]
    public int SpiritSlot1 { get; set; } = 30;

    [Range(1, 10000)]
    [DefaultValue(80)]
    public int SpiritSlot2 { get; set; } = 80;

    [Range(1, 10000)]
    [DefaultValue(150)]
    public int SpiritSlot3 { get; set; } = 150;

    [Range(0f, 5f)]
    [Increment(0.05f)]
    [DefaultValue(0.3f)]
    public float SpiritWhip { get; set; } = 0.3f;

    [Range(0, 2000)]
    [DefaultValue(200)]
    public int VitMaxLife { get; set; } = 200;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.15f)]
    public float VitDamageReduction { get; set; } = 0.15f;

    // +1 регенерации за каждые N очков
    [Range(1, 1000)]
    [DefaultValue(25)]
    public int VitRegenPerPoints { get; set; } = 25;

    // Шанс крита в процентах (15 = +15%)
    [Range(0f, 100f)]
    [Increment(1f)]
    [DefaultValue(15f)]
    public float LuckCrit { get; set; } = 15f;

    [Range(0f, 2f)]
    [Increment(0.05f)]
    [DefaultValue(0.3f)]
    public float LuckLuck { get; set; } = 0.3f;

    // ---------- Отладка ----------
    [Header("Debug")]

    // Разрешает чат-команду /rpg
    [DefaultValue(false)]
    public bool DebugCommands { get; set; } = false;
}
