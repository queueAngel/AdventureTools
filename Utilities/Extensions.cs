using Terraria;

namespace AdventureTools.Utilities;

public static class Extensions
{
    public static CustomBiomePlayer Biomes(this Player p) => p.GetModPlayer<CustomBiomePlayer>();
}
