using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Daybreak.Common.UI;
using System.Collections.Generic;

namespace AdventureTools.UI;
public enum ResizeDirection : byte
{
    None = 0,
    Left = 1,
    Right = 2,
    Up = 4,
    Down = 8,
    DownLeft = Left | Down,
    UpRight = Right | Up,
}

public sealed class DynamicPanel : UIPanel
{
    private Vector2 offset;
    private bool dragging;
    public ResizeDirection dir;
    public static DynamicPanel HoverPanel;
    public bool SelfIsHandle;
    private bool resizing;
    public HashSet<UIElement> Handles;
    
    public DynamicPanel(bool selfIsHandle = true, params UIElement[] otherHandles)
    {
        if (SelfIsHandle = selfIsHandle)
            Handles = [this];
        else
            Handles = [];
        foreach (var item in otherHandles)
            Handles.Add(item);
    }
    public override void MouseOver(UIMouseEvent evt)
    {
        if (!resizing)
            HoverPanel = this;
        base.MouseOver(evt);
    }
    public override void MouseOut(UIMouseEvent evt)
    {
        if (HoverPanel == this)
            HoverPanel = null;
        base.MouseOut(evt);
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        DragStart(evt);
        base.LeftMouseDown(evt);
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        DragEnd(evt);
        base.LeftMouseUp(evt);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!resizing)
            dir = 0;
        if (!ContainsPoint(Main.MouseScreen) || resizing)
            return;
        var resizeSpace = 12f;
        var normalized = Main.MouseScreen - GetDimensions().Position();
        if (normalized.X < resizeSpace)
            dir |= ResizeDirection.Left;
        else if (normalized.X > GetDimensions().Width - resizeSpace)
            dir |= ResizeDirection.Right;
        if (normalized.Y < resizeSpace)
            dir |= ResizeDirection.Up;
        else if (normalized.Y > GetDimensions().Height - resizeSpace)
            dir |= ResizeDirection.Down;
    }
    public void ResetHandles()
    {
        Handles.Clear();
        if (SelfIsHandle)
            Handles.Add(this);
    }
    private void DragStart(UIMouseEvent evt)
    {
        CalculatedStyle innerDimensions = GetInnerDimensions();
        if (!Handles.Contains(evt.Target))
            return;
        if (dir != 0)
        {
            offset = new Vector2(evt.MousePosition.X - innerDimensions.X - innerDimensions.Width - 6, evt.MousePosition.Y - innerDimensions.Y - innerDimensions.Height - 6);
            resizing = true;
        }
        else
        {
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }
    }

    private void DragEnd(UIMouseEvent evt)
    {
        dragging = resizing = false;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = base.GetOuterDimensions();
        if (ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
            Main.LocalPlayer.cursorItemIconEnabled = false;
            Main.ItemIconCacheUpdate(0);
        }
        if (dragging)
        {
            Left.Set(Main.MouseScreen.X - offset.X, 0f);
            Top.Set(Main.MouseScreen.Y - offset.Y, 0f);
            Recalculate();
        }
        else
        {
            if (Parent != null && !dimensions.ToRectangle().Intersects(Parent.Dimensions))
            {
                var parentSpace = Parent.GetDimensions().ToRectangle();
                Left.Pixels = Utils.Clamp(Left.Pixels, Width.Pixels - parentSpace.Right, 0);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }
        }
        if (resizing)
        {
            if ((dir & ResizeDirection.Left) != 0)
            {
                var widthAfterMoving = Width.Pixels + Left.Pixels - Main.MouseScreen.X;
                if (widthAfterMoving >= MinWidth.Pixels)
                {
                    Left.Pixels = Main.MouseScreen.X;
                    Width.Pixels = widthAfterMoving;
                }
            }
            else if ((dir & ResizeDirection.Right) != 0)
                Width.Pixels = Main.MouseScreen.X - dimensions.X - offset.X;
            if ((dir & ResizeDirection.Up) != 0)
            {
                var heightAfterMoving = Height.Pixels + Top.Pixels - Main.MouseScreen.Y;
                if (heightAfterMoving >= MinHeight.Pixels)
                {
                    Top.Pixels = Main.MouseScreen.Y;
                    Height.Pixels = heightAfterMoving;
                }
            }
            else if ((dir & ResizeDirection.Down) != 0)
                Height.Pixels = Main.MouseScreen.Y - dimensions.Y - offset.Y;
            Recalculate();
        }
        base.DrawSelf(spriteBatch);
    }
}
