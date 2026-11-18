using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RPGMod.Common.Config;

/// <summary>Личные настройки интерфейса (у каждого игрока свои).</summary>
public class RPGClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public static RPGClientConfig Instance => ModContent.GetInstance<RPGClientConfig>();

    // Позиция кнопки «Статус» в экране инвентаря (в пикселях при UI scale = 1).
    // По умолчанию — под колонкой патронов, справа от мусорного слота.
    [Range(0, 3000)]
    [DefaultValue(534)]
    public int StatusButtonX { get; set; } = 534;

    [Range(0, 3000)]
    [DefaultValue(262)]
    public int StatusButtonY { get; set; } = 262;
}
