using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGMod.Common.Data;
using RPGMod.Common.Players;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace RPGMod.UI;

/// <summary>
/// Панель древа ≈1172 × 892 (UI_SPEC §4): область узлов 860 × 720 и панель описания справа.
/// Выбор узла — только кнопкой в описании, клик по узлу лишь выделяет его.
/// </summary>
public class TreePanel : UIElement
{
    public const int W = 1172, H = 892;
    private const int Pad = 18, AreaW = 860, AreaH = 720, Node = 52;
    private const int AreaTop = 92; // заголовок + подписи колонок

    // Центры узлов внутри области 860×720 по индексу кучи (UI_SPEC §4)
    private static readonly Vector2[] centers = BuildCenters();

    private static readonly Color GoldGlow = new(242, 193, 78);

    private string selectedId;   // выделенный узел (класс выбран)
    private int selectedClass;   // выделенный класс (класс не выбран)
    private readonly List<(Rectangle Rect, Action Action)> clickables = new();

    public TreePanel()
    {
        Width.Set(W, 0f);
        Height.Set(H, 0f);
        OnLeftClick += (evt, _) =>
        {
            Point mouse = evt.MousePosition.ToPoint();
            // Последние нарисованные — сверху, поэтому идём с конца.
            for (int i = clickables.Count - 1; i >= 0; i--)
            {
                if (clickables[i].Rect.Contains(mouse))
                {
                    clickables[i].Action();
                    break;
                }
            }
        };
    }

    private static Vector2[] BuildCenters()
    {
        var c = new Vector2[15];
        c[0] = new Vector2(70, 360);
        c[1] = new Vector2(250, 180); c[2] = new Vector2(250, 540);
        float[] t2 = { 90, 270, 450, 630 };
        for (int i = 0; i < 4; i++) c[3 + i] = new Vector2(440, t2[i]);
        for (int i = 0; i < 8; i++) c[7 + i] = new Vector2(620, 45 + 90 * i);
        return c;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }

    private static RPGPlayer Rpg => Main.LocalPlayer.GetModPlayer<RPGPlayer>();

    protected override void DrawSelf(SpriteBatch sb)
    {
        clickables.Clear();
        var rpg = Rpg;
        Rectangle r = GetDimensions().ToRectangle();
        Theme.Panel(sb, r);

        // Заголовок и очки древа
        string cls = Language.GetTextValue($"Mods.RPGMod.Classes.{ClassIds.Key(rpg.ClassId)}.Name");
        string title = rpg.ClassId == ClassIds.None
            ? Language.GetTextValue("Mods.RPGMod.UI.TreeTitleNoClass")
            : Language.GetTextValue("Mods.RPGMod.UI.TreeTitle", cls);
        Theme.Title(sb, title, new Vector2(r.X + Pad, r.Y + Pad - 2));

        string points = rpg.TreePoints().ToString();
        float pw = Theme.MeasureWidth(points, 1.1f);
        UIDraw.Text(sb, points, new Vector2(r.Right - Pad - pw, r.Y + Pad + 6), Theme.Accent, 1.1f);
        string pointsLabel = Language.GetTextValue("Mods.RPGMod.UI.TreePoints");
        UIDraw.Text(sb, pointsLabel, new Vector2(r.Right - Pad - pw - 8 - Theme.MeasureWidth(pointsLabel, 0.9f), r.Y + Pad + 9), Color.White, 0.9f);

        var area = new Rectangle(r.X + Pad, r.Y + AreaTop, AreaW, AreaH);
        var aside = new Rectangle(area.Right + 24, r.Y + 70, r.Right - Pad - (area.Right + 24), r.Bottom - Pad - (r.Y + 70));
        UIDraw.RoundedRect(sb, aside, new Color(30, 36, 100) * 0.85f, Theme.SlotBorder, 2, radius: 8);

        if (rpg.ClassId == ClassIds.None)
        {
            DrawClassChoice(sb, rpg, area);
            DrawClassDescription(sb, rpg, aside);
        }
        else
        {
            DrawColumnLabels(sb, area);
            DrawTree(sb, rpg, area);
            DrawNodeDescription(sb, rpg, aside);
        }
    }

