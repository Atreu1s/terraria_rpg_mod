using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RPGMod.Common.Data;
using RPGMod.Common.Players;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RPGMod.UI;

public class AwakeningHud : ModSystem
{
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Resource Bars");
        if (index < 0) index = layers.Count;
        layers.Insert(index, new LegacyGameInterfaceLayer("RPGMod: Fervor", Draw, InterfaceScaleType.Game));
    }

    private static bool Draw()
    {
        if (Main.gameMenu || Main.LocalPlayer.dead) return true;
        var rpg = Main.LocalPlayer.GetModPlayer<RPGPlayer>();
        if (!rpg.Awakened || rpg.ClassId != ClassIds.Warrior) return true;
        var awakening = Main.LocalPlayer.GetModPlayer<AwakeningPlayer>();
        Vector2 position = Main.LocalPlayer.Bottom - Main.screenPosition + new Vector2(-40f, 12f);
        var bar = new Rectangle((int)position.X, (int)position.Y, 80, 8);
        UIDraw.Box(Main.spriteBatch, bar, new Color(26, 33, 72), new Color(70, 86, 168), 1);
        float fraction = awakening.FervorProgress;
        int width = (int)((bar.Width - 2) * fraction);
        if (width > 0) UIDraw.Box(Main.spriteBatch, new Rectangle(bar.X + 1, bar.Y + 1, width, bar.Height - 2), new Color(242, 193, 78), null, 0);
        UIDraw.TextCentered(Main.spriteBatch, Language.GetTextValue("Mods.RPGMod.Awakening2.Fervor"), new Vector2(bar.Center.X, bar.Bottom + 9), Color.White, .6f);
        return true;
    }
}
