using System.Collections.Generic;
using RPGMod.Common.Config;
using RPGMod.Common.Systems;

namespace RPGMod.Common.Data;

/// <summary>Порядок важен: это индексы в RPGPlayer.Invested и в сохранении.</summary>
public enum Stat
{
    Str = 0,     // Сила
    Dex = 1,     // Ловкость
    Int = 2,     // Интеллект
    Spirit = 3,  // Дух
    Vit = 4,     // Выносливость
    Luck = 5,    // Удача
    Cunning = 6, // Хитрость (индекс сохранения, порядок UI задаётся отдельно)
}

public static class StatIds
{
    public static IEnumerable<Stat> Visible()
    {
        yield return Stat.Str;
        yield return Stat.Dex;
        yield return Stat.Int;
        yield return Stat.Spirit;
        if (CalamityCompat.RogueAvailable) yield return Stat.Cunning;
        yield return Stat.Vit;
        yield return Stat.Luck;
    }
}

/// <summary>ID классов. 0 — без класса. Порядок важен: сохраняется числом.</summary>
public static class ClassIds
{
    public const int None = 0;
    public const int Warrior = 1;
    public const int Ranger = 2;
    public const int Mage = 3;
    public const int Summoner = 4;
    public const int Rogue = 5;
    public const int Count = 6;

    public static bool Available(int id) => id > None && id < Count
        && (id != Rogue || CalamityCompat.RogueAvailable);

    public static IEnumerable<int> AvailableClasses()
    {
        for (int id = Warrior; id < Count; id++)
            if (Available(id)) yield return id;
    }

    /// <summary>ID корневого узла древа для класса (совпадает с ID в DESIGN §4).</summary>
    public static string RootNode(int classId) => classId switch
    {
        Warrior => "warrior",
        Ranger => "ranger",
        Mage => "mage",
        Summoner => "summoner",
        Rogue => "rogue",
        _ => null,
    };

    /// <summary>Ключ для локализации и иконок.</summary>
    public static string Key(int classId) => RootNode(classId) ?? "none";
}

/// <summary>
/// Классовый прирост статов за уровень (DESIGN §3). Не хранится, а вычисляется:
/// floor(уровень × коэффициент).
/// </summary>
public static class ClassGrowth
{
    //                                   Str   Dex   Int   Spirit Vit   Luck
    private static readonly float[][] table =
    {
        new[] { 0f,   0f,   0f,   0f,    0f,   0f }, // без класса
        new[] { 0.5f, 0f,   0f,   0f,    0.35f, 0f }, // Воин
        new[] { 0f,   0.6f, 0f,   0f,    0.25f, 0f }, // Стрелок
        new[] { 0f,   0f,   0.6f, 0.1f,  0.15f, 0f }, // Маг
        new[] { 0f,   0f,   0.1f, 0.55f, 0.2f,  0f }, // Призыватель
    };

    public static float Get(int classId, Stat stat)
    {
        if (!ClassIds.Available(classId))
            return 0f;
        if (classId == ClassIds.Rogue)
        {
            var c = RPGConfig.Instance;
            return stat switch
            {
                Stat.Cunning => c.RogueCunningGrowth,
                Stat.Dex => c.RogueDexGrowth,
                Stat.Vit => c.RogueVitGrowth,
                _ => 0f,
            };
        }
        if ((int)stat < 0 || (int)stat >= table[classId].Length) return 0f;
        return table[classId][(int)stat];
    }

    public static int Points(int classId, Stat stat, int level) => (int)(level * Get(classId, stat));
}
