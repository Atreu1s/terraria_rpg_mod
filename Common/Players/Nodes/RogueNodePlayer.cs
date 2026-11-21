using System;
using Microsoft.Xna.Framework;
using RPGMod.Common.Config;
using RPGMod.Common.Data;
using RPGMod.Common.NPCs;
using RPGMod.Common.Systems;
using RPGMod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace RPGMod.Common.Players.Nodes;

public class RogueNodePlayer : ModPlayer
{
    private int idleTicks, jugglerTicks, stacks, ricochetCooldown, dodgeCooldown, awakeningHits;
    private bool shadowReady, smokeReady;
    private NPC ambushTarget;
    private RPGPlayer Rpg => Player.GetModPlayer<RPGPlayer>();
    private bool Active => CalamityCompat.RogueAvailable && Rpg.ClassId == ClassIds.Rogue;
    private bool Owner => Player.whoAmI == Main.myPlayer;
    public bool InShadow => Active && Rpg.Has("shadow") && shadowReady;

    public override void PostUpdate()
    {
        if (!Active)
        {
            ClearEffects();
            return;
        }

        var config = RPGConfig.Instance;
        if (!Rpg.Has("shadow"))
        {
            shadowReady = false;
            idleTicks = 0;
        }
        else if (Player.ItemAnimationActive && Player.HeldItem.damage > 0)
        {
            idleTicks = 0;
            if (!Player.HeldItem.CountsAsClass(CalamityCompat.Rogue)) shadowReady = false;
        }
        else if (!shadowReady && ++idleTicks >= config.ShadowDelay)
            shadowReady = true;

        if (!Rpg.Has("juggler") || (jugglerTicks > 0 && --jugglerTicks == 0))
            stacks = 0;
        if (ricochetCooldown > 0) ricochetCooldown--;
        if (dodgeCooldown > 0) dodgeCooldown--;
        if (!Rpg.Has("smoke_bomb")) smokeReady = false;
        if (!Rpg.Awakened) awakeningHits = 0;
    }

    public override void PostUpdateEquips()
    {
        if (!Active || !Rpg.Has("juggler") || stacks == 0) return;

        var config = RPGConfig.Instance;
        var rogue = CalamityCompat.Rogue;
        Player.GetAttackSpeed(rogue) += stacks * config.JugglerSpeed;
        if (stacks >= config.JugglerMaxStacks && Rpg.Has("bravado"))
        {
            Player.moveSpeed += config.BravadoMoveSpeed;
            Player.GetCritChance(rogue) += config.BravadoCrit;
        }
    }

