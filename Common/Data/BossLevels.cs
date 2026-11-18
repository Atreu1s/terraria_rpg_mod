using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Common.Data;

/// <summary>
/// Таблица уровней боссов (DESIGN §2). Share — доля награды, если босс состоит
/// из нескольких отдельно убиваемых NPC (Близнецы, Leviathan + Anahita и т. п.).
/// </summary>
public readonly record struct BossEntry(int Level, float Share = 1f);

public class BossLevels : ModSystem
{
    private static readonly Dictionary<int, BossEntry> byType = new();

    /// <summary>Сегменты Пожирателя миров: награда выдаётся, когда умирает последний.</summary>
    public static readonly HashSet<int> EaterOfWorldsTypes = new()
    {
        NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail,
    };

    /// <summary>
    /// Внутренние имена ModNPC Calamity (сверены с CalamityModPublic, ветка 1.4.4,
    /// папка NPCs/). Если NPC с таким именем не найден — запись просто пропускается.
    /// </summary>
    private static readonly (string Name, int Level, float Share)[] calamityBosses =
    {
        ("DesertScourgeHead", 6, 1f),
        ("Crabulon", 9, 1f),
        ("HiveMind", 13, 1f),
        ("PerforatorHive", 13, 1f),
        ("SlimeGodCore", 20, 1f),
        ("Cryogen", 30, 1f),
        ("AquaticScourgeHead", 32, 1f),
        ("BrimstoneElemental", 36, 1f),
        ("CalamitasClone", 52, 1f),
        ("Leviathan", 57, 0.5f),
        ("Anahita", 57, 0.5f),
        ("AstrumAureus", 58, 1f),
        ("PlaguebringerGoliath", 62, 1f),
        ("RavagerBody", 65, 1f),
        ("AstrumDeusHead", 70, 1f),
        ("ProfanedGuardianCommander", 78, 1f / 3f),
        ("ProfanedGuardianDefender", 78, 1f / 3f),
        ("ProfanedGuardianHealer", 78, 1f / 3f),
        ("Dragonfolly", 79, 1f),
        ("Providence", 82, 1f),
        ("StormWeaverHead", 85, 1f),
        ("CeaselessVoid", 85, 1f),
        ("Signus", 85, 1f),
        ("Polterghast", 88, 1f),
        ("OldDuke", 90, 1f),
        ("DevourerofGodsHead", 95, 1f),
        ("Yharon", 100, 1f),
        ("AresBody", 108, 1f / 3f),
        ("ThanatosHead", 108, 1f / 3f),
        ("Artemis", 108, 1f / 6f),
        ("Apollo", 108, 1f / 6f),
        ("SupremeCalamitas", 110, 1f),
    };

    public static bool CalamityLoaded { get; private set; }

    public override void PostSetupContent()
    {
        byType.Clear();

        // Ваниль
        Add(NPCID.KingSlime, 5);
        Add(NPCID.EyeofCthulhu, 7);
        foreach (int t in EaterOfWorldsTypes)
            Add(t, 12);
        Add(NPCID.BrainofCthulhu, 12);
        Add(NPCID.QueenBee, 15);
        Add(NPCID.SkeletronHead, 17);
        Add(NPCID.Deerclops, 17);
        Add(NPCID.WallofFlesh, 25);
        Add(NPCID.QueenSlimeBoss, 28);
        Add(NPCID.TheDestroyer, 42);
        Add(NPCID.Retinazer, 44, 0.5f);
        Add(NPCID.Spazmatism, 44, 0.5f);
        Add(NPCID.SkeletronPrime, 46);
        Add(NPCID.Plantera, 55);
        Add(NPCID.Golem, 60);
        Add(NPCID.DukeFishron, 63);
        Add(NPCID.HallowBoss, 64);
        Add(NPCID.CultistBoss, 68);
        Add(NPCID.MoonLordCore, 75);

        // Calamity (мягкая зависимость)
        CalamityLoaded = ModLoader.TryGetMod("CalamityMod", out Mod calamity);
        if (CalamityLoaded)
        {
            foreach (var (name, level, share) in calamityBosses)
            {
                if (calamity.TryFind(name, out ModNPC modNpc))
                    Add(modNpc.Type, level, share);
                else
                    Mod.Logger.Warn($"Calamity boss '{name}' not found, its level will use the generic formula.");
            }
        }
    }

    public override void Unload() => byType.Clear();

    private static void Add(int type, int level, float share = 1f) => byType[type] = new BossEntry(level, share);

    public static bool TryGet(int type, out BossEntry entry) => byType.TryGetValue(type, out entry);

    /// <summary>NPC принадлежит Calamity (для правила «боссы неизвестных модов × 5»).</summary>
    public static bool IsFromCalamity(NPC npc) => npc.ModNPC?.Mod.Name == "CalamityMod";
}
