using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using RPGMod.Common.Data;
using RPGMod.Common.Players;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RPGMod.UI;

/// <summary>
/// HUD уровня под хотбаром (UI_SPEC §5) и всплывающие уведомления.
/// Рисуется в слое интерфейса с масштабом UI, поэтому координаты — «как при UI scale = 1».
/// </summary>
public class RPGHudSystem : ModSystem
{
    // Палитра HUD (docs/mockups/Hud.dc.html)
    public static readonly Color HudBack = new(26, 33, 72);
    public static readonly Color HudBorder = new(70, 86, 168);
    public static readonly Color BarBack = new(20, 26, 58);
    public static readonly Color BarBorder = new(58, 70, 135);
    public static readonly Color AwakenGold = new(242, 193, 78);
    public static readonly Color SecondaryText = new(179, 185, 220);
    public static readonly Color NoClassGray = new(140, 146, 170);

    private const int HudX = 20, HudY = 78, HudW = 360, HudH = 76;
    private const float ToastDuration = 4f * 60f; // кадры

    private record Toast(string Badge, string Title, string Subtitle, Color Accent)
    {
        public float Time;
    }

    private static readonly List<Toast> toasts = new();

    public static void ShowToast(string badge, string title, string subtitle, Color accent)
    {
        if (Main.dedServ)
            return;
        toasts.Add(new Toast(badge, title, subtitle, accent));
        if (toasts.Count > 3)
            toasts.RemoveAt(0);
    }

