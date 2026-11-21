using System;
using System.IO;
using RPGMod.Common.Config;
using RPGMod.Common.Data;
using RPGMod.Common.Players;
using RPGMod.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Common.NPCs;

public class RogueBleedNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    private int timeLeft, damagePerSecond;

    internal static void ApplyHit(NPC target, int hitDamage)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Apply(target, hitDamage);
        else if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            var packet = ModContent.GetInstance<RPGMod>().GetPacket();
            packet.Write((byte)MessageType.RogueBleed);
            packet.Write((short)target.whoAmI);
            packet.Write(target.netID);
            packet.Write(hitDamage);
            packet.Send();
        }
    }

    internal static void Receive(BinaryReader reader, int sender)
    {
        if (Main.netMode != NetmodeID.Server || !CalamityCompat.RogueAvailable) return;

        int index = reader.ReadInt16();
        int netId = reader.ReadInt32();
        int hitDamage = reader.ReadInt32();
        if (index < 0 || index >= Main.maxNPCs || sender < 0 || sender >= Main.maxPlayers) return;

        NPC target = Main.npc[index];
        Player player = Main.player[sender];
        if (!player.active || player.dead || !target.active || target.netID != netId || target.friendly
            || target.dontTakeDamage || target.SpawnedFromStatue || target.lifeMax <= 5
            || (uint)(hitDamage - 1) >= (uint)target.lifeMax) return;

        var rpg = player.GetModPlayer<RPGPlayer>();
        if (rpg.ClassId == ClassIds.Rogue && rpg.Has("first_blood"))
            Apply(target, hitDamage);
    }

    private static void Apply(NPC target, int hitDamage)
    {
        var config = RPGConfig.Instance;
        int dps = (int)(hitDamage * (double)config.FirstBloodDamage);
        var bleed = target.GetGlobalNPC<RogueBleedNPC>();
        if (dps <= 0 || (bleed.timeLeft > 0 && dps < bleed.damagePerSecond)) return;

        bleed.timeLeft = config.FirstBloodDuration;
        bleed.damagePerSecond = dps;
    }

    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        // The server owns DoT damage and shares resulting NPC life with clients.
        if (Main.netMode == NetmodeID.MultiplayerClient || timeLeft <= 0) return;
        timeLeft--;
        npc.lifeRegen = Math.Min(0, npc.lifeRegen) - 2 * damagePerSecond;
        damage = Math.Max(damage, damagePerSecond / 4);
    }
}
