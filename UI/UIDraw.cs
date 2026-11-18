using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace RPGMod.UI;

/// <summary>
/// Маленькая библиотека рисования для UI: скруглённые плашки, круги, текст с обводкой.
/// Текстуры углов генерируются в коде (не PNG), кэшируются по (радиус, толщина).
/// </summary>
public class UIDraw : ModSystem
{
    private static readonly Dictionary<(int r, int t), Texture2D> rings = new();
    private static readonly Dictionary<int, Texture2D> discs = new();

    private static Texture2D Pixel => TextureAssets.MagicPixel.Value;

    public override void Unload()
    {
        var all = new List<Texture2D>();
        all.AddRange(rings.Values);
        all.AddRange(discs.Values);
        rings.Clear();
        discs.Clear();
        Main.QueueMainThreadAction(() =>
        {
            foreach (var tex in all)
                tex.Dispose();
        });
    }

    /// <summary>Скруглённый прямоугольник: заливка + (опционально) рамка.</summary>
    public static void RoundedRect(SpriteBatch sb, Rectangle rect, Color fill, Color? border, int thickness, int radius = 10)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;
        int r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));

        if (fill.A > 0)
            FillRounded(sb, rect, r, fill);
        if (border is Color b && thickness > 0 && b.A > 0)
            BorderRounded(sb, rect, r, Math.Min(thickness, Math.Max(1, r)), b);
    }

    /// <summary>Круг, вписанный в квадрат rect.</summary>
    public static void Circle(SpriteBatch sb, Rectangle rect, Color fill, Color border, int thickness)
        => RoundedRect(sb, rect, fill, border, thickness, Math.Min(rect.Width, rect.Height) / 2);

    /// <summary>Прямоугольник без скругления (для слотов в стиле инвентаря).</summary>
    public static void Box(SpriteBatch sb, Rectangle rect, Color fill, Color? border, int thickness)
    {
        if (fill.A > 0)
            sb.Draw(Pixel, rect, fill);
        if (border is Color b && thickness > 0)
        {
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), b);
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), b);
            sb.Draw(Pixel, new Rectangle(rect.X, rect.Y + thickness, thickness, rect.Height - 2 * thickness), b);
            sb.Draw(Pixel, new Rectangle(rect.Right - thickness, rect.Y + thickness, thickness, rect.Height - 2 * thickness), b);
        }
    }

    public static void Text(SpriteBatch sb, string text, Vector2 pos, Color color, float scale = 1f)
        => Utils.DrawBorderString(sb, text, pos, color, scale);

    /// <summary>Текст, центрированный по точке.</summary>
    public static void TextCentered(SpriteBatch sb, string text, Vector2 center, Color color, float scale = 1f)
        => Utils.DrawBorderString(sb, text, center, color, scale, 0.5f, 0.5f);

    // ---------- внутреннее ----------

    private static void FillRounded(SpriteBatch sb, Rectangle rc, int r, Color c)
    {
        if (r == 0)
        {
            sb.Draw(Pixel, rc, c);
            return;
        }
        // центр и боковые полосы
        sb.Draw(Pixel, new Rectangle(rc.X + r, rc.Y, rc.Width - 2 * r, rc.Height), c);
        sb.Draw(Pixel, new Rectangle(rc.X, rc.Y + r, r, rc.Height - 2 * r), c);
        sb.Draw(Pixel, new Rectangle(rc.Right - r, rc.Y + r, r, rc.Height - 2 * r), c);
        DrawCorners(sb, Disc(r), rc, r, c);
    }

    private static void BorderRounded(SpriteBatch sb, Rectangle rc, int r, int t, Color c)
    {
        if (r == 0)
        {
            Box(sb, rc, Color.Transparent, c, t);
            return;
        }
        sb.Draw(Pixel, new Rectangle(rc.X + r, rc.Y, rc.Width - 2 * r, t), c);
        sb.Draw(Pixel, new Rectangle(rc.X + r, rc.Bottom - t, rc.Width - 2 * r, t), c);
        sb.Draw(Pixel, new Rectangle(rc.X, rc.Y + r, t, rc.Height - 2 * r), c);
        sb.Draw(Pixel, new Rectangle(rc.Right - t, rc.Y + r, t, rc.Height - 2 * r), c);
        DrawCorners(sb, Ring(r, t), rc, r, c);
    }

    /// <summary>Рисует 4 четверти текстуры 2r×2r в углы прямоугольника.</summary>
    private static void DrawCorners(SpriteBatch sb, Texture2D tex, Rectangle rc, int r, Color c)
    {
        sb.Draw(tex, new Rectangle(rc.X, rc.Y, r, r), new Rectangle(0, 0, r, r), c);
        sb.Draw(tex, new Rectangle(rc.Right - r, rc.Y, r, r), new Rectangle(r, 0, r, r), c);
        sb.Draw(tex, new Rectangle(rc.X, rc.Bottom - r, r, r), new Rectangle(0, r, r, r), c);
        sb.Draw(tex, new Rectangle(rc.Right - r, rc.Bottom - r, r, r), new Rectangle(r, r, r, r), c);
    }

    private static Texture2D Disc(int r)
    {
        if (!discs.TryGetValue(r, out var tex))
            discs[r] = tex = Generate(r, (dist) => dist <= r);
        return tex;
    }

    private static Texture2D Ring(int r, int t)
    {
        if (!rings.TryGetValue((r, t), out var tex))
            rings[(r, t)] = tex = Generate(r, (dist) => dist <= r && dist > r - t);
        return tex;
    }

    /// <summary>Белая текстура 2r×2r: пиксель закрашен, если inside(расстояние от центра).</summary>
    private static Texture2D Generate(int r, Func<float, bool> inside)
    {
        int size = r * 2;
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x + 0.5f - r, dy = y + 0.5f - r;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            data[y * size + x] = inside(dist) ? Color.White : Color.Transparent;
        }
        var tex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
        tex.SetData(data);
        return tex;
    }
}
