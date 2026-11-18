using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace RPGMod.UI;

/// <summary>Палитра варианта A (UI_SPEC §2) и общие «компоненты» рисования.</summary>
public static class Theme
{
    public static readonly Color PanelBack = new Color(47, 58, 150) * 0.84f;
    public static readonly Color PanelBorder = new(159, 176, 255);
    public static readonly Color SlotBack = new Color(63, 82, 190) * 0.95f;
    public static readonly Color SlotBorder = new(184, 196, 255);
    public static readonly Color DarkSlotBack = new Color(25, 30, 80) * 0.92f;
    public static readonly Color DarkSlotBorder = new(93, 107, 184);
    public static readonly Color TextSecondary = new(227, 232, 255);
    public static readonly Color Accent = new(255, 224, 102);
    public static readonly Color Confirm = new(63, 155, 59);
    public static readonly Color LineBack = new(27, 34, 102);
    public static readonly Color LockedTint = new(154, 166, 230);
    public static readonly Color LockedLabel = new(195, 203, 245);

    public static void Panel(SpriteBatch sb, Rectangle r)
        => UIDraw.RoundedRect(sb, r, PanelBack, PanelBorder, 2, radius: 8);

    public static void Slot(SpriteBatch sb, Rectangle r, bool dark = false, float opacity = 1f)
        => UIDraw.RoundedRect(sb, r,
            (dark ? DarkSlotBack : SlotBack) * opacity,
            (dark ? DarkSlotBorder : SlotBorder) * opacity, 2, radius: 8);

    /// <summary>Крупный заголовок (DeathText, как в ванильных меню).</summary>
    public static void Title(SpriteBatch sb, string text, Vector2 pos, float scale = 0.6f)
        => Utils.DrawBorderStringBig(sb, text, pos, Color.White, scale);

    public static float MeasureWidth(string text, float scale)
        => FontAssets.MouseText.Value.MeasureString(text).X * scale;

    /// <summary>Иконка 32×32 по центру слота (натуральный размер) или буква-заглушка.</summary>
    public static void IconOrLetter(SpriteBatch sb, Texture2D icon, string letter, Rectangle slot, Color tint, Color letterColor)
    {
        if (icon != null)
        {
            var pos = new Vector2(slot.Center.X - icon.Width / 2, slot.Center.Y - icon.Height / 2);
            sb.Draw(icon, pos, tint);
        }
        else
        {
            Utils.DrawBorderStringBig(sb, letter, new Vector2(slot.Center.X, slot.Center.Y + 4), letterColor, 0.55f, 0.5f, 0.5f);
        }
    }
}
