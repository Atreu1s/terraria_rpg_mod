using System;
using Microsoft.Xna.Framework;
using RPGMod.Common.Data;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Common.Players.Nodes;

/// <summary>Условные эффекты Стрелка, тиры 2–3 (DESIGN §4).</summary>
public class RangerNodePlayer : ModPlayer
{
    // ---- Числа узлов (DESIGN §4) ----
    private const int ColdBloodDelay = 60;
    private const float ColdBloodCrit = 15f, ColdBloodDamage = 0.10f;
    private const float LongRangeMax = 0.20f, LongRangeTiles = 40f;
    private const float SpinPerSecond = 0.02f, SpinMax = 0.20f;
    private const int SpinResetIdle = 30;
    private const float QuartermasterAmmo = 0.20f, QuartermasterMove = 0.08f;
    private const float DeadshotCritMult = 2.5f / 2f;
    private const int HuntersMarkTime = 5 * 60;
    private const float ExplosiveFraction = 0.35f, ExplosiveRadius = 3 * 16f;
    private const int ArmorBreak = 20;
    private const float LeadRainChance = 0.20f;
    private const int OverheatTime = 3 * 60;
    private const float RunAndGunDamage = 0.10f;
    private const float BottomlessAmmo = 0.30f, BottomlessDamage = 0.08f, BottomlessLife = 0.80f;

    // ---- Состояние ----
    private int stillTicks;
    private int spinTicks, idleTicks;
    private bool inExplosion;

    private RPGPlayer Rpg => Player.GetModPlayer<RPGPlayer>();
    private bool Has(string id) => Rpg.ClassId == ClassIds.Ranger && Rpg.Has(id);

    /// <summary>Текущая прибавка «Раскрутки» (0…0.2).</summary>
    public float SpinBonus => Math.Min(SpinMax, SpinPerSecond * spinTicks / 60f);
    public bool MaxSpin => Has("spin_up") && SpinBonus >= SpinMax;

    public override void PostUpdateEquips()
    {
        if (Rpg.ClassId != ClassIds.Ranger)
            return;

        if (Has("cold_blood") && stillTicks >= ColdBloodDelay)
        {
            Player.GetCritChance(DamageClass.Ranged) += ColdBloodCrit;
            Player.GetDamage(DamageClass.Ranged) += ColdBloodDamage;
        }

        if (Has("spin_up"))
            Player.GetAttackSpeed(DamageClass.Ranged) += SpinBonus;

        if (Has("quartermaster"))
            Player.moveSpeed += QuartermasterMove;

        if (Has("run_and_gun") && Math.Abs(Player.velocity.X) > 0.1f)
            Player.GetDamage(DamageClass.Ranged) += RunAndGunDamage;

        if (Has("bottomless_mag") && Player.statLife > Player.statLifeMax2 * BottomlessLife)
            Player.GetDamage(DamageClass.Ranged) += BottomlessDamage;
    }

    public override void PostUpdate()
    {
        stillTicks = Player.velocity.LengthSquared() < 0.01f ? stillTicks + 1 : 0;

        bool firing = Player.ItemAnimationActive && Player.HeldItem.CountsAsClass(DamageClass.Ranged);
        if (firing)
        {
            spinTicks = Math.Min(spinTicks + 1, 60 * 60);
            idleTicks = 0;
        }
        else if (++idleTicks > SpinResetIdle)
        {
            spinTicks = 0;
        }
    }

    public override bool CanConsumeAmmo(Item weapon, Item ammo)
    {
        float chance = 0f;
        if (Has("quartermaster")) chance += QuartermasterAmmo;
        if (Has("bottomless_mag")) chance += BottomlessAmmo;
        return !(chance > 0f && Main.rand.NextFloat() < chance);
    }

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (Has("lead_rain") && MaxSpin && item.CountsAsClass(DamageClass.Ranged) && Main.rand.NextFloat() < LeadRainChance)
            Projectile.NewProjectile(source, position, velocity.RotatedByRandom(MathHelper.ToRadians(6)), type, damage, knockback, Player.whoAmI);
        return true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Rpg.ClassId != ClassIds.Ranger || !modifiers.DamageType.CountsAsClass(DamageClass.Ranged))
            return;

        if (Has("long_range"))
        {
            float tiles = Vector2.Distance(Player.Center, target.Center) / 16f;
            modifiers.FinalDamage *= 1f + LongRangeMax * Math.Min(1f, tiles / LongRangeTiles);
        }
        if (Has("deadshot"))
            modifiers.CritDamage *= DeadshotCritMult;
        if (Has("armor_breaker"))
            modifiers.ArmorPenetration += ArmorBreak;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Rpg.ClassId != ClassIds.Ranger || !hit.DamageType.CountsAsClass(DamageClass.Ranged) || inExplosion)
            return;

        if (Has("hunters_mark"))
            target.GetGlobalNPC<RangerGlobalNPC>().MarkTime = HuntersMarkTime;

        if (Has("overheat") && MaxSpin)
            target.AddBuff(BuffID.OnFire, OverheatTime);

        if (Has("explosive_rounds") && hit.Crit && Player.whoAmI == Main.myPlayer)
        {
            inExplosion = true;
            int dmg = Math.Max(1, (int)(damageDone * ExplosiveFraction));
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.whoAmI == target.whoAmI || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                    continue;
                if (Vector2.Distance(npc.Center, target.Center) <= ExplosiveRadius)
                    Player.ApplyDamageToNPC(npc, dmg, 0f, hit.HitDirection, false, DamageClass.Ranged);
            }
            for (int i = 0; i < 12; i++)
                Dust.NewDust(target.Center - new Vector2(ExplosiveRadius / 2), (int)ExplosiveRadius, (int)ExplosiveRadius, DustID.Torch);
            inExplosion = false;
        }
    }
}

/// <summary>Метка охотника на враге: +10% входящего урона от всех источников.</summary>
public class RangerGlobalNPC : GlobalNPC
{
    private const float MarkBonus = 0.10f;

    public override bool InstancePerEntity => true;

    public int MarkTime;

    public override void PostAI(NPC npc)
    {
        if (MarkTime > 0)
        {
            MarkTime--;
            if (Main.rand.NextBool(10))
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.GreenTorch, 0f, -1f, 100, default, 0.9f);
        }
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (MarkTime > 0)
            modifiers.FinalDamage *= 1f + MarkBonus;
    }
}
