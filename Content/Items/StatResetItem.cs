using RPGMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RPGMod.Content.Items;

public class StatResetItem : ModItem
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
        Item.rare = ItemRarityID.Blue;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.GetModPlayer<RPGPlayer>().ResetStats();
            Main.NewText(Language.GetTextValue("Mods.RPGMod.Awakening2.StatsReset"));
        }
        return true;
    }

    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.Wood)
        .AddIngredient(ItemID.FallenStar).AddTile(TileID.WorkBenches).Register();
}
