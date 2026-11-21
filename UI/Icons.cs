using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using RPGMod.Common.Data;
using Terraria.ModLoader;

namespace RPGMod.UI;

/// <summary>
/// Доступ к иконкам из Assets/. Если PNG нет (Codex ещё не нарисовал) — возвращает null,
/// и вызывающий код рисует заглушку-букву. Мод не падает.
/// </summary>
public static class Icons
{
    private static readonly Dictionary<string, Asset<Texture2D>> cache = new();

    public static Texture2D Get(string path)
    {
        if (!cache.TryGetValue(path, out var asset))
        {
            asset = ModContent.HasAsset(path)
                ? ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad)
                : null;
            cache[path] = asset;
        }
        return asset?.Value;
    }

    public static Texture2D Skill(string nodeId) => Get($"RPGMod/Assets/SkillIcons/{nodeId}");

    public static Texture2D Stat(Stat s) => Get($"RPGMod/Assets/StatIcons/stat_{StatInfo.Key(s)}");

    public static void Clear() => cache.Clear();
}

/// <summary>Справочные данные по статам для UI.</summary>
public static class StatInfo
{
    /// <summary>Ключ для иконок и локализации (совпадает с docs/icons.json).</summary>
    public static string Key(Stat s) => s switch
    {
        Stat.Str => "str",
        Stat.Dex => "agi",
        Stat.Int => "int",
        Stat.Spirit => "spi",
        Stat.Cunning => "cun",
        Stat.Vit => "vit",
        _ => "luck",
    };

    /// <summary>Цвет буквы-заглушки (UI_SPEC §2).</summary>
    public static Color LetterColor(Stat s) => s switch
    {
        Stat.Str => new Color(255, 138, 122),
        Stat.Dex => new Color(143, 240, 143),
        Stat.Int => new Color(201, 168, 255),
        Stat.Spirit => new Color(127, 240, 245),
        Stat.Cunning => new Color(212, 107, 176),
        Stat.Vit => new Color(255, 194, 122),
        _ => new Color(255, 224, 102),
    };
}
