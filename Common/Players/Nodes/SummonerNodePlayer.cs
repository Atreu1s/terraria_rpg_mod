using System;
using System.IO;
using Microsoft.Xna.Framework;
using RPGMod.Common.Data;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RPGMod.Common.Players.Nodes;

public class SummonerNodePlayer : ModPlayer
{
    private const float PackDamage = .02f, WhipSpeed = .20f, WhipDamage = .10f;
    private const float BondReduction = .12f, UnityReduction = .10f, AlphaDamage = .20f;
    private const float BiteDamage = .01f, SicDamage = .15f, DevourDamage = .05f;
    private const int WhipDuration = 300, HealCooldown = 60, DevourDuration = 300;
    private const int DevourMax = 3, DevourHeal = 5, DevourMinimum = 5, UnityCount = 5;
    private const int SacrificeCooldown = 3 * 60 * 60;
    private int whipTime, healCooldown, devourTime, devourStacks, sacrificeCooldown;
    private RPGPlayer Rpg => Player.GetModPlayer<RPGPlayer>();
    internal bool Has(string id) => Rpg.ClassId == ClassIds.Summoner && Rpg.Has(id);

    // Count actual slot-bearing minions, not their shots, sentries, pets or visual segments.
    public int MinionCount(out bool singleType)
    {
        int count = 0, type = -1;
        singleType = true;
        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            if (proj.owner != Player.whoAmI || !proj.minion || proj.minionSlots <= 0f) continue;
            count++;
            if (type < 0) type = proj.type;
            else if (type != proj.type) singleType = false;
        }
        return count;
    }

    internal static bool MinionHit(Projectile proj) => !ProjectileID.Sets.IsAWhip[proj.type] &&
        (proj.minion || ProjectileID.Sets.MinionShot[proj.type]);

    public override void PostUpdateEquips()
    {
        if (sacrificeCooldown > 0) sacrificeCooldown--;
        if (healCooldown > 0) healCooldown--;
        if (whipTime > 0) whipTime--;
        if (devourTime > 0 && --devourTime == 0) devourStacks = 0;
        if (Rpg.ClassId != ClassIds.Summoner) { whipTime = devourTime = devourStacks = 0; return; }
        if (!Has("whip_master")) whipTime = 0;
        if (!Has("devourer")) devourTime = devourStacks = 0;
        int count = MinionCount(out _);
        if (Has("legion")) Player.maxMinions++;
        if (Has("whip_master")) Player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += WhipSpeed;
        if (Has("bond") && count > 0) Player.endurance += BondReduction;
        if (Has("unity") && count >= UnityCount) Player.endurance += UnityReduction;
    }

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Rpg.ClassId != ClassIds.Summoner || !MinionHit(proj)) return;
        int count = MinionCount(out bool singleType);
        float bonus = 0f;
        if (Has("pack")) bonus += count * PackDamage;
        if (Has("alpha") && count > 0 && singleType) bonus += AlphaDamage;
        if (Has("whip_master") && whipTime > 0) bonus += WhipDamage;
        if (Has("devourer")) bonus += devourStacks * DevourDamage;
        var state = target.GetGlobalNPC<SummonerStackNPC>();
        if (Has("thousand_bites")) bonus += state.Stacks[Player.whoAmI] * BiteDamage;
        if (Has("sic_em") && state.WhipTimes[Player.whoAmI] > 0 && TagDuration(target) > 0) bonus += SicDamage;
        modifiers.SourceDamage += bonus;
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Rpg.ClassId != ClassIds.Summoner || Player.whoAmI != Main.myPlayer) return;
        var state = target.GetGlobalNPC<SummonerStackNPC>();
        if (ProjectileID.Sets.IsAWhip[proj.type])
        {
            if (Has("whip_master")) whipTime = WhipDuration;
            if (Has("sic_em")) state.WhipTimes[Player.whoAmI] = Math.Min(short.MaxValue, TagDuration(target));
            if (Has("pack_heal") && healCooldown == 0)
            {
                int count = MinionCount(out _);
                if (count > 0) { Player.Heal(count); healCooldown = HealCooldown; }
            }
        }
        if (MinionHit(proj) && Has("thousand_bites")) state.AddStack(Player.whoAmI);
        if (target.life <= 0) state.RewardKill(target);
        else if (Has("thousand_bites") || Has("sic_em"))
            SummonerNodeNetwork.Send(target, Player.whoAmI);
    }

    internal void Devour()
    {
        if (!Has("devourer") || Player.whoAmI != Main.myPlayer) return;
        Player.Heal(DevourHeal);
        devourStacks = Math.Min(DevourMax, devourStacks + 1);
        devourTime = DevourDuration;
    }

    private static int TagDuration(NPC npc)
    {
        int duration = 0;
        for (int i = 0; i < NPC.maxBuffs; i++)
            if (npc.buffType[i] > 0 && BuffID.Sets.IsATagBuff[npc.buffType[i]]) duration = Math.Max(duration, npc.buffTime[i]);
        return duration;
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        if (!Has("sacrifice") || sacrificeCooldown > 0 || Player.whoAmI != Main.myPlayer || MinionCount(out _) == 0) return true;
        sacrificeCooldown = SacrificeCooldown;
        foreach (Projectile proj in Main.ActiveProjectiles)
            if (proj.owner == Player.whoAmI && proj.minion) proj.Kill();
        Player.statLife = 1;
        return false;
    }

    public override void UpdateDead()
    {
        if (sacrificeCooldown > 0) sacrificeCooldown--;
        whipTime = devourTime = devourStacks = 0;
    }

    public class SummonerStackNPC : GlobalNPC
    {
        private const int MaxStacks = 10, StackDuration = 180;
        private const float HiveDamage = .10f;
        public override bool InstancePerEntity => true;
        public int[] Stacks = new int[Main.maxPlayers], Times = new int[Main.maxPlayers], WhipTimes = new int[Main.maxPlayers];
        private bool rewarded;

        public void AddStack(int owner) { Stacks[owner] = Math.Min(MaxStacks, Stacks[owner] + 1); Times[owner] = StackDuration; }
        public override void PostAI(NPC npc)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (!Main.player[i].active || !Main.player[i].GetModPlayer<SummonerNodePlayer>().Has("thousand_bites")) Stacks[i] = Times[i] = 0;
                if (Times[i] > 0 && --Times[i] == 0) Stacks[i] = 0;
                if (WhipTimes[i] > 0) WhipTimes[i]--;
            }
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
                if (Stacks[i] >= MaxStacks && Main.player[i].active && Main.player[i].GetModPlayer<SummonerNodePlayer>().Has("hive"))
                { modifiers.FinalDamage *= 1f + HiveDamage; break; }
        }
        public void RewardKill(NPC npc)
        {
            if (rewarded || Main.netMode == NetmodeID.Server) return;
            int owner = Main.myPlayer;
            if (owner < Main.maxPlayers && Stacks[owner] >= DevourMinimum)
            {
                rewarded = true;
                Main.player[owner].GetModPlayer<SummonerNodePlayer>().Devour();
            }
        }
        public override void OnKill(NPC npc) => RewardKill(npc);
        // HitEffect also runs on clients receiving another player's lethal NPC strike.
        public override void HitEffect(NPC npc, NPC.HitInfo hit) { if (npc.life <= 0) RewardKill(npc); }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
        {
            // Initial NPC synchronization includes active stacks for late joiners.
            int count = 0;
            for (int i = 0; i < Main.maxPlayers; i++)
                if (Stacks[i] > 0 || WhipTimes[i] > 0) count++;
            writer.Write((byte)count);
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Stacks[i] == 0 && WhipTimes[i] == 0) continue;
                writer.Write((byte)i);
                writer.Write((byte)Stacks[i]);
                writer.Write((short)Times[i]);
                writer.Write((short)WhipTimes[i]);
            }
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (i != Main.myPlayer) Stacks[i] = Times[i] = WhipTimes[i] = 0;
            }
            int count = reader.ReadByte();
            for (int entry = 0; entry < count; entry++)
            {
                int i = reader.ReadByte();
                int stacks = reader.ReadByte(), time = reader.ReadInt16(), whip = reader.ReadInt16();
                if (i >= Main.maxPlayers || i == Main.myPlayer) continue; // Do not overwrite fresher owning-client hits.
                Stacks[i] = Math.Clamp(stacks, 0, MaxStacks);
                Times[i] = Math.Clamp(time, 0, StackDuration);
                WhipTimes[i] = Math.Max(0, whip);
            }
        }
    }
}

