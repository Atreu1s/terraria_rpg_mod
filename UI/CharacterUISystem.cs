using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGMod.Common.Config;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RPGMod.UI;

/// <summary>Корневой UIState окна персонажа: панель статов слева, древо справа.</summary>
public class CharacterWindow : UIState
{
    public const int Gap = 24;
    public StatPanel Stats { get; private set; }
    public UIElement Tree { get; private set; }

    public override void OnInitialize()
    {
        var root = new UIElement();
        int width = StatPanel.W + Gap + TreePanel.W;
        root.Width.Set(width, 0f);
        root.Height.Set(StatPanel.H, 0f);
        root.Left.Set(-width / 2f, 0.5f);
        root.Top.Set(80, 0f);
        Append(root);

        Stats = new StatPanel();
        root.Append(Stats);

        Tree = new TreePanel();
        Tree.Left.Set(StatPanel.W + Gap, 0f);
        root.Append(Tree);
    }
}

/// <summary>
/// Владелец окна персонажа (≈ провайдер модалки): открытие/закрытие, кнопка «Статус», клавиша.
/// </summary>
public class CharacterUISystem : ModSystem
{
    public static ModKeybind ToggleKey { get; private set; }

    private UserInterface ui;
    private CharacterWindow window;

    public static bool Visible => ModContent.GetInstance<CharacterUISystem>().ui?.CurrentState != null;

    public override void Load()
    {
        ToggleKey = KeybindLoader.RegisterKeybind(Mod, "ToggleCharacter", "None");
    }

    public override void PostSetupContent()
    {
        // All ModSystem.Load hooks (including CalamityCompat) have completed.
        // StatPanel builds its visible rows once for the selected mode.
        if (Main.dedServ)
            return;
        ui = new UserInterface();
        window = new CharacterWindow();
        window.Activate();
    }

    public override void Unload()
    {
        ToggleKey = null;
        ui = null;
        window = null;
        Icons.Clear();
    }

    public static void Toggle()
    {
        var sys = ModContent.GetInstance<CharacterUISystem>();
        if (sys.ui == null)
            return;
        if (sys.ui.CurrentState == null)
        {
            Main.playerInventory = true;
            sys.ui.SetState(sys.window);
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }
        else
        {
            sys.ui.SetState(null);
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (ui == null)
            return;
        // Закрытие инвентаря закрывает окно.
        if (ui.CurrentState != null && !Main.playerInventory)
            ui.SetState(null);
        ui.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(l => l.Name == "Vanilla: Inventory");
        if (index < 0)
            return;
        layers.Insert(index + 1, new LegacyGameInterfaceLayer("RPGMod: Character", () =>
        {
            if (Main.playerInventory)
            {
                DrawStatusButton(Main.spriteBatch);
                if (ui?.CurrentState != null)
                    ui.Draw(Main.spriteBatch, new GameTime());
            }
            return true;
        }, InterfaceScaleType.UI));
    }

    /// <summary>Кнопка «Статус» в экране инвентаря (позиция настраивается в клиентском конфиге).</summary>
    private void DrawStatusButton(SpriteBatch sb)
    {
        var cfg = RPGClientConfig.Instance;
        string label = Language.GetTextValue("Mods.RPGMod.UI.StatusButton");
        int w = (int)Theme.MeasureWidth(label, 0.8f) + 20;
        var rect = new Rectangle(cfg.StatusButtonX, cfg.StatusButtonY, w, 28);
        bool hover = rect.Contains(Main.MouseScreen.ToPoint());
        bool open = ui?.CurrentState != null;

        Theme.Slot(sb, rect);
        if (hover || open)
            UIDraw.RoundedRect(sb, rect, Color.Transparent, Theme.Accent, 2, radius: 8);
        UIDraw.TextCentered(sb, label, new Vector2(rect.Center.X, rect.Center.Y + 2), open ? Theme.Accent : Color.White, 0.8f);

        if (hover)
        {
            Main.LocalPlayer.mouseInterface = true;
            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                Main.mouseLeftRelease = false;
                Toggle();
            }
        }
    }
}

/// <summary>Нажатие клавиши (по умолчанию не назначена) открывает окно.</summary>
public class CharacterKeybindPlayer : ModPlayer
{
    public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
    {
        if (CharacterUISystem.ToggleKey?.JustPressed == true)
            CharacterUISystem.Toggle();
    }
}
