using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace AdventureTools.Core;

public abstract class LocalizedCommand : ModCommand, ILocalizedModType
{
    public string LocalizationCategory { get; } = "Commands";
    public static readonly Color Error = Color.Red;
}
