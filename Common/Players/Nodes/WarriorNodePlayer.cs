using System;
using Microsoft.Xna.Framework;
using RPGMod.Common.Data;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RPGMod.Common.Players.Nodes;

/// <summary>
/// Условные эффекты Воина, тиры 2–3 (DESIGN §4). Отдельный ModPlayer на класс,
/// чтобы логика узлов не раздувала RPGPlayer. Состояние (стаки, таймеры) — локальное.
/// </summary>
public class WarriorNodePlayer : ModPlayer
{
    // ---- Числа узлов (DESIGN §4) ----
    private const float BloodRageMax = 0.25f, BloodRageThreshold = 0.30f;
    private const float FrenzySpeedPerStack = 0.02f;
    private const int FrenzyDuration = 3 * 60, FrenzyMaxStacks = 10;
    private const float IronFleshPerFiveDef = 0.01f, IronFleshMax = 0.25f;
    private const int RetributionWindow = 3 * 60;
    private const float RetributionBonus = 0.30f;
    private const float PossessedThreshold = 0.25f, PossessedSteal = 0.03f, PossessedPerSecond = 10f;
    private const int UnbreakableCooldown = 5 * 60 * 60, UnbreakableImmune = 3 * 60;
    private const float WhirlwindDamage = 0.15f, WhirlwindMove = 0.15f;
    private const int IronRhythmDuration = 6 * 60, IronRhythmMaxStacks = 6;
    private const float IronRhythmDamagePerStack = 0.02f;
    private const float ColossusLife = 0.15f, ColossusDRPer100 = 0.02f, ColossusDRMax = 0.10f;
    private const int WillShieldDelay = 8 * 60;
    private const float WillShieldFraction = 0.15f;
    private const float EyeForEyeHeal = 0.05f;
    private const float CounterRange = 10 * 16f;
    private const int CounterCooldown = 2 * 60;

    // ---- Состояние ----
    public int FrenzyStacks;
    private int frenzyTimer;
    private int retributionTimer;
    private bool retributionThisHit;
    private float possessedBudget, possessedFraction;
    public int UnbreakableCooldownLeft;
    private int noHurtTimer;
    public int ShieldHP;
    private int counterCooldownLeft;

    private RPGPlayer Rpg => Player.GetModPlayer<RPGPlayer>();
    private bool Has(string id) => Rpg.ClassId == ClassIds.Warrior && Rpg.Has(id);

    private int MaxFrenzyStacks => Has("iron_rhythm") ? IronRhythmMaxStacks : FrenzyMaxStacks;

    public override void PostUpdateEquips()
    {
        if (Rpg.ClassId != ClassIds.Warrior)
            return;

        // Кровавая ярость: линейно от 0 при полном HP до максимума при HP ≤ 30%
        if (Has("blood_rage"))
        {
            float lifeFrac = Player.statLife / (float)Math.Max(1, Player.statLifeMax2);
            float t = Utils.Clamp((1f - lifeFrac) / (1f - BloodRageThreshold), 0f, 1f);
            Player.GetDamage(DamageClass.Melee) += BloodRageMax * t;
        }

        if (Has("frenzy") && FrenzyStacks > 0)
        {
            Player.GetAttackSpeed(DamageClass.Melee) += FrenzySpeedPerStack * FrenzyStacks;
            if (Has("iron_rhythm"))
                Player.GetDamage(DamageClass.Melee) += IronRhythmDamagePerStack * FrenzyStacks;
            if (Has("whirlwind") && FrenzyStacks >= FrenzyMaxStacks)
            {
                Player.GetDamage(DamageClass.Melee) += WhirlwindDamage;
                Player.moveSpeed += WhirlwindMove;
            }
        }

        if (Has("iron_flesh"))
        {
            int defense = Player.statDefense;
            Player.GetDamage(DamageClass.Melee) += Math.Min(IronFleshMax, defense / 5 * IronFleshPerFiveDef);
        }

        if (Has("will_shield"))
            Player.noKnockback = true;
    }

    public override void PostUpdateMiscEffects()
    {
        // Колосс считается после всех прибавок к HP
        if (Has("colossus"))
        {
            Player.statLifeMax2 = (int)(Player.statLifeMax2 * (1f + ColossusLife));
            Player.endurance += Math.Min(ColossusDRMax, Player.statLifeMax2 / 100 * ColossusDRPer100);
        }
    }

