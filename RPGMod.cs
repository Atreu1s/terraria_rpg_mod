using System.IO;
using RPGMod.Common;
using Terraria.ModLoader;

namespace RPGMod;

/// <summary>Главный класс мода. Здесь только приём сетевых пакетов.</summary>
public class RPGMod : Mod
{
    public override void HandlePacket(BinaryReader reader, int whoAmI)
        => Networking.HandlePacket(this, reader, whoAmI);
}
