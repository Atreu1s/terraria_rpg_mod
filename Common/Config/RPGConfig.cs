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

    // ---------- Rogue (DESIGN_V2 sections 4-5) ----------
    [Header("Rogue")]
    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.6f)]
    public float CunningDamage { get; set; } = 0.6f;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.2f)]
    public float CunningSpeed { get; set; } = 0.2f;

    [Range(0f, 2f)]
    [Increment(0.01f)]
    [DefaultValue(0.55f)]
    public float RogueCunningGrowth { get; set; } = 0.55f;

    [Range(0f, 2f)]
    [Increment(0.01f)]
    [DefaultValue(0.1f)]
    public float RogueDexGrowth { get; set; } = 0.1f;

    [Range(0f, 2f)]
    [Increment(0.01f)]
    [DefaultValue(0.2f)]
    public float RogueVitGrowth { get; set; } = 0.2f;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.05f)]
    public float RogueRootDamage { get; set; } = 0.05f;

    [Range(0f, 100f)]
    [Increment(0.01f)]
    [DefaultValue(5f)]
    public float RogueRootCrit { get; set; } = 5f;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.12f)]
    public float AssassinDamage { get; set; } = 0.12f;

    [Range(0f, 100f)]
    [Increment(0.01f)]
    [DefaultValue(6f)]
    public float AssassinCrit { get; set; } = 6f;

    [Range(0f, 0.9f)]
    [Increment(0.01f)]
    [DefaultValue(0.1f)]
    public float AssassinLifePenalty { get; set; } = 0.1f;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.15f)]
    public float TricksterAttackSpeed { get; set; } = 0.15f;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.1f)]
    public float TricksterMoveSpeed { get; set; } = 0.1f;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.06f)]
    public float TricksterDamagePenalty { get; set; } = 0.06f;

    [Range(1, 600)]
    [DefaultValue(120)]
    public int ShadowDelay { get; set; } = 120;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.5f)]
    public float AmbushDamage { get; set; } = 0.5f;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.25f)]
    public float ShadowDamage { get; set; } = 0.25f;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.03f)]
    public float JugglerSpeed { get; set; } = 0.03f;

    [Range(1, 600)]
    [DefaultValue(180)]
    public int JugglerDuration { get; set; } = 180;

    [Range(1, 20)]
    [DefaultValue(5)]
    public int JugglerMaxStacks { get; set; } = 5;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.1f)]
    public float EvasionChance { get; set; } = 0.1f;

    [Range(1, 1800)]
    [DefaultValue(300)]
    public int EvasionCooldown { get; set; } = 300;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.3f)]
    public float ExecutionerDamage { get; set; } = 0.3f;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.3f)]
    public float ExecutionerThreshold { get; set; } = 0.3f;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.1f)]
    public float FirstBloodDamage { get; set; } = 0.1f;

    [Range(1, 600)]
    [DefaultValue(300)]
    public int FirstBloodDuration { get; set; } = 300;

    [Range(1f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(1.25f)]
    public float DuskBladeCritMultiplier { get; set; } = 1.25f;

    [Range(0, 200)]
    [DefaultValue(15)]
    public int DuskBladeArmorPenetration { get; set; } = 15;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.4f)]
    public float RicochetDamage { get; set; } = 0.4f;

    [Range(1, 100)]
    [DefaultValue(15)]
    public int RicochetRangeTiles { get; set; } = 15;

    [Range(1, 600)]
    [DefaultValue(20)]
    public int RicochetInterval { get; set; } = 20;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.15f)]
    public float BravadoMoveSpeed { get; set; } = 0.15f;

    [Range(0, 100)]
    [DefaultValue(10)]
    public int BravadoCrit { get; set; } = 10;

    [Range(1, 600)]
    [DefaultValue(90)]
    public int SmokeBombImmunity { get; set; } = 90;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.4f)]
    public float SmokeBombDamage { get; set; } = 0.4f;

    [Range(1, 1000)]
    [DefaultValue(20)]
    public int LuckyBreakLuckPerStep { get; set; } = 20;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.01f)]
    public float LuckyBreakChancePerStep { get; set; } = 0.01f;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.1f)]
    public float LuckyBreakMaxChance { get; set; } = 0.1f;

    [Range(0f, 1f)]
    [Increment(0.01f)]
    [DefaultValue(0.05f)]
    public float LuckyBreakHeal { get; set; } = 0.05f;

    [Range(1, 100)]
    [DefaultValue(8)]
    public int RogueAwakeningHits { get; set; } = 8;

    [Range(1, 100)]
    [DefaultValue(5)]
    public int TricksterAwakeningHits { get; set; } = 5;

    [Range(1, 9)]
    [DefaultValue(3)]
    public int RogueAwakeningDaggers { get; set; } = 3;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(0.6f)]
    public float RogueAwakeningDamage { get; set; } = 0.6f;

    [Range(0f, 5f)]
    [Increment(0.01f)]
    [DefaultValue(1f)]
    public float AssassinAwakeningDamage { get; set; } = 1f;

    // ---------- Отладка ----------
    [Header("Debug")]

    // Разрешает чат-команду /rpg
    [DefaultValue(false)]
    public bool DebugCommands { get; set; } = false;
}
