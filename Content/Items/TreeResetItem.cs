using System.Collections.Generic;
using RPGMod.Common.Players;
using RPGMod.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RPGMod.Content.Items;

public class TreeResetItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useTime = Item.useAnimation = 30;
        Item.UseSound = SoundID.Item3;
        Item.rare = ItemRarityID.Green;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            bool refund = player.GetModPlayer<RPGPlayer>().ResetTree();
            if (refund && CalamityCompat.AwakeningAvailable
                && ModContent.TryFind<ModItem>("RPGMod/AwakeningStone", out var stone))
                player.QuickSpawnItem(player.GetSource_ItemUse(Item), stone.Type);
            Main.NewText(Language.GetTextValue(refund ? "Mods.RPGMod.Awakening2.TreeResetRefund" : "Mods.RPGMod.Awakening2.TreeReset"));
        }
        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!CalamityCompat.AwakeningAvailable)
            foreach (var line in tooltips)
                if (line.Mod == "Terraria" && line.Name == "Tooltip0")
                    line.Text = Language.GetTextValue("Mods.RPGMod.UI.TreeResetVanillaTooltip");
    }

    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.Wood)
        .AddIngredient(ItemID.FallenStar, 2).AddTile(TileID.WorkBenches).Register();
}
