using System;
using Microsoft.Xna.Framework;
using RPGMod.Common.Data;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Common.Players.Nodes;

public class MageNodePlayer : ModPlayer
{
    private const float OverloadMax = .25f, ElementChance = .15f;
    private const float FlowThreshold = .70f, FlowDamage = .10f, FlowSpeed = .08f;
    private const int ElementDuration = 180, MeditationRegen = 4, BloodCost = 2;
    private const int ResonanceHits = 5, EnlightenmentWait = 180, ManaBloodCost = 100, ManaBloodHeal = 10;
    private const float CatalystDamage = .15f, ChainRadius = 160f, ShieldFraction = .20f;
    private const float OverflowCost = .10f, EnlightenmentDamage = .40f;

    private int hits, stationary, manaSpent;
    private bool enlightened;
    private RPGPlayer Rpg => Player.GetModPlayer<RPGPlayer>();
    private bool Has(string id) => Rpg.ClassId == ClassIds.Mage && Rpg.Has(id);

    public override void Load()
    {
        On_Player.UpdateManaRegen += UpdateManaRegen;
        On_Player.CheckMana_Item_int_bool_bool += CheckMana;
    }
    public override void Unload()
    {
        On_Player.UpdateManaRegen -= UpdateManaRegen;
        On_Player.CheckMana_Item_int_bool_bool -= CheckMana;
    }

    private static bool CheckMana(On_Player.orig_CheckMana_Item_int_bool_bool orig, Player player, Item item, int amount, bool pay, bool blockQuickMana)
    {
        int needed = amount < 0 ? player.GetManaCost(item) : amount;
        if (!pay && !blockQuickMana && player.whoAmI == Main.myPlayer && item.CountsAsClass(DamageClass.Magic) && player.statMana < needed)
        {
            var mage = player.GetModPlayer<MageNodePlayer>();
            // Affordability probes must not spend life or start an awakening cooldown.
            if (player.GetModPlayer<AwakeningPlayer>().CanSupplyMana
                || (mage.Has("blood_magic") && player.statLife > (needed - player.statMana) * BloodCost)) return true;
        }
        return orig(player, item, amount, pay, blockQuickMana);
    }

    private static void UpdateManaRegen(On_Player.orig_UpdateManaRegen orig, Player player)
    {
        var mage = player.GetModPlayer<MageNodePlayer>();
        if (mage.Has("overflow")) player.manaRegenDelay = 0f;
        orig(player);
        // Apply to the actual regeneration routine, rather than manaRegen before vanilla
        // recalculates it. A second pass at zero delay doubles its accumulation rate.
        if (mage.Has("meditation") && player.velocity.LengthSquared() < .01f && player.manaRegenDelay <= 0f)
            orig(player);
    }

    public override void PostUpdateEquips()
    {
        if (Rpg.ClassId != ClassIds.Mage) { ClearTransient(); return; }
        if (Has("overload"))
            Player.GetDamage(DamageClass.Magic) += Math.Clamp(1f - Player.statMana / (float)Math.Max(1, Player.statManaMax2), 0f, OverloadMax);
        if (Has("flow") && Player.statMana > Player.statManaMax2 * FlowThreshold)
        {
            Player.GetDamage(DamageClass.Magic) += FlowDamage;
            Player.GetAttackSpeed(DamageClass.Magic) += FlowSpeed;
        }
        if (Has("overflow")) Player.manaCost -= OverflowCost;
        if (!Has("resonance")) hits = 0;
        if (!Has("mana_blood")) manaSpent = 0;
        if (!Has("enlightenment")) { enlightened = false; stationary = 0; }
    }

    public override void PostUpdateMiscEffects()
    {
        if (Has("meditation") && Player.velocity.LengthSquared() < .01f && Player.whoAmI == Main.myPlayer)
        {
            Player.lifeRegen += MeditationRegen; // lifeRegen units are half HP per second.
        }
    }