    // ================= Класс не выбран =================

    private void DrawClassChoice(SpriteBatch sb, RPGPlayer rpg, Rectangle area)
    {
        const int big = 88, gap = 110;
        int total = 4 * big + 3 * gap;
        int x0 = area.X + (area.Width - total) / 2;
        int y = area.Y + area.Height / 2 - big / 2 - 40;

        UIDraw.TextCentered(sb, Language.GetTextValue("Mods.RPGMod.UI.SelectClassHint"),
            new Vector2(area.Center.X, y - 60), Theme.TextSecondary, 0.9f);
        if (!rpg.CanPickClass())
            UIDraw.TextCentered(sb, Language.GetTextValue("Mods.RPGMod.UI.ClassFromLevel", RPGPlayer.TierLevel(0)),
                new Vector2(area.Center.X, y - 30), Theme.Accent, 0.9f);

        for (int i = 0; i < 4; i++)
        {
            int classId = ClassIds.Warrior + i;
            string key = ClassIds.Key(classId);
            var slot = new Rectangle(x0 + i * (big + gap), y, big, big);
            Color col = RPGHudSystem.ClassColor(classId);
            bool hover = slot.Contains(Main.MouseScreen.ToPoint());

            Theme.Slot(sb, slot);
            UIDraw.RoundedRect(sb, slot, Color.Transparent, hover ? Color.White : col, 2, radius: 8);
            if (selectedClass == classId)
                UIDraw.RoundedRect(sb, Inflate(slot, 6), Color.Transparent, Theme.Accent, 2, radius: 12);

            string name = Language.GetTextValue($"Mods.RPGMod.Classes.{key}.Name");
            Theme.IconOrLetter(sb, Icons.Skill(key), name.Substring(0, 1), slot, Color.White, col);
            UIDraw.TextCentered(sb, name, new Vector2(slot.Center.X, slot.Bottom + 18), col, 0.95f);

            clickables.Add((slot, () =>
            {
                selectedClass = classId;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }));
        }
    }

    private void DrawClassDescription(SpriteBatch sb, RPGPlayer rpg, Rectangle aside)
    {
        int x = aside.X + 14, w = aside.Width - 28, y = aside.Y + 14;
        if (selectedClass == ClassIds.None)
        {
            DrawWrapped(sb, Language.GetTextValue("Mods.RPGMod.UI.SelectClassHint"), x, ref y, w, Theme.TextSecondary, 0.8f);
            return;
        }

        string key = ClassIds.Key(selectedClass);
        UIDraw.Text(sb, Language.GetTextValue("Mods.RPGMod.UI.ClassTierLabel"), new Vector2(x, y), Theme.TextSecondary, 0.7f);
        y += 22;
        DrawWrapped(sb, Language.GetTextValue($"Mods.RPGMod.Classes.{key}.Name"), x, ref y, w, Theme.Accent, 1.05f);
        y += 6;
        DrawWrapped(sb, Language.GetTextValue($"Mods.RPGMod.Nodes.{key}.Effect"), x, ref y, w, Color.White, 0.8f);
        y += 8;
        DrawWrapped(sb, Language.GetTextValue($"Mods.RPGMod.Classes.{key}.Growth"), x, ref y, w, Theme.TextSecondary, 0.75f);
        y += 8;
        DrawWrapped(sb, Language.GetTextValue("Mods.RPGMod.UI.Requirement",
            Language.GetTextValue("Mods.RPGMod.UI.ClassRequirement", RPGPlayer.TierLevel(0))), x, ref y, w, Theme.TextSecondary, 0.7f);
        y += 12;

        int cls = selectedClass;
        DrawConfirmButton(sb, new Rectangle(x, y, w, 44), Language.GetTextValue("Mods.RPGMod.UI.PickClass"), rpg.CanPickClass(), () =>
        {
            Rpg.PickClass(cls);
            selectedClass = ClassIds.None;
            SoundEngine.PlaySound(SoundID.Item4);
        });
    }