    public override void PostUpdate()
    {
        if (frenzyTimer > 0 && --frenzyTimer == 0)
            FrenzyStacks = 0;
        if (retributionTimer > 0)
            retributionTimer--;
        if (UnbreakableCooldownLeft > 0)
            UnbreakableCooldownLeft--;
        if (counterCooldownLeft > 0)
            counterCooldownLeft--;

        // Бюджет вампиризма: 10 HP в секунду
        possessedBudget = Math.Min(PossessedPerSecond, possessedBudget + PossessedPerSecond / 60f);

        // Щит воли: 8 с без урона → щит на 15% макс. HP
        if (Has("will_shield"))
        {
            noHurtTimer++;
            if (noHurtTimer >= WillShieldDelay && ShieldHP <= 0)
            {
                ShieldHP = (int)(Player.statLifeMax2 * WillShieldFraction);
                if (Player.whoAmI == Main.myPlayer)
                    CombatText.NewText(Player.getRect(), new Color(150, 200, 255),
                        Language.GetTextValue("Mods.RPGMod.Nodes.will_shield.Name"));
            }
            if (ShieldHP > 0 && Main.rand.NextBool(12))
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.BlueFairy, 0f, 0f, 150, default, 0.8f);
        }
        else
        {
            ShieldHP = 0;
        }
    }

    // ---------- Атака ----------

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        retributionThisHit = false;
        if (Has("retribution") && retributionTimer > 0)
        {
            modifiers.FinalDamage *= 1f + RetributionBonus;
            retributionThisHit = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Rpg.ClassId != ClassIds.Warrior)
            return;

        if (Has("frenzy") && hit.DamageType.CountsAsClass(DamageClass.Melee))
        {
            FrenzyStacks = Math.Min(MaxFrenzyStacks, FrenzyStacks + 1);
            frenzyTimer = Has("iron_rhythm") ? IronRhythmDuration : FrenzyDuration;
        }

        if (retributionThisHit)
        {
            retributionThisHit = false;
            retributionTimer = 0;
            if (Has("eye_for_eye"))
                HealSafe((int)(damageDone * EyeForEyeHeal));
        }

        if (Has("possessed") && Player.statLife < Player.statLifeMax2 * PossessedThreshold)
        {
            possessedFraction += Math.Min(possessedBudget, damageDone * PossessedSteal);
            possessedBudget = Math.Max(0f, possessedBudget - damageDone * PossessedSteal);
            int heal = (int)possessedFraction;
            if (heal > 0)
            {
                possessedFraction -= heal;
                HealSafe(heal);
            }
        }
    }

    // ---------- Защита ----------

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (ShieldHP > 0)
        {
            modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) =>
            {
                int absorbed = Math.Min(ShieldHP, info.Damage - 1);
                if (absorbed > 0)
                {
                    info.Damage -= absorbed;
                    ShieldHP -= absorbed;
                }
                else
                {
                    ShieldHP = 0;
                }
            };
        }
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        noHurtTimer = 0;
        if (Has("retribution"))
            retributionTimer = RetributionWindow;

        if (Has("counterstrike") && counterCooldownLeft <= 0 && Player.whoAmI == Main.myPlayer)
        {
            counterCooldownLeft = CounterCooldown;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5 || npc.immortal)
                    continue;
                if (Vector2.Distance(npc.Center, Player.Center) > CounterRange)
                    continue;
                int dir = npc.Center.X > Player.Center.X ? 1 : -1;
                Player.ApplyDamageToNPC(npc, info.Damage, 0f, dir, false, DamageClass.Melee);
            }
        }
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        if (Has("unbreakable") && UnbreakableCooldownLeft <= 0)
        {
            UnbreakableCooldownLeft = UnbreakableCooldown;
            Player.statLife = 1;
            Player.SetImmuneTimeForAllTypes(UnbreakableImmune);
            CombatText.NewText(Player.getRect(), new Color(232, 96, 76),
                Language.GetTextValue("Mods.RPGMod.Nodes.unbreakable.Name"), dramatic: true);
            return false;
        }
        return true;
    }

    private void HealSafe(int amount)
    {
        if (amount > 0 && Player.whoAmI == Main.myPlayer && Player.statLife < Player.statLifeMax2)
            Player.Heal(amount);
    }
}