    public override void PostUpdate()
    {
        if (Has("overflow")) Player.manaRegenDelay = 0;
        if (!Has("enlightenment")) return;
        if (Player.velocity.LengthSquared() < .01f)
        {
            if (++stationary >= EnlightenmentWait) { enlightened = true; stationary = EnlightenmentWait; }
        }
        else stationary = 0;
    }

    public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (!item.CountsAsClass(DamageClass.Magic) || !Has("enlightenment") || !enlightened) return;
        damage = (int)(damage * (1f + EnlightenmentDamage));
        enlightened = false;
        stationary = 0;
    }

    public override void OnMissingMana(Item item, int neededMana)
    {
        if (Player.GetModPlayer<AwakeningPlayer>().SupplyAwakenedMana(item, neededMana)) return;
        if (!Has("blood_magic") || !item.CountsAsClass(DamageClass.Magic) || Player.whoAmI != Main.myPlayer) return;
        int missing = Math.Max(0, neededMana - Player.statMana);
        int lifeCost = missing * BloodCost;
        // Never turn a failed cast into an unavoidable death.
        if (lifeCost <= 0 || Player.statLife <= lifeCost) return;
        Player.statLife -= lifeCost;
        Player.statMana += missing; // CheckMana consumes the supplied mana normally.
        Player.lifeRegenTime = 0;
    }

    public override void OnConsumeMana(Item item, int manaConsumed)
    {
        if (!Has("mana_blood") || Player.whoAmI != Main.myPlayer) return;
        manaSpent += manaConsumed;
        while (manaSpent >= ManaBloodCost)
        {
            manaSpent -= ManaBloodCost;
            Player.Heal(ManaBloodHeal);
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Rpg.ClassId != ClassIds.Mage || !modifiers.DamageType.CountsAsClass(DamageClass.Magic)) return;
        if (Has("resonance") && ++hits >= ResonanceHits)
        {
            hits = 0;
            modifiers.FinalDamage *= 2f;
        }
        if (Has("catalyst") && HasDebuff(target)) modifiers.FinalDamage *= 1f + CatalystDamage;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Rpg.ClassId != ClassIds.Mage || !hit.DamageType.CountsAsClass(DamageClass.Magic) || Player.whoAmI != Main.myPlayer) return;
        if (Has("elemental") && Main.rand.NextFloat() < ElementChance)
            target.AddBuff(Main.rand.Next(3) switch { 0 => BuffID.OnFire, 1 => BuffID.Frostburn, _ => BuffID.Poisoned }, ElementDuration);
        if (!Has("chain_reaction") || target.life > 0 || !HasDebuff(target)) return;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.whoAmI == target.whoAmI || !npc.CanBeChasedBy() || Vector2.DistanceSquared(npc.Center, target.Center) > ChainRadius * ChainRadius) continue;
            for (int i = 0; i < NPC.maxBuffs; i++)
                if (target.buffType[i] > 0 && Main.debuff[target.buffType[i]] && target.buffTime[i] > 0)
                    npc.AddBuff(target.buffType[i], target.buffTime[i]);
        }
    }

    private static bool HasDebuff(NPC npc)
    {
        for (int i = 0; i < NPC.maxBuffs; i++)
            if (npc.buffType[i] > 0 && npc.buffTime[i] > 0 && Main.debuff[npc.buffType[i]]) return true;
        return false;
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (!Has("mana_shield") || Player.whoAmI != Main.myPlayer) return;
        modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) =>
        {
            int absorb = Math.Min(Player.statMana, Math.Min(info.Damage - 1, (int)(info.Damage * ShieldFraction)));
            if (absorb <= 0) return;
            Player.statMana -= absorb;
            info.Damage -= absorb;
        };
    }

    public override void UpdateDead() => ClearTransient();
    private void ClearTransient() { hits = stationary = manaSpent = 0; enlightened = false; }
}
