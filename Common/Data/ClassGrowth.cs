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
}

/// <summary>ID классов. 0 — без класса. Порядок важен: сохраняется числом.</summary>
public static class ClassIds
{
    public const int None = 0;
    public const int Warrior = 1;
    public const int Ranger = 2;
    public const int Mage = 3;
    public const int Summoner = 4;
    public const int Count = 5;

    /// <summary>ID корневого узла древа для класса (совпадает с ID в DESIGN §4).</summary>
    public static string RootNode(int classId) => classId switch
    {
        Warrior => "warrior",
        Ranger => "ranger",
        Mage => "mage",
        Summoner => "summoner",
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
        if (classId < 0 || classId >= table.Length)
            return 0f;
        return table[classId][(int)stat];
    }

    public static int Points(int classId, Stat stat, int level) => (int)(level * Get(classId, stat));
}
