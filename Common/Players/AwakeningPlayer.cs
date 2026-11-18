using System;
using System.Collections.Generic;
using System.IO;
using RPGMod.Common.Data;
using RPGMod.Common.Players.Nodes;
using RPGMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RPGMod.Common.Players;

public class AwakeningPlayer : ModPlayer
{
    public const int FervorMax = 100;
    private const int FervorPerHit = 5, FervorDuration = 8 * 60;
    private const float FervorDamage = .30f, FervorSpeed = .20f, FervorSteal = .02f, GuardianReduction = .15f;
    private const int RangerInterval = 10, SniperInterval = 6, GunnerBolts = 3;
    private const float RangerDamage = 3f;
    private const int InfiniteDuration = 5 * 60, InfiniteCooldown = 45 * 60;
    private const float ArcanistDamage = .25f, MysticHeal = .20f, BeastmasterCrit = 1.25f;

    // Stable names, not runtime NPC type IDs: safe across Calamity updates/removal.
    public static readonly string[] SourceNames = { "Providence", "CeaselessVoid", "StormWeaverHead", "Signus", "Polterghast" };
    public HashSet<string> EssenceSources = new();
    public int Fervor { get; private set; }
    public int FervorTime { get; private set; }
    public float FervorProgress => FervorTime > 0 ? FervorTime / (float)FervorDuration : Fervor / (float)FervorMax;
    private int shots, infiniteTime, infiniteCooldown, lastClass;
    private float stolenLife;
    private RPGPlayer Rpg => Player.GetModPlayer<RPGPlayer>();
    private bool Active(int classId) => Rpg.Awakened && Rpg.ClassId == classId;
    internal bool InfiniteMana => Active(ClassIds.Mage) && infiniteTime > 0;
    internal bool CanSupplyMana => Active(ClassIds.Mage) && (infiniteTime > 0 || infiniteCooldown == 0);

    public override void Initialize() => EssenceSources = new HashSet<string>();

    public override void SaveData(TagCompound tag) => tag["essenceSources"] = new List<string>(EssenceSources);

    public override void LoadData(TagCompound tag)
    {
        EssenceSources.Clear();
        // Ignore malformed values, unknown bosses and duplicate entries from old saves.
        try
        {
            if (!tag.TryGet("essenceSources", out List<string> saved) || saved == null) return;
            foreach (string name in saved)
                if (Array.IndexOf(SourceNames, name) >= 0) EssenceSources.Add(name);
        }
        catch (IOException) { } // Wrong tag type in a damaged/older save: keep an empty set.
    }

    public bool ClaimSource(int source)
        => source >= 0 && source < SourceNames.Length && EssenceSources.Add(SourceNames[source]);

    public override void PostUpdateEquips()
    {
        if (infiniteCooldown > 0) infiniteCooldown--;
        if (!Rpg.Awakened || lastClass != Rpg.ClassId)
            ClearEffects();
        lastClass = Rpg.ClassId;
        if (FervorTime > 0) FervorTime--;
        if (infiniteTime > 0) infiniteTime--;
        if (Active(ClassIds.Warrior) && FervorTime > 0)
        {
            Player.GetDamage(DamageClass.Melee) += FervorDamage;
            Player.GetAttackSpeed(DamageClass.Melee) += FervorSpeed;
            if (Rpg.Has("guardian")) Player.endurance += GuardianReduction;
        }
        if (InfiniteMana && Rpg.Has("arcanist")) Player.GetDamage(DamageClass.Magic) += ArcanistDamage;
        if (Active(ClassIds.Summoner)) Player.maxMinions += Rpg.Has("swarm_lord") ? 2 : 1;
    }

    public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
    {
        if (InfiniteMana && item.CountsAsClass(DamageClass.Magic)) mult = 0f;
    }

    // Also called by MageNodePlayer before its blood-magic fallback, independent of hook order.
    internal bool SupplyAwakenedMana(Item item, int neededMana)
    {
        if (!Active(ClassIds.Mage) || !item.CountsAsClass(DamageClass.Magic) || Player.whoAmI != Main.myPlayer) return false;
        if (infiniteTime <= 0)
        {
            if (infiniteCooldown > 0) return false;
            infiniteTime = InfiniteDuration;
            infiniteCooldown = InfiniteCooldown;
            if (Rpg.Has("mystic")) Player.Heal((int)(Player.statLifeMax2 * MysticHeal));
        }
        // CheckMana cached this cast's cost before ModifyManaCost could make it free.
        Player.statMana = Math.Max(Player.statMana, neededMana);
        return true;
    }

    public override void OnMissingMana(Item item, int neededMana) => SupplyAwakenedMana(item, neededMana);

    public override void OnConsumeMana(Item item, int manaConsumed)
    {
        if (Active(ClassIds.Mage) && item.CountsAsClass(DamageClass.Magic) && Player.statMana <= manaConsumed)
            SupplyAwakenedMana(item, manaConsumed);
    }

    public override void PostUpdate()
    {
        if (Active(ClassIds.Mage) && Player.statMana <= 0 && infiniteTime == 0 && infiniteCooldown == 0 && Player.whoAmI == Main.myPlayer)
            SupplyAwakenedMana(Player.HeldItem, 0);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!Active(ClassIds.Warrior) || !hit.DamageType.CountsAsClass(DamageClass.Melee) || Player.whoAmI != Main.myPlayer || target.lifeMax <= 5 || target.friendly) return;
        if (FervorTime == 0)
        {
            Fervor = Math.Min(FervorMax, Fervor + FervorPerHit);
            if (Fervor == FervorMax) { FervorTime = FervorDuration; Fervor = 0; }
        }
        else if (Rpg.Has("berserker"))
        {
            stolenLife += damageDone * FervorSteal;
            int heal = (int)stolenLife;
            stolenLife -= heal;
            if (heal > 0) Player.Heal(heal);
        }
    }

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (!Active(ClassIds.Ranger) || !item.CountsAsClass(DamageClass.Ranged) || Player.whoAmI != Main.myPlayer) return true;
        if (++shots < (Rpg.Has("sniper") ? SniperInterval : RangerInterval)) return true;
        shots = 0;
        int count = Rpg.Has("gunner") ? GunnerBolts : 1;
        int boltDamage = Math.Max(1, (int)(damage * (count == 1 ? RangerDamage : 1f)));
        Vector2 direction = velocity.SafeNormalize(Vector2.UnitX * Player.direction);
        for (int i = 0; i < count; i++)
        {
            Vector2 boltVelocity = direction.RotatedBy((i - (count - 1) / 2f) * .15f) * AwakeningBolt.Speed;
            Projectile.NewProjectile(source, position, boltVelocity, ModContent.ProjectileType<AwakeningBolt>(), boltDamage, knockback, Player.whoAmI);
        }
        return true;
    }

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!Active(ClassIds.Summoner) || !SummonerNodePlayer.MinionHit(proj)) return;
        float chance = Player.GetTotalCritChance(DamageClass.Generic);
        if (Main.rand.NextFloat(100f) < chance) modifiers.SetCrit();
        if (Rpg.Has("beastmaster")) modifiers.CritDamage *= BeastmasterCrit;
    }

    public override void UpdateDead()
    {
        if (infiniteCooldown > 0) infiniteCooldown--;
        ClearEffects();
    }

    private void ClearEffects() { Fervor = FervorTime = shots = infiniteTime = 0; stolenLife = 0f; }
}
