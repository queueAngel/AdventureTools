using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.GameContent;
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
        // var fitInRow = (int)((_dimensions.Width - XSpacing) / calcWidth);
        var curTop = 0f;
        var curLeft = 0f;
        foreach (var element in CollectionsMarshal.AsSpan(Elements))
        {
            if (curLeft + ElementWidth >= _dimensions.Width)
            {
                curLeft = 0f;
                curTop += ElementHeight + YSpacing;
            }
            element.Left.Pixels = curLeft;
            element.Top.Pixels = curTop;
            curLeft += calcWidth;
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
