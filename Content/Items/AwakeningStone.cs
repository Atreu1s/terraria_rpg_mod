using RPGMod.Common.Data;
using RPGMod.Common.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RPGMod.Content.Items;

public class AwakeningStone : ModItem
{
    private const int EssenceCount = 3, GeodeCount = 10, GuardianCount = 3, FlashDust = 60;

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 45;
        Item.rare = ItemRarityID.Red;
    }

    public override bool CanUseItem(Player player)
    {
        var rpg = player.GetModPlayer<RPGPlayer>();
        if (rpg.Awakened) return false;
        foreach (string id in rpg.Picked)
            if (SkillTree.Nodes.TryGetValue(id, out var node) && node.Tier == 3 && node.ClassId == rpg.ClassId) return true;
        return false;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer) return true;
        var rpg = player.GetModPlayer<RPGPlayer>();
        rpg.Awakened = true;
        for (int i = 0; i < FlashDust; i++)
        {
            Vector2 velocity = Vector2.UnitY.RotatedBy(MathHelper.TwoPi * i / FlashDust) * Main.rand.NextFloat(3f, 8f);
            Dust.NewDustPerfect(player.Center, DustID.GoldFlame, velocity, 0, new Color(242, 193, 78), 1.6f).noGravity = true;
        }
        Lighting.AddLight(player.Center, 1.5f, 1.1f, .4f);
        SoundEngine.PlaySound(SoundID.Item29, player.Center);
        Main.NewText(Language.GetTextValue("Mods.RPGMod.Awakening2.Awakened",
            Language.GetTextValue($"Mods.RPGMod.Awakening.{ClassIds.Key(rpg.ClassId)}.Name")), new Color(242, 193, 78));
        return true;
    }

    public override void AddRecipes()
    {
        if (!ModLoader.TryGetMod("CalamityMod", out var cal)
            || !cal.TryFind<ModItem>("DivineGeode", out var geode)
            || !cal.TryFind<ModItem>("ArmoredShell", out var shell)
            || !cal.TryFind<ModItem>("DarkPlasma", out var plasma)
            || !cal.TryFind<ModItem>("TwistingNether", out var nether)) return;
        CreateRecipe().AddIngredient<AwakeningEssence>(EssenceCount)
            .AddIngredient(geode.Type, GeodeCount).AddIngredient(shell.Type, GuardianCount)
            .AddIngredient(plasma.Type, GuardianCount).AddIngredient(nether.Type, GuardianCount)
            .AddTile(TileID.LunarCraftingStation)
            .AddCondition(new Condition("Mods.RPGMod.Awakening2.ThreeSources", () =>
                Main.LocalPlayer.GetModPlayer<AwakeningPlayer>().EssenceSources.Count >= EssenceCount))
            .Register();
    }
}
