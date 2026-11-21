using Microsoft.Xna.Framework;
using RPGMod.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Content.Projectiles;

public class ShadowDagger : ModProjectile
{
    internal const float Speed = 16f;
    private const float HomingRange = 800f, Inertia = 10f;

    public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.ThrowingKnife}";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.DamageType = CalamityCompat.Rogue ?? DamageClass.Default;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 120;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    // Ricochet locks onto the chosen target. Awakening daggers can retarget freely.
    public override bool? CanHitNPC(NPC target)
        => Projectile.ai[0] > 0 && target.whoAmI != (int)Projectile.ai[0] - 1 ? false : null;

    internal static NPC FindTarget(Vector2 origin, float range, NPC excluded = null)
    {
        NPC nearest = null;
        float best = range * range;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc == excluded || !npc.CanBeChasedBy()) continue;
            float distance = Vector2.DistanceSquared(origin, npc.Center);
            if (distance >= best) continue;
            nearest = npc;
            best = distance;
        }
        return nearest;
    }

    public override void AI()
    {
        int targetIndex = (int)Projectile.ai[0] - 1;
        NPC target = (uint)targetIndex < Main.maxNPCs
            ? Main.npc[targetIndex]
            : FindTarget(Projectile.Center, HomingRange);

        if (target != null && target.CanBeChasedBy(Projectile))
        {
            Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * Speed;
            Projectile.velocity = (Projectile.velocity * (Inertia - 1f) + desired) / Inertia;
        }
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        if (!Main.dedServ)
            Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, -Projectile.velocity * .1f, 100, default, .8f).noGravity = true;
    }
}
