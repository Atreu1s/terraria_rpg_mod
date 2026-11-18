using System.IO;
using RPGMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RPGMod.Common;

/// <summary>Типы сетевых сообщений мода (первый байт каждого ModPacket).</summary>
public enum MessageType : byte
{
    /// <summary>Сервер → клиент: «тебе +N опыта» (клиент начисляет сам).</summary>
    GainXP,
    /// <summary>Клиент ↔ сервер: полное состояние RPGPlayer (сервер пересылает остальным).</summary>
    SyncPlayer,
    /// <summary>Клиент → сервер: отладочная установка флагов боссов (/rpg downed).</summary>
    DebugDowned,
    /// <summary>Клиент ↔ сервер: стаки «Тысячи укусов» и метки хлыста призывателя на NPC.</summary>
    SummonerStacks,
}

/// <summary>
/// Обработка входящих пакетов. Аналогия: switch по action.type в reducer'е.
/// </summary>
public static class Networking
{
    public static void HandlePacket(Mod mod, BinaryReader reader, int whoAmI)
    {
        var type = (MessageType)reader.ReadByte();
        switch (type)
        {
            case MessageType.GainXP:
            {
                long amount = reader.ReadInt64();
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    Main.LocalPlayer.GetModPlayer<RPGPlayer>().GainXP(amount);
                break;
            }
            case MessageType.SyncPlayer:
            {
                byte playerIndex = reader.ReadByte();
                // На сервере доверяем только индексу отправителя.
                if (Main.netMode == NetmodeID.Server)
                    playerIndex = (byte)whoAmI;

                var rpg = Main.player[playerIndex].GetModPlayer<RPGPlayer>();
                rpg.ReceiveSync(reader);

                // Сервер пересылает изменения остальным клиентам.
                if (Main.netMode == NetmodeID.Server)
                    rpg.SyncPlayer(toWho: -1, fromWho: whoAmI, newPlayer: false);
                break;
            }
            case MessageType.DebugDowned:
            {
                string what = reader.ReadString();
                if (Main.netMode == NetmodeID.Server && Config.RPGConfig.Instance.DebugCommands)
                {
                    Commands.RPGCommand.ApplyDowned(what);
                    NetMessage.SendData(MessageID.WorldData);
                }
                break;
            }
            case MessageType.SummonerStacks:
                Players.Nodes.SummonerNodeNetwork.Receive(reader, whoAmI);
                break;
            default:
                mod.Logger.Warn($"Unknown RPGMod packet type: {type}");
                break;
        }
    }

    /// <summary>Отправить игроку опыт: в одиночной игре — сразу, на сервере — пакетом.</summary>
    public static void GiveXP(Mod mod, Player player, long amount)
    {
        if (amount <= 0)
            return;

        if (Main.netMode == NetmodeID.Server)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.GainXP);
            packet.Write(amount);
            packet.Send(toClient: player.whoAmI);
        }
        else if (Main.netMode == NetmodeID.SinglePlayer)
        {
            player.GetModPlayer<RPGPlayer>().GainXP(amount);
        }
    }
}
