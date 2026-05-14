using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AdventureTools.Utilities;

public static class Extensions
{
    public static CustomBiomePlayer Biomes(this Player p) => p.GetModPlayer<CustomBiomePlayer>();
}
