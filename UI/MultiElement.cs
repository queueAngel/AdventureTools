using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class MultiElement : UIElement
{
    public readonly List<UIElement> OtherContainers = [];
    public override bool ContainsPoint(Vector2 point) => base.ContainsPoint(point) || OtherContainers.Any(c => c.ContainsPoint(point));
}
