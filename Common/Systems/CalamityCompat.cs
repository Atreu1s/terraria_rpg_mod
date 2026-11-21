using Terraria.ModLoader;

namespace RPGMod.Common.Systems;

/// <summary>Soft dependency only: no Calamity assembly reference.</summary>
public class CalamityCompat : ModSystem
{
    public static bool Enabled { get; private set; }
    public static Mod Calamity { get; private set; }
    public static DamageClass Rogue { get; private set; }
    public static bool RogueAvailable => Rogue != null;
    public static bool AwakeningAvailable => Enabled;

    public override void Load()
    {
        Enabled = ModLoader.HasMod("CalamityMod");
        if (ModLoader.TryGetMod("CalamityMod", out var calamity))
        {
            Calamity = calamity;
            if (calamity.TryFind<DamageClass>("RogueDamageClass", out var rogue))
                Rogue = rogue;
            else
                Mod.Logger.Warn("Calamity is loaded, but RogueDamageClass was not found; Rogue and Cunning are unavailable.");
        }
    }

    public override void Unload()
    {
        Enabled = false;
        Calamity = null;
        Rogue = null;
    }
}
