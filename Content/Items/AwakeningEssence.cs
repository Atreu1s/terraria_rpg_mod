using System.Collections.Generic;
using System.IO;
using RPGMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RPGMod.Content.Items;

public class AwakeningEssence : ModItem
{
    public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

    // Only a freshly spawned personal drop carries a source; inventory essences are ordinary materials.
    internal int BossSource = -1;
    private bool discard;

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Red;
        BossSource = -1;
        discard = false;
    }

    public override void NetSend(BinaryWriter writer) => writer.Write((sbyte)BossSource);
    public override void NetReceive(BinaryReader reader) => BossSource = reader.ReadSByte();

    private bool AcceptDrop()
    {
        if (Main.netMode == NetmodeID.Server) return true;
        if (BossSource >= 0)
        {
            discard = !Main.LocalPlayer.GetModPlayer<AwakeningPlayer>().ClaimSource(BossSource);
            BossSource = -1;
            if (discard) { Item.active = false; Item.stack = 0; }
        }
        return !discard;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed) => AcceptDrop();
    public override bool CanPickup(Player player) => player.whoAmI == Main.myPlayer && AcceptDrop();

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (Main.LocalPlayer.GetModPlayer<AwakeningPlayer>().EssenceSources.Count > 0)
            tooltips.Add(new TooltipLine(Mod, "StoneRecipe", Language.GetTextValue("Mods.RPGMod.Awakening2.RecipeHint")));
    }
}
