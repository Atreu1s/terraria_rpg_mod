using System.Collections.Generic;
using RPGMod.Common.Players;
using RPGMod.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Common.NPCs;

public class AwakeningDropNPC : GlobalNPC
{
    public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

    private static readonly Dictionary<int, int> sources = new();

    public override void SetStaticDefaults()
    {
        sources.Clear();
        var cal = Systems.CalamityCompat.Calamity;
        if (cal == null) return;
        for (int i = 0; i < AwakeningPlayer.SourceNames.Length; i++)
            if (cal.TryFind<ModNPC>(AwakeningPlayer.SourceNames[i], out var boss)) sources[boss.Type] = i;
    }

    public override void Unload() => sources.Clear();

    public override void OnKill(NPC npc)
    {
        if (!Systems.CalamityCompat.AwakeningAvailable || Main.netMode == NetmodeID.MultiplayerClient
            || !sources.TryGetValue(npc.type, out int source)
            || !ModContent.TryFind<ModItem>("RPGMod/AwakeningEssence", out var essence)) return;
        if (Main.netMode == NetmodeID.SinglePlayer && Main.LocalPlayer.GetModPlayer<AwakeningPlayer>().EssenceSources.Contains(AwakeningPlayer.SourceNames[source])) return;
        int index = Item.NewItem(npc.GetSource_Loot(), npc.getRect(), essence.Type, noBroadcast: true);
        if (index >= Main.maxItems) return;
        ((AwakeningEssence)Main.item[index].ModItem).BossSource = source;
        if (Main.netMode == NetmodeID.Server)
        {
            // Same protocol as NPC.DropItemInstanced/CommonCode: send a private copy, then
            // remove the server placeholder. Source filtering happens on the owning client,
            // where the character save lives; no changes to the existing packet handler.
            foreach (Player player in Main.ActivePlayers)
                NetMessage.SendData(MessageID.InstancedItem, player.whoAmI, -1, null, index);
            Main.item[index].active = false;
        }
    }
}
