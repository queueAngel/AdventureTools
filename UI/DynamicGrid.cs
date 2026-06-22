using AdventureTools.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.UI;

namespace AdventureTools.UI;

// bajically just something that allows making grids with elements that stay a constant pixel size without it lookin weird
public sealed class DynamicGrid : UIElement
{
    private CalculatedStyle _last;
    private int _lastCount;
    public float XSpacing;
    public float YSpacing;
    public float ElementWidth;
    public float ElementHeight;
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!_last.SameSize(_dimensions) || _lastCount != Elements.Count)
            RecalculateElements();
        _last = _dimensions;
        _lastCount = Elements.Count;
        base.Draw(spriteBatch);
    }
    public void RecalculateElements()
    {
        var calcWidth = ElementWidth + XSpacing;
        var fitInRow = (int)(_dimensions.Width / calcWidth);
        var finalWidth = fitInRow * calcWidth;
        var xAdjust = (_dimensions.Width - finalWidth) * 0.5f;
        var x = 0;
        var y = 0;
        foreach (var element in CollectionsMarshal.AsSpan(Elements))
        {
            element.Left.Pixels = x * calcWidth + xAdjust;
            element.Top.Pixels = y * (ElementHeight + YSpacing);
            if (++x == fitInRow)
            {
                x = 0;
                y++;
            }
        }
    }
    public void Add(UIElement element)
    {
        element.Width.Pixels = ElementWidth;
        element.Height.Pixels = ElementHeight;
        Append(element);
        RecalculateElements();
    }
    public void Add(IEnumerable<UIElement> elements)
    {
        foreach (var element in elements)
        {
            element.Width.Pixels = ElementWidth;
            element.Height.Pixels = ElementHeight;
            Append(element);
        }
        RecalculateElements();
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {

    }
}
