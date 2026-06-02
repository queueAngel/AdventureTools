using System;
using Terraria;
using Terraria.UI;

namespace AdventureTools.Utilities;

public static class Extensions
{
    public static CustomBiomePlayer Biomes(this Player p) => p.GetModPlayer<CustomBiomePlayer>();
    extension(StyleDimension)
    {
        public static StyleDimension Half => StyleDimension.FromPercent(0.5f);
    }
    extension(CalculatedStyle self)
    {
        public unsafe bool SameSize(CalculatedStyle other)
        {
            var aP = *(ulong*)(void*)&self.Width;
            var bP = *(ulong*)(void*)&other.Width;
            return aP == bP;
        }
        public static unsafe bool operator ==(CalculatedStyle a, CalculatedStyle b)
        {
            var aP = *(UInt128*)(void*)&a.X;
            var bP = *(UInt128*)(void*)&b.X;
            return aP == bP;
        }
        public static bool operator !=(CalculatedStyle a, CalculatedStyle b) => !(a == b);
    }
}
