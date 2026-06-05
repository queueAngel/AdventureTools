using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.Rendering;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class EquipPicker : UIElement
{
    public Player Dummy;
    public EquipType Type;
    public StyleDimension ButtonHeight;
    public StyleDimension MenuHeight;
    private float _anim;
    public bool Open;
    private bool _lastOpen;
    public bool All;
    public override void OnInitialize()
    {
        
    }
    public override void Update(GameTime gameTime)
    {
        var oldAnim = _anim;
        _anim = _anim < 0.01f && !Open ? 0f : MathHelper.HermiteZero(_anim, Open ? 1f : 0f, 0.3f);
        if (_anim != oldAnim)
        {
            Height = new(ButtonHeight.Pixels + (MenuHeight.Pixels * _anim), ButtonHeight.Percent + (MenuHeight.Percent * _anim));
            Recalculate();
        }
        _smoothScroll = float.Lerp(_smoothScroll, _scrollPosition / 120f, 0.2f);
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (evt.MousePosition.Y - _dimensions.Y < ButtonHeight.Pixels)
            Open ^= true;
    }
    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        _scrollPosition += evt.ScrollWheelValue;
        ValidateScrollPosition();
    }
    private void ValidateScrollPosition()
    {
        var lastPos = -(_scrollPosition / 120 - (int)MathF.Ceiling(_gridSpace.Height / _height));
        if (lastPos > _rows)
            _scrollPosition += 120 * (lastPos - _rows);
        if (_scrollPosition > 0)
            _scrollPosition = 0;
    }
    private const float _width = 42f;
    private const float _height = 64f;
    private int _scrollPosition;
    private float _smoothScroll;
    private CalculatedStyle _preDims;
    private Rectangle _gridSpace;
    private int _fit;
    private int _rows;
    public override void Recalculate()
    {
        var prevHeight = Height;
        Height = ButtonHeight;
        base.Recalculate();
        _preDims = _dimensions;
        Height = prevHeight;
        base.Recalculate();
        var postDims = _dimensions;
        _gridSpace = new((int)_preDims.X, (int)(_preDims.Y + _preDims.Height), (int)_preDims.Width, (int)(postDims.Height - _preDims.Height));
        _fit = (int)(_gridSpace.Width / _width);
        _rows = (int)MathF.Ceiling(EquipLoader.GetSearch(Type)._idToName.Count / (float)_fit);
        ValidateScrollPosition();
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var pTex = TextureAssets.SettingsPanel.Value;
        ConfigElement.DrawPanel2(spriteBatch, _preDims.Position(), pTex, _preDims.Width, _preDims.Height, UICommon.DefaultUIBlue);
        if (_anim <= float.Epsilon)
            return;
        var x = 0;
        var y = 0;
        var postDims = _dimensions;
        var gridTl = _gridSpace.TopLeft();
        ConfigElement.DrawPanel2(spriteBatch, gridTl, pTex, postDims.Width, _gridSpace.Height, UICommon.DefaultUIBlue);

        spriteBatch.End(out var ss);
        var g = spriteBatch.graphicsDevice;
        var oldScissor = g.ScissorRectangle;
        var oldRasterizer = g.RasterizerState;
        var clipping = GetClippingRectangle(spriteBatch);
        var adjust = (int)_preDims.Height + 8;
        clipping.Y += adjust;
        clipping.Height -= adjust + 3;

        g.ScissorRectangle = Rectangle.Intersect(clipping, oldScissor);
        g.RasterizerState = OverflowHiddenRasterizerState;
        spriteBatch.Begin(ss with { RasterizerState = OverflowHiddenRasterizerState });

        if (All)
        {
            ref var equip = ref WorldNPC.GetPlayerEquip(Dummy, Type);
            var old = equip;
            var finalWidth = _fit * _width;
            var xAdjust = (_preDims.Width - finalWidth) * 0.5f;
            var search = EquipLoader.GetSearch(Type);
            foreach (var slot in search._idToName.Keys)
            {
                equip = slot;
                var pos = gridTl + new Vector2(x * _width + xAdjust, (y + _smoothScroll) * _height);
                if (pos.Y + 2f < _gridSpace.Bottom && pos.Y + _height - 2f > _gridSpace.Top)
                {
                    var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)_width, (int)_height);
                    if (slot == old)
                    {
                        DrawUtils.Draw9Slice(spriteBatch, TextButton.HighlightTex.Value, rect, Color.White);
                        spriteBatch.FlushBatch();
                    }
                    if (rect.Contains(Main.mouseX, Main.mouseY))
                    {
                        if (Main.mouseLeft && Main.mouseLeftRelease)
                            CadastralUIState.AppearanceNode[Type.ToString()] = search.GetName(slot);
                        DrawUtils.Draw9Slice(spriteBatch, TextButton.BorderTex.Value, rect, Color.White);
                        spriteBatch.FlushBatch();
                    }

                    var playerPosition = pos + new Vector2(24f - Dummy.width * 0.5f, 36f - Dummy.height * 0.5f);
                    Main.PlayerRenderer.DrawPlayer(Main.Camera, Dummy, playerPosition + Main.screenPosition, 0f, Vector2.Zero, 0f, 1f);
                }
                if (++x == _fit)
                {
                    x = 0;
                    y++;
                }
            }
            equip = old;
        }
        else
        {
            foreach (var item in EquipLoader.slotToId[Type].Values)
            {

            }
        }

        spriteBatch.End();
        g.ScissorRectangle = oldScissor;
        g.RasterizerState = oldRasterizer;
        spriteBatch.Begin(in ss);
    }
}
