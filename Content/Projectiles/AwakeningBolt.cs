using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Content.Projectiles;

public class AwakeningBolt : ModProjectile
{
    public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

    public const float Speed = 14f;
    private const float HomingRange = 1200f, Inertia = 12f;
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GoldenShowerFriendly}";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 240;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        NPC nearest = null;
        float distance = HomingRange * HomingRange;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy(Projectile)) continue;
            float next = Vector2.DistanceSquared(npc.Center, Projectile.Center);
            if (next < distance) { nearest = npc; distance = next; }
        }
        if (nearest != null)
        {
            Vector2 desired = (nearest.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * Speed;
            Projectile.velocity = (Projectile.velocity * (Inertia - 1f) + desired) / Inertia;
        }
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        if (!Main.dedServ)
            Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, -Projectile.velocity * .1f, 100, default, .8f).noGravity = true;
    }
}
