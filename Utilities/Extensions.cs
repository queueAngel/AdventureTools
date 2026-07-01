using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.UI;

namespace AdventureTools.Utilities;

public static class Extensions
{
    public static CustomBiomePlayer Biomes(this Player p) => p.GetModPlayer<CustomBiomePlayer>();
    public static CalculatedStyle ToDims(this Rectangle r) => new(r.X, r.Y, r.Width, r.Height);
    extension(StyleDimension)
    {
        public static StyleDimension Quarter => StyleDimension.FromPercent(0.25f);
        public static StyleDimension Third => StyleDimension.FromPercent(1f / 3f);
        public static StyleDimension Half => StyleDimension.FromPercent(0.5f);
        public static StyleDimension operator +(StyleDimension a, float b) => new(a.Pixels + b, a.Percent);
        public static StyleDimension operator -(StyleDimension a, float b) => new(a.Pixels - b, a.Percent);
        public static StyleDimension operator *(StyleDimension a, float b) => new(a.Pixels * b, a.Percent * b);
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
    extension(MathHelper)
    {
        public static float HermiteZero(float value1, float value2, float amount)
        {
            double num = value1;
            double num2 = value2;
            double num5 = amount;
            double num6 = num5 * num5 * num5;
            double num7 = num5 * num5;
            double num8 = MathHelper.WithinEpsilon(amount, 0f) ? ((double)value1) : ((!MathHelper.WithinEpsilon(amount, 1f)) ? ((2.0 * num - 2.0 * num2) * num6 + (3.0 * num2 - 3.0 * num) * num7 + num) : ((double)value2));
            return (float)num8;
        }
    }
}