    public override void Unload() => toasts.Clear();

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name == "Vanilla: Resource Bars");
        if (index < 0)
            index = layers.Count;

        layers.Insert(index, new LegacyGameInterfaceLayer("RPGMod: Level HUD", () =>
        {
            DrawHud(Main.spriteBatch);
            return true;
        }, InterfaceScaleType.UI));
    }

    public override void UpdateUI(GameTime gameTime)
    {
        for (int i = toasts.Count - 1; i >= 0; i--)
        {
            toasts[i].Time++;
            if (toasts[i].Time >= ToastDuration)
                toasts.RemoveAt(i);
        }
    }

    private static void DrawHud(SpriteBatch sb)
    {
        if (Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active)
            return;

        // В инвентаре это место занято слотами — HUD прячем, тосты оставляем.
        int hudY = HudY + BuffRows() * 50;
        if (!Main.playerInventory)
            DrawLevelBlock(sb, hudY);
        DrawToasts(sb, hudY);
    }

    /// <summary>Баффы рисуются под хотбаром рядами по 11 — HUD сдвигаем под них.</summary>
    private static int BuffRows()
    {
        int count = 0;
        for (int i = 0; i < Player.MaxBuffs; i++)
            if (Main.LocalPlayer.buffType[i] > 0 && Main.LocalPlayer.buffTime[i] > 0)
                count++;
        return (count + 10) / 11;
    }

    private static void DrawLevelBlock(SpriteBatch sb, int hudY)
    {
        var rpg = Main.LocalPlayer.GetModPlayer<RPGPlayer>();
        DynamicSpriteFont font = FontAssets.MouseText.Value;

        // Плашка
        UIDraw.RoundedRect(sb, new Rectangle(HudX, hudY, HudW, HudH), HudBack, HudBorder, 2);

        // Круглый значок уровня
        Color classColor = ClassColor(rpg.ClassId);
        var badge = new Rectangle(HudX + 12, hudY + 12, 52, 52);
        UIDraw.Circle(sb, badge, classColor * 0.35f, classColor, 2);
        string lvLabel = Language.GetTextValue("Mods.RPGMod.HUD.LevelShort");
        UIDraw.TextCentered(sb, lvLabel, new Vector2(badge.Center.X, badge.Y + 13), Color.White, 0.6f);
        UIDraw.TextCentered(sb, rpg.Level.ToString(), new Vector2(badge.Center.X, badge.Y + 33), Color.White, 1.0f);

        // Правая часть
        int left = badge.Right + 12;
        int right = HudX + HudW - 12;
        string className = Language.GetTextValue($"Mods.RPGMod.Classes.{ClassIds.Key(rpg.ClassId)}.Name");
        if (rpg.ClassId != ClassIds.None)
        {
            // «Воин · Берсерк»: добавляем выбранный узел тира 1
            foreach (SkillNode n in SkillTree.ByClass[rpg.ClassId])
                if (n.Tier == 1 && rpg.Has(n.Id))
                    className += " · " + Language.GetTextValue($"Mods.RPGMod.Nodes.{n.Id}.Name");
        }
        UIDraw.Text(sb, className, new Vector2(left, hudY + 10), SecondaryText, 0.75f);

        long need = rpg.XPToNext;
        string xpText = Language.GetTextValue("Mods.RPGMod.HUD.XPProgress", FormatNumber(rpg.XP), FormatNumber(need));
        float xpWidth = font.MeasureString(xpText).X * 0.75f;
        UIDraw.Text(sb, xpText, new Vector2(right - xpWidth, hudY + 10), SecondaryText, 0.75f);

        // Полоса опыта 12 px
        var bar = new Rectangle(left, hudY + 32, right - left, 12);
        UIDraw.RoundedRect(sb, bar, BarBack, BarBorder, 1, radius: 6);
        float progress = need > 0 ? MathHelper.Clamp((float)rpg.XP / need, 0f, 1f) : 0f;
        int fillW = (int)((bar.Width - 2) * progress);
        if (fillW > 0)
            UIDraw.RoundedRect(sb, new Rectangle(bar.X + 1, bar.Y + 1, fillW, bar.Height - 2), AwakenGold, null, 0, radius: 5);

        // Строка очков (статы и древо появятся на этапах 2–3)
        string pointsText = Language.GetTextValue("Mods.RPGMod.HUD.Points", Math.Max(0, rpg.FreeStatPoints), rpg.TreePoints());
        UIDraw.Text(sb, pointsText, new Vector2(left, hudY + 50), SecondaryText, 0.75f);

        // Мышь над HUD — подсказка с процентом
        if (new Rectangle(HudX, hudY, HudW, HudH).Contains(Main.MouseScreen.ToPoint()))
        {
            Main.LocalPlayer.mouseInterface = true;
            Terraria.ModLoader.UI.UICommon.TooltipMouseText(Language.GetTextValue("Mods.RPGMod.HUD.XPTooltip",
                (int)(progress * 100), rpg.Level + 1));
        }
    }

    private static void DrawToasts(SpriteBatch sb, int y)
    {
        int x = HudX + HudW + 16;
        foreach (Toast t in toasts)
        {
            // Плавное появление и исчезновение (0.25 с)
            float alpha = Math.Min(1f, Math.Min(t.Time, ToastDuration - t.Time) / 15f);
            var rect = new Rectangle(x, y, 300, 60);
            UIDraw.RoundedRect(sb, rect, HudBack * alpha, t.Accent * alpha, 2);
            UIDraw.TextCentered(sb, t.Badge, new Vector2(rect.X + 30, rect.Center.Y + 2), t.Accent * alpha, 1.1f);
            UIDraw.Text(sb, t.Title, new Vector2(rect.X + 60, rect.Y + 9), Color.White * alpha, 0.9f);
            UIDraw.Text(sb, t.Subtitle, new Vector2(rect.X + 60, rect.Y + 33), SecondaryText * alpha, 0.75f);
            y += rect.Height + 8;
        }
    }

    public static Color ClassColor(int classId) => classId switch
    {
        1 => new Color(232, 96, 76),   // Воин
        2 => new Color(92, 196, 97),   // Стрелок
        3 => new Color(167, 123, 240), // Маг
        4 => new Color(60, 200, 208),  // Призыватель
        _ => NoClassGray,
    };

    /// <summary>84210 → «84 210».</summary>
    public static string FormatNumber(long n)
        => n.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', ' ');
}
