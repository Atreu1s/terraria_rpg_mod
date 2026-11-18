using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RPGMod.Common;
using RPGMod.Common.Data;
using RPGMod.Common.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace RPGMod.UI;

/// <summary>Панель статов 420 × 892 (UI_SPEC §3). ≈ React-компонент: рисует себя по состоянию игрока.</summary>
public class StatPanel : UIElement
{
    public const int W = 420, H = 892;
    private const int Pad = 18, RowH = 64, Gap = 10, RowsTop = 118;

    public StatPanel()
    {
        Width.Set(W, 0f);
        Height.Set(H, 0f);
        for (int i = 0; i < StatInfo.All.Length; i++)
        {
            var row = new StatRow(StatInfo.All[i]);
            row.Left.Set(Pad, 0f);
            row.Top.Set(RowsTop + i * (RowH + Gap), 0f);
            row.Width.Set(W - Pad * 2, 0f);
            row.Height.Set(RowH, 0f);
            Append(row);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var rpg = Main.LocalPlayer.GetModPlayer<RPGPlayer>();
        Rectangle r = GetDimensions().ToRectangle();
        Theme.Panel(sb, r);

        // 1. Заголовок + «Класс · ур. N»
        Theme.Title(sb, Language.GetTextValue("Mods.RPGMod.UI.StatsTitle"), new Vector2(r.X + Pad, r.Y + Pad - 2));
        string cls = Language.GetTextValue($"Mods.RPGMod.Classes.{ClassIds.Key(rpg.ClassId)}.Name");
        string right = Language.GetTextValue("Mods.RPGMod.UI.ClassLevel", cls, rpg.Level);
        float rw = Theme.MeasureWidth(right, 0.85f);
        UIDraw.Text(sb, right, new Vector2(r.Right - Pad - rw, r.Y + Pad + 10), Color.White, 0.85f);

        // 2. Плашка «Свободные очки»
        var plate = new Rectangle(r.X + Pad, r.Y + 62, r.Width - Pad * 2, 46);
        Theme.Slot(sb, plate);
        UIDraw.Text(sb, Language.GetTextValue("Mods.RPGMod.UI.FreePoints"), new Vector2(plate.X + 12, plate.Y + 13), Color.White, 0.9f);
        string free = Math.Max(0, rpg.FreeStatPoints).ToString();
        Utils.DrawBorderStringBig(sb, free, new Vector2(plate.Right - 12, plate.Center.Y + 4), Theme.Accent, 0.55f, 1f, 0.5f);
    }
}

/// <summary>Строка стата: слот с иконкой → текст → кнопка «+».</summary>
public class StatRow : UIElement
{
    private readonly Stat stat;
    private Rectangle plusRect;

    public StatRow(Stat stat)
    {
        this.stat = stat;
        OnLeftClick += Clicked;
    }

    private RPGPlayer Rpg => Main.LocalPlayer.GetModPlayer<RPGPlayer>();

    private void Clicked(UIMouseEvent evt, UIElement listening)
    {
        if (!plusRect.Contains(evt.MousePosition.ToPoint()))
            return;

        int amount = 1;
        if (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl))
            amount = int.MaxValue;
        else if (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift))
            amount = 5;

        if (Rpg.Invest(stat, amount) > 0)
            SoundEngine.PlaySound(SoundID.MenuTick);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var rpg = Rpg;
        Rectangle r = GetDimensions().ToRectangle();
        string key = StatInfo.Key(stat);

        // Слот 52×52 с иконкой
        var slot = new Rectangle(r.X, r.Y + (r.Height - 52) / 2, 52, 52);
        Theme.Slot(sb, slot);
        Theme.IconOrLetter(sb, Icons.Stat(stat), Language.GetTextValue($"Mods.RPGMod.Stats.{key}.Letter"),
            slot, Color.White, StatInfo.LetterColor(stat));

        // Кнопка «+» 44×44
        plusRect = new Rectangle(r.Right - 44, r.Y + (r.Height - 44) / 2, 44, 44);
        bool canAdd = rpg.FreeStatPoints > 0;
        float op = canAdd ? 1f : 0.45f;
        bool hoverPlus = canAdd && plusRect.Contains(Main.MouseScreen.ToPoint());
        Theme.Slot(sb, plusRect, opacity: op);
        if (hoverPlus)
            UIDraw.RoundedRect(sb, plusRect, Color.Transparent, Theme.Accent, 2, radius: 8);
        Utils.DrawBorderStringBig(sb, "+", new Vector2(plusRect.Center.X, plusRect.Center.Y + 4), Theme.Accent * op, 0.6f, 0.5f, 0.5f);

        // Текст: название / итог / две строки эффектов
        int tx = slot.Right + 12;
        int textRight = plusRect.X - 12;
        UIDraw.Text(sb, Language.GetTextValue($"Mods.RPGMod.Stats.{key}.Name"), new Vector2(tx, r.Y + 4), Color.White, 0.95f);
        string total = rpg.GetStat(stat).ToString();
        float tw = Theme.MeasureWidth(total, 1.1f);
        UIDraw.Text(sb, total, new Vector2(textRight - tw, r.Y + 1), Color.White, 1.1f);

        var (line1, line2) = EffectLines(stat, rpg.Bonuses);
        UIDraw.Text(sb, line1, new Vector2(tx, r.Y + 26), Theme.TextSecondary, 0.75f);
        UIDraw.Text(sb, line2, new Vector2(tx, r.Y + 44), Theme.TextSecondary, 0.75f);

        // Подсказка при наведении на строку (кроме кнопки)
        if (IsMouseHovering && !plusRect.Contains(Main.MouseScreen.ToPoint()))
        {
            string tip = Language.GetTextValue($"Mods.RPGMod.Stats.{key}.Name") + "\n"
                + Language.GetTextValue("Mods.RPGMod.UI.StatBreakdown", rpg.ClassPoints(stat), rpg.Invested[(int)stat]) + "\n"
                + line1 + "\n" + line2 + "\n"
                + Language.GetTextValue("Mods.RPGMod.UI.StatHint");
            UICommon.TooltipMouseText(tip);
        }
    }

    private static string Pct(float v) => (v * 100f).ToString("0.#");

    public static (string, string) EffectLines(Stat s, StatBonuses b)
    {
        string k = $"Mods.RPGMod.Stats.{StatInfo.Key(s)}";
        return s switch
        {
            Stat.Str => (Language.GetTextValue(k + ".Line1", Pct(b.MeleeDamage)),
                         Language.GetTextValue(k + ".Line2", Pct(b.MeleeSpeed))),
            Stat.Dex => (Language.GetTextValue(k + ".Line1", Pct(b.RangedDamage)),
                         Language.GetTextValue(k + ".Line2", Pct(b.MoveSpeed), Pct(b.AmmoSave))),
            Stat.Int => (Language.GetTextValue(k + ".Line1", Pct(b.MagicDamage)),
                         Language.GetTextValue(k + ".Line2", b.MaxMana, Pct(b.ManaCost))),
            Stat.Spirit => (Language.GetTextValue(k + ".Line1", Pct(b.MinionDamage)),
                            Language.GetTextValue(k + ".Line2", b.MinionSlots, Pct(b.WhipBonus))),
            Stat.Vit => (Language.GetTextValue(k + ".Line1", b.MaxLife),
                         Language.GetTextValue(k + ".Line2", Pct(b.DamageReduction), b.LifeRegen)),
            _ => (Language.GetTextValue(k + ".Line1", b.Crit.ToString("0.#")),
                  Language.GetTextValue(k + ".Line2", b.Luck.ToString("0.00"))),
        };
    }
}