/// <summary>
/// Receives only our reserved ModPacket discriminator, restoring the reader for every
/// other message. This keeps the existing RPGMod/Networking files untouched.
/// Header encoding follows Mod.GetPacket and ModNet.HandleModPacket (tML 1.4.4).
/// </summary>
/// <summary>Синхронизация стаков призывателя: пакет MessageType.SummonerStacks (см. Common/Networking.cs).</summary>
public static class SummonerNodeNetwork
{
    internal static void Send(NPC npc, int owner, int ignore = -1)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        var state = npc.GetGlobalNPC<SummonerNodePlayer.SummonerStackNPC>();
        var packet = ModContent.GetInstance<RPGMod>().GetPacket();
        packet.Write((byte)MessageType.SummonerStacks);
        packet.Write((byte)owner);
        packet.Write((short)npc.whoAmI);
        packet.Write(npc.netID);
        packet.Write((byte)state.Stacks[owner]);
        packet.Write((short)state.Times[owner]);
        packet.Write((short)state.WhipTimes[owner]);
        packet.Send(ignoreClient: ignore);
    }

    /// <summary>Приём пакета (тип уже прочитан в Networking.HandlePacket).</summary>
    internal static void Receive(BinaryReader reader, int playerNumber)
    {
        int owner = reader.ReadByte(), index = reader.ReadInt16(), type = reader.ReadInt32();
        int stacks = reader.ReadByte(), time = reader.ReadInt16(), whip = reader.ReadInt16();
        if (Main.netMode == NetmodeID.Server) owner = playerNumber;
        if (owner < 0 || owner >= Main.maxPlayers || index < 0 || index >= Main.maxNPCs) return;
        NPC npc = Main.npc[index];
        Player player = Main.player[owner];
        if (!npc.active || npc.netID != type || !player.active || stacks > 10 || time < 0 || time > 180 || whip < 0) return;
        var summoner = player.GetModPlayer<SummonerNodePlayer>();
        var state = npc.GetGlobalNPC<SummonerNodePlayer.SummonerStackNPC>();
        if (owner != Main.myPlayer)
        {
            state.Stacks[owner] = summoner.Has("thousand_bites") ? stacks : 0;
            state.Times[owner] = summoner.Has("thousand_bites") ? time : 0;
            state.WhipTimes[owner] = summoner.Has("sic_em") ? whip : 0;
        }
        if (Main.netMode == NetmodeID.Server) Send(npc, owner, playerNumber);
    }
}