    // ================= Древо =================

    private static void DrawColumnLabels(SpriteBatch sb, Rectangle area)
    {
        int y = area.Y - 18;
        UIDraw.TextCentered(sb, Language.GetTextValue("Mods.RPGMod.UI.ColumnClass"), new Vector2(area.X + 70, y), Theme.TextSecondary, 0.75f);
        for (int t = 1; t <= 3; t++)
        {
            float cx = area.X + centers[t == 1 ? 1 : t == 2 ? 3 : 7].X;
            string label = Language.GetTextValue("Mods.RPGMod.UI.ColumnTier", Roman(t), RPGPlayer.TierLevel(t));
            UIDraw.TextCentered(sb, label, new Vector2(cx, y), Theme.TextSecondary, 0.75f);
        }
    }

    private void DrawTree(SpriteBatch sb, RPGPlayer rpg, Rectangle area)
    {
        SkillNode[] nodes = SkillTree.ByClass[rpg.ClassId];
        var states = new RPGPlayer.PickState[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
            states[i] = rpg.GetState(nodes[i]);

        // 1. Фон всех связей
        for (int i = 1; i < nodes.Length; i++)
            DrawEdge(sb, area, (i - 1) / 2, i, Theme.LineBack, 4, dashed: false);
        // 2. Выбранный путь и доступные переходы поверх
        for (int i = 1; i < nodes.Length; i++)
        {
            if (states[i] == RPGPlayer.PickState.Picked)
                DrawEdge(sb, area, (i - 1) / 2, i, Theme.Accent, 4, dashed: false);
            else if (states[i] == RPGPlayer.PickState.Available)
                DrawEdge(sb, area, (i - 1) / 2, i, Color.White, 3, dashed: true);
        }

        // 3. Узлы
        SkillNode hovered = null;
        for (int i = 0; i < nodes.Length; i++)
        {
            Rectangle slot = NodeRect(area, i);
            if (DrawNode(sb, rpg, nodes[i], states[i], slot))
                hovered = nodes[i];

            string id = nodes[i].Id;
            clickables.Add((slot, () =>
            {
                selectedId = id;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }));
        }

        if (hovered != null)
            UICommon.TooltipMouseText(NodeTooltip(rpg, hovered, rpg.GetState(hovered)));
    }

    /// <summary>Рисует узел; возвращает true, если мышь над ним.</summary>
    private bool DrawNode(SpriteBatch sb, RPGPlayer rpg, SkillNode node, RPGPlayer.PickState state, Rectangle slot)
    {
        bool awakened = node.Tier == 3 && state == RPGPlayer.PickState.Picked && rpg.Awakened;
        bool closed = state is RPGPlayer.PickState.Locked or RPGPlayer.PickState.BranchClosed;
        float op = state == RPGPlayer.PickState.BranchClosed ? 0.45f : 1f;
        float time = (float)Main.timeForVisualEffects / 60f;

        // Свечение: пульс у доступных (~1.4 с), золото у пробуждённого
        if (state == RPGPlayer.PickState.Available)
        {
            float pulse = (MathF.Sin(time * MathHelper.TwoPi / 1.4f) + 1f) / 2f;
            UIDraw.RoundedRect(sb, Inflate(slot, 3 + (int)(pulse * 4)), Color.White * (0.15f + 0.25f * pulse), null, 0, radius: 12);
        }
        else if (awakened)
        {
            float pulse = (MathF.Sin(time * MathHelper.TwoPi / 2f) + 1f) / 2f;
            UIDraw.RoundedRect(sb, Inflate(slot, 5), GoldGlow * (0.3f + 0.2f * pulse), null, 0, radius: 12);
        }

        Color border = state switch
        {
            RPGPlayer.PickState.Picked => awakened ? GoldGlow : Theme.Accent,
            RPGPlayer.PickState.Available => Color.White,
            _ => Theme.DarkSlotBorder,
        };
        UIDraw.RoundedRect(sb, slot, (closed ? Theme.DarkSlotBack : Theme.SlotBack) * op, border * op, 2, radius: 8);

        string name = Language.GetTextValue($"Mods.RPGMod.Nodes.{node.Id}.Name");
        Color tint = (closed ? Theme.LockedTint : Color.White) * op;
        Theme.IconOrLetter(sb, Icons.Skill(node.Id), name.Substring(0, 1), slot, tint, tint);

        if (node.Tier > 0)
            Utils.DrawBorderString(sb, Roman(node.Tier), new Vector2(slot.Right - 4, slot.Bottom - 1), Theme.Accent * op, 0.55f, 1f, 1f);

        if (selectedId == node.Id)
            UIDraw.RoundedRect(sb, Inflate(slot, 6), Color.Transparent, Theme.Accent, 2, radius: 12);

        // Подпись: у тира 3 — справа, у остальных — под узлом
        Color labelColor = (closed ? Theme.LockedLabel : Color.White) * op;
        if (node.Tier == 3)
            UIDraw.Text(sb, name, new Vector2(slot.X + 66, slot.Y + 15), labelColor, 0.8f);
        else
            UIDraw.TextCentered(sb, name, new Vector2(slot.Center.X, slot.Bottom + 16), labelColor, 0.8f);

        return slot.Contains(Main.MouseScreen.ToPoint());
    }

    private static string NodeTooltip(RPGPlayer rpg, SkillNode node, RPGPlayer.PickState state)
    {
        string cls = Language.GetTextValue($"Mods.RPGMod.Classes.{ClassIds.Key(node.ClassId)}.Name");
        string tier = node.Tier == 0
            ? Language.GetTextValue("Mods.RPGMod.UI.ClassTierLabel")
            : Language.GetTextValue("Mods.RPGMod.UI.TierLabel", Roman(node.Tier), cls);
        string text = Language.GetTextValue($"Mods.RPGMod.Nodes.{node.Id}.Name") + "\n"
            + tier + "\n"
            + Language.GetTextValue($"Mods.RPGMod.Nodes.{node.Id}.Effect") + "\n"
            + StatusText(rpg, node, state);
        if (node.Tier > 0)
            text += "\n" + Language.GetTextValue("Mods.RPGMod.UI.Requirement", RequirementText(node.Tier));
        return text;
    }

    private void DrawNodeDescription(SpriteBatch sb, RPGPlayer rpg, Rectangle aside)
    {
        int x = aside.X + 14, w = aside.Width - 28, y = aside.Y + 14;
        SkillNode node = SkillTree.Get(selectedId);
        if (node == null || node.ClassId != rpg.ClassId)
        {
            selectedId = null;
            DrawWrapped(sb, Language.GetTextValue("Mods.RPGMod.UI.SelectNodeHint"), x, ref y, w, Theme.TextSecondary, 0.8f);
        }
        else
        {
            var state = rpg.GetState(node);
            string cls = Language.GetTextValue($"Mods.RPGMod.Classes.{ClassIds.Key(node.ClassId)}.Name");
            string tier = node.Tier == 0
                ? Language.GetTextValue("Mods.RPGMod.UI.ClassTierLabel")
                : Language.GetTextValue("Mods.RPGMod.UI.TierLabel", Roman(node.Tier), cls);

            UIDraw.Text(sb, tier, new Vector2(x, y), Theme.TextSecondary, 0.7f);
            y += 22;
            DrawWrapped(sb, Language.GetTextValue($"Mods.RPGMod.Nodes.{node.Id}.Name"), x, ref y, w, Theme.Accent, 1.05f);
            y += 6;
            DrawWrapped(sb, Language.GetTextValue($"Mods.RPGMod.Nodes.{node.Id}.Effect"), x, ref y, w, Color.White, 0.8f);
            y += 10;
            DrawWrapped(sb, StatusText(rpg, node, state), x, ref y, w, Theme.TextSecondary, 0.75f);
            if (node.Tier > 0)
            {
                y += 4;
                DrawWrapped(sb, Language.GetTextValue("Mods.RPGMod.UI.Requirement", RequirementText(node.Tier)), x, ref y, w, Theme.TextSecondary, 0.7f);
            }
            y += 12;

            if (state == RPGPlayer.PickState.Available)
            {
                string id = node.Id;
                DrawConfirmButton(sb, new Rectangle(x, y, w, 44), Language.GetTextValue("Mods.RPGMod.UI.PickNode"), true, () =>
                {
                    if (Rpg.TryPick(id))
                        SoundEngine.PlaySound(SoundID.Item4);
                });
            }
        }

        DrawAwakeningSlot(sb, rpg, aside);
    }

    /// <summary>Слот пробуждения внизу панели описания (виден после Лунного лорда, DESIGN §5).</summary>
    private static void DrawAwakeningSlot(SpriteBatch sb, RPGPlayer rpg, Rectangle aside)
    {
        if (!NPC.downedMoonlord && !rpg.Awakened)
            return;

        int x = aside.X + 14, w = aside.Width - 28;
        int top = aside.Bottom - 170;
        UIDraw.Text(sb, Language.GetTextValue("Mods.RPGMod.UI.AwakenSlot"), new Vector2(x, top), Theme.TextSecondary, 0.75f);
        var slot = new Rectangle(x, top + 24, Node, Node);
        string key = ClassIds.Key(rpg.ClassId);
        int y = top + 24;

        if (rpg.Awakened)
        {
            UIDraw.RoundedRect(sb, Inflate(slot, 4), GoldGlow * 0.35f, null, 0, radius: 12);
            UIDraw.RoundedRect(sb, slot, Theme.SlotBack, GoldGlow, 2, radius: 8);
            Theme.IconOrLetter(sb, Icons.Get($"RPGMod/Assets/SkillIcons/awaken_{key}"), "A", slot, Color.White, GoldGlow);
            int tx = slot.Right + 10;
            int ty = y + 2;
            DrawWrapped(sb, Language.GetTextValue($"Mods.RPGMod.Awakening.{key}.Name"), tx, ref ty, aside.Right - 14 - tx, GoldGlow, 0.85f);
            y = slot.Bottom + 6;
            DrawWrapped(sb, Language.GetTextValue($"Mods.RPGMod.Awakening.{key}.Effect"), x, ref y, w, Color.White, 0.7f);
            if (slot.Contains(Main.MouseScreen.ToPoint()))
                UICommon.TooltipMouseText(Language.GetTextValue($"Mods.RPGMod.Awakening.{key}.Name") + "\n"
                    + Language.GetTextValue($"Mods.RPGMod.Awakening.{key}.Effect") + "\n"
                    + Language.GetTextValue($"Mods.RPGMod.Awakening.{key}.Bonus"));
        }
        else
        {
            Theme.Slot(sb, slot, dark: true);
            UIDraw.TextCentered(sb, Language.GetTextValue("Mods.RPGMod.UI.AwakenUnknown"), new Vector2(slot.Center.X, slot.Center.Y + 2), GoldGlow, 0.9f);
            int tx = slot.Right + 10;
            int ty = y;
            DrawWrapped(sb, Language.GetTextValue("Mods.RPGMod.UI.AwakenHint"), tx, ref ty, aside.Right - 14 - tx, Theme.TextSecondary, 0.7f);
        }
    }

    // ================= Помощники =================

    private static Rectangle NodeRect(Rectangle area, int index)
    {
        Vector2 c = centers[index];
        return new Rectangle(area.X + (int)c.X - Node / 2, area.Y + (int)c.Y - Node / 2, Node, Node);
    }

    /// <summary>Связь «ступенькой»: вправо до середины, вертикально, вправо до ребёнка.</summary>
    private static void DrawEdge(SpriteBatch sb, Rectangle area, int parent, int child, Color color, int thickness, bool dashed)
    {
        Vector2 p = centers[parent] + new Vector2(area.X, area.Y);
        Vector2 c = centers[child] + new Vector2(area.X, area.Y);
        float x1 = p.X + Node / 2f, x2 = c.X - Node / 2f, mid = (x1 + x2) / 2f;
        Segment(sb, new Vector2(x1, p.Y), new Vector2(mid, p.Y), color, thickness, dashed);
        Segment(sb, new Vector2(mid, p.Y), new Vector2(mid, c.Y), color, thickness, dashed);
        Segment(sb, new Vector2(mid, c.Y), new Vector2(x2, c.Y), color, thickness, dashed);
    }

    /// <summary>Горизонтальный или вертикальный отрезок; пунктир 7/6.</summary>
    private static void Segment(SpriteBatch sb, Vector2 a, Vector2 b, Color color, int t, bool dashed)
    {
        Texture2D px = TextureAssets.MagicPixel.Value;
        bool horizontal = Math.Abs(a.Y - b.Y) < 0.5f;
        float from = horizontal ? Math.Min(a.X, b.X) : Math.Min(a.Y, b.Y);
        float to = horizontal ? Math.Max(a.X, b.X) : Math.Max(a.Y, b.Y);
        // Продлеваем на половину толщины, чтобы углы ступеньки были сплошными
        from -= t / 2f;
        to += t / 2f;
        float step = dashed ? 13f : to - from;
        for (float s = from; s < to; s += step)
        {
            float len = Math.Min(dashed ? 7f : to - from, to - s);
            Rectangle rect = horizontal
                ? new Rectangle((int)s, (int)(a.Y - t / 2f), (int)Math.Ceiling(len), t)
                : new Rectangle((int)(a.X - t / 2f), (int)s, t, (int)Math.Ceiling(len));
            sb.Draw(px, rect, color);
        }
    }

    private void DrawConfirmButton(SpriteBatch sb, Rectangle rect, string label, bool enabled, Action onClick)
    {
        float op = enabled ? 1f : 0.45f;
        bool hover = enabled && rect.Contains(Main.MouseScreen.ToPoint());
        UIDraw.RoundedRect(sb, rect, Theme.Confirm * op, (hover ? Theme.Accent : Theme.SlotBorder) * op, 2, radius: 8);
        UIDraw.TextCentered(sb, label, new Vector2(rect.Center.X, rect.Center.Y + 2), Color.White * op, 0.85f);
        if (enabled)
            clickables.Add((rect, onClick));
    }

    /// <summary>Текст с переносом по ширине; сдвигает y вниз.</summary>
    private static void DrawWrapped(SpriteBatch sb, string text, int x, ref int y, int width, Color color, float scale)
    {
        string[] lines = Utils.WordwrapString(text, FontAssets.MouseText.Value, (int)(width / scale), 12, out int count);
        for (int i = 0; i <= count && i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i]))
                continue;
            UIDraw.Text(sb, lines[i].TrimEnd(), new Vector2(x, y), color, scale);
            y += (int)(26 * scale);
        }
    }

    private static Rectangle Inflate(Rectangle r, int by)
    {
        r.Inflate(by, by);
        return r;
    }

    public static string Roman(int tier) => tier switch { 1 => "I", 2 => "II", 3 => "III", _ => "" };

    public static string StatusText(RPGPlayer rpg, SkillNode node, RPGPlayer.PickState state) => state switch
    {
        RPGPlayer.PickState.Picked => Language.GetTextValue("Mods.RPGMod.UI.StatePicked"),
        RPGPlayer.PickState.Available => Language.GetTextValue("Mods.RPGMod.UI.StateAvailable"),
        RPGPlayer.PickState.BranchClosed => Language.GetTextValue("Mods.RPGMod.UI.StateBranchClosed"),
        _ => !rpg.TierOpen(node.Tier)
            ? Language.GetTextValue("Mods.RPGMod.UI.StateLocked", RequirementText(node.Tier))
            : Language.GetTextValue("Mods.RPGMod.UI.StateNeedParent"),
    };

    public static string RequirementText(int tier)
        => Language.GetTextValue($"Mods.RPGMod.UI.TierReq{tier}", RPGPlayer.TierLevel(tier));
}
