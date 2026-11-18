using System;
using Microsoft.Xna.Framework;
using RPGMod.Common.Config;
using RPGMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RPGMod.Common.Commands;

/// <summary>
/// Отладочная команда /rpg. Работает только при RPGConfig.DebugCommands = true.
///   /rpg                 — показать состояние
///   /rpg level N         — установить уровень
///   /rpg xp N            — начислить N опыта (с повышением уровня)
///   /rpg points          — показать очки статов и древа
///   /rpg downed wof|mech|moonlord|all|none — флаги боссов мира
///   /rpg reset           — сбросить весь прогресс персонажа
/// </summary>
public class RPGCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "rpg";
    public override string Usage => "/rpg [level N | xp N | points | downed wof|mech|moonlord|all|none | reset]";
    public override string Description => Language.GetTextValue("Mods.RPGMod.Commands.Description");

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (!RPGConfig.Instance.DebugCommands)
        {
            caller.Reply(Language.GetTextValue("Mods.RPGMod.Commands.Disabled"), Color.OrangeRed);
            return;
        }

        var rpg = caller.Player.GetModPlayer<RPGPlayer>();
        string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "";

        switch (sub)
        {
            case "level" when args.Length > 1 && int.TryParse(args[1], out int lvl):
                rpg.Level = Math.Max(1, lvl);
                rpg.XP = 0;
                break;

            case "xp" when args.Length > 1 && long.TryParse(args[1], out long xp):
                rpg.GainXP(xp);
                break;

            case "points":
                break; // просто покажем состояние ниже

            case "downed" when args.Length > 1:
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket p = Mod.GetPacket();
                    p.Write((byte)MessageType.DebugDowned);
                    p.Write(args[1].ToLowerInvariant());
                    p.Send();
                }
                else
                {
                    ApplyDowned(args[1].ToLowerInvariant());
                }
                break;

            case "reset":
                rpg.ResetProgress();
                break;

            case "":
                break;

            default:
                throw new UsageException(Usage);
        }

        caller.Reply(Language.GetTextValue("Mods.RPGMod.Commands.State",
            rpg.Level, rpg.XP, rpg.XPToNext, rpg.ClassId, rpg.Picked.Count,
            Main.hardMode, NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3, NPC.downedMoonlord),
            Color.LightGreen);
    }

    /// <summary>Выставить флаги боссов мира. На сервере после вызова нужно разослать WorldData.</summary>
    public static void ApplyDowned(string what)
    {
        bool wof = false, mech = false, moon = false;
        switch (what)
        {
            case "wof": wof = true; break;
            case "mech": wof = mech = true; break;
            case "moonlord": wof = mech = moon = true; break;
            case "all": wof = mech = moon = true; break;
            case "none": break;
            default: return;
        }
        Main.hardMode = wof;
        NPC.downedMechBoss1 = NPC.downedMechBoss2 = NPC.downedMechBoss3 = mech;
        NPC.downedMoonlord = moon;
    }
}