    internal static bool CountsForRogueSkills(NPC target, DamageClass damageType) =>
        CalamityCompat.Rogue is DamageClass rogue && damageType.CountsAsClass(rogue)
        && !target.friendly && !target.SpawnedFromStatue && target.lifeMax > 5;

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (proj.ModProjectile is not ShadowDagger)
            ModifyRogueHit(target, proj.DamageType, ref modifiers);
    }

    public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        => ModifyRogueHit(target, item.DamageType, ref modifiers);

    private void ModifyRogueHit(NPC target, DamageClass damageType, ref NPC.HitModifiers modifiers)
    {
        if (!Owner || !Active || !CountsForRogueSkills(target, damageType)) return;

        var config = RPGConfig.Instance;
        // This hook runs before the hit takes life. OnHit can no longer test full life.
        ambushTarget = null;
        if (Rpg.Has("ambush") && target.life >= target.lifeMax)
        {
            modifiers.SourceDamage *= 1f + config.AmbushDamage;
            ambushTarget = target;
        }
        if (Rpg.Has("executioner") && target.life < target.lifeMax * config.ExecutionerThreshold)
            modifiers.SourceDamage *= 1f + config.ExecutionerDamage;
        if (InShadow)
        {
            modifiers.SourceDamage *= 1f + config.ShadowDamage;
            modifiers.SetCrit();
            if (Rpg.Has("dusk_blade"))
            {
                modifiers.CritDamage *= config.DuskBladeCritMultiplier;
                modifiers.ArmorPenetration += config.DuskBladeArmorPenetration;
            }
        }
        if (smokeReady && Rpg.Has("smoke_bomb"))
            modifiers.SourceDamage *= 1f + config.SmokeBombDamage;
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        => OnRogueHit(target, hit.DamageType, damageDone);

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (proj.ModProjectile is not ShadowDagger)
            OnRogueHit(target, hit.DamageType, damageDone);
    }

    private void OnRogueHit(NPC target, DamageClass damageType, int damageDone)
    {
        if (!Owner || !Active || !CountsForRogueSkills(target, damageType)) return;

        if (ambushTarget == target && target.active && target.life > 0 && Rpg.Has("first_blood"))
            RogueBleedNPC.ApplyHit(target, damageDone);
        ambushTarget = null;
        idleTicks = 0;
        shadowReady = target.life <= 0 && Rpg.Has("vanish");
        smokeReady = false;

        var config = RPGConfig.Instance;
        if (Rpg.Has("juggler"))
        {
            stacks = Math.Min(stacks + 1, config.JugglerMaxStacks);
            jugglerTicks = config.JugglerDuration;
            if (stacks >= config.JugglerMaxStacks && Rpg.Has("ricochet") && ricochetCooldown == 0)
                Ricochet(target, damageDone, config);
        }
        if (Rpg.Awakened)
            CountAwakeningHit(target, damageDone, config);
    }

    private void CountAwakeningHit(NPC target, int damageDone, RPGConfig config)
    {
        int interval = Rpg.Has("trickster") ? config.TricksterAwakeningHits : config.RogueAwakeningHits;
        if (++awakeningHits < interval) return;
        awakeningHits = 0;

        float fraction = Rpg.Has("assassin") ? config.AssassinAwakeningDamage : config.RogueAwakeningDamage;
        if (fraction <= 0f || !ModContent.TryFind<ModProjectile>("RPGMod/ShadowDagger", out var dagger)) return;

        int damage = Math.Max(1, (int)(damageDone * fraction));
        Vector2 direction = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX * Player.direction);
        var source = Player.GetSource_OnHit(target, "RogueAwakening");
        int count = config.RogueAwakeningDaggers;
        for (int i = 0; i < count; i++)
        {
            Vector2 velocity = direction.RotatedBy((i - (count - 1) / 2f) * .22f) * ShadowDagger.Speed;
            Projectile.NewProjectile(source, Player.Center, velocity, dagger.Type, damage, 0f, Player.whoAmI);
        }
    }

    private void Ricochet(NPC original, int damageDone, RPGConfig config)
    {
        if (config.RicochetDamage <= 0f
            || !ModContent.TryFind<ModProjectile>("RPGMod/ShadowDagger", out var dagger)) return;

        NPC next = ShadowDagger.FindTarget(original.Center, config.RicochetRangeTiles * 16f, original);
        if (next == null) return;

        Vector2 velocity = (next.Center - original.Center).SafeNormalize(Vector2.UnitX) * ShadowDagger.Speed;
        int damage = Math.Max(1, (int)(damageDone * config.RicochetDamage));
        Projectile.NewProjectile(Player.GetSource_OnHit(original, "RogueRicochet"), original.Center,
            velocity, dagger.Type, damage, 0f, Player.whoAmI, next.whoAmI + 1);
        ricochetCooldown = config.RicochetInterval;
    }

    public override bool ConsumableDodge(Player.HurtInfo info)
    {
        if (!Owner || !Active || dodgeCooldown > 0 || !Rpg.Has("evasion")) return false;

        var config = RPGConfig.Instance;
        bool lucky = Rpg.Has("lucky_break");
        float chance = config.EvasionChance;
        if (lucky)
            chance += Math.Min(config.LuckyBreakMaxChance,
                Rpg.GetStat(Stat.Luck) / config.LuckyBreakLuckPerStep * config.LuckyBreakChancePerStep);
        if (Main.rand.NextFloat() >= chance) return false;

        dodgeCooldown = config.EvasionCooldown;
        Player.NinjaDodge();
        if (Rpg.Has("smoke_bomb"))
        {
            Player.SetImmuneTimeForAllTypes(config.SmokeBombImmunity);
            smokeReady = true;
        }
        if (lucky)
        {
            int heal = (int)(Player.statLifeMax2 * config.LuckyBreakHeal);
            if (heal > 0) Player.Heal(heal);
        }
        return true;
    }

    public override void UpdateDead() => ClearEffects();

    internal void ClearEffects()
    {
        idleTicks = jugglerTicks = stacks = ricochetCooldown = dodgeCooldown = awakeningHits = 0;
        shadowReady = smokeReady = false;
        ambushTarget = null;
    }
}
