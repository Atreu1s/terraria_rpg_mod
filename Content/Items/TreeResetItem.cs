using RPGMod.Common.Players;
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
            if (refund)
                player.QuickSpawnItem(player.GetSource_ItemUse(Item), ModContent.ItemType<AwakeningStone>());
            Main.NewText(Language.GetTextValue(refund ? "Mods.RPGMod.Awakening2.TreeResetRefund" : "Mods.RPGMod.Awakening2.TreeReset"));
        }
        return true;
    }

    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.Wood)
        .AddIngredient(ItemID.FallenStar, 2).AddTile(TileID.WorkBenches).Register();
}
