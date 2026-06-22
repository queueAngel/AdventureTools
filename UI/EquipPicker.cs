using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.Features.Hooks;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public enum EquipDisplayStyle
{
    FromSlot,
    FromItem,
    FromSlotOnlyWithItem
}
public sealed class EquipPicker : UIElement
{
    public static readonly List<int> Dyes = [];
    private static readonly Asset<Texture2D> _equipIcons = ModContent.Request<Texture2D>(nameof(AdventureTools) + "/EquipIcons");
    public UIScrollbar ScrollBarAttachedToList;
    public Player Dummy;
    public EquipType Type;
    public EquipDisplayStyle DisplayStyle;
    public StyleDimension ButtonHeight;
    public StyleDimension MenuHeight;
    private float _anim;
    public bool Open;
    public bool DyeMenu;
    public bool HideDyes;
    [ModSystemHooks.PostSetupContent]
    public static void InitDyesList()
    {
        Dyes.Add(ItemID.None);
        foreach (var item in ContentSamples.ItemsByType.Values)
            if (item.dye > 0)
                Dyes.Add(item.type);
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
        var yNormal = evt.MousePosition.Y - _dimensions.Y;
        if (yNormal > ButtonHeight.Pixels)
            return;
        var xNormal = evt.MousePosition.X - _dimensions.X;
        if (xNormal > _preDims.Width - 102f)
            DyeMenu ^= true;
        else if (xNormal > _preDims.Width - 155f
            && xNormal < _preDims.Width - 113f
            && yNormal < _preDims.Height * 0.8f
            && yNormal > _preDims.Height * 0.2f)
            HideDyes ^= true;
        else
            Open ^= true;
    }
    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        if (evt.MousePosition.Y < _preDims.Y + _preDims.Height)
        {
            ScrollBarAttachedToList?.ViewPosition -= evt.ScrollWheelValue;
            return;
        }
        _scrollPosition += evt.ScrollWheelValue;
        ValidateScrollPosition();
    }
    private void ValidateScrollPosition()
    {
        var lastPos = -(_scrollPosition / 120 - (int)MathF.Ceiling(_gridSpace.Height / HEIGHT));
        if (lastPos > _rows)
            _scrollPosition += 120 * (lastPos - _rows);
        if (_scrollPosition > 0)
            _scrollPosition = 0;
    }
    private const float _width = 42f;
    public const float HEIGHT = 64f;
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
        _rows = (int)MathF.Ceiling((DyeMenu ? Dyes.Count : EquipLoader.GetSearch(Type)._idToName.Count) / (float)_fit);
        ValidateScrollPosition();
    }
    private float _hideDyesToggleAnim;
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var pTex = TextureAssets.SettingsPanel.Value;
        ConfigElement.DrawPanel2(spriteBatch, _preDims.Position(), pTex, _preDims.Width - 1f, _preDims.Height, UICommon.DefaultUIBlue);
        DrawUtils.DrawCoolToggle(spriteBatch, in _preDims, ref _hideDyesToggleAnim, HideDyes, -118f);
        DrawUtils.Draw9Slice(spriteBatch, TextButton.Texture.Value, new Rectangle((int)(_preDims.X + _preDims.Width - 108f), (int)_preDims.Y + 4, 103, (int)_preDims.Height - 8));
        var frame = _equipIcons.Frame((int)EquipType.Beard + 1, frameX: (int)Type);
        var yCenter = _preDims.Y + _preDims.Height * 0.5f;
        var iconDraw = new DrawParameters(_equipIcons)
        {
            Source = frame,
            Origin = new(frame.Width * 0.5f, frame.Height * 0.5f),
            Position = new(_preDims.X + frame.Width + 4f, yCenter),
        };
        spriteBatch.Draw(iconDraw);
        var font = FontAssets.MouseText.Value;
        var text = AdventureTools.Instance.GetLocalization(nameof(EquipType) + "." + Type).Value;
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, new Vector2(iconDraw.Destination.Right - 8f, yCenter - ChatManager.GetStringSize(font, text, Vector2.One).Y * 0.35f), Color.White, 0f, default, Vector2.One);
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
        var inner = _innerDimensions;
        _innerDimensions = _gridSpace.ToDims();
        g.ScissorRectangle = GetClippingRectangle(spriteBatch);
        _innerDimensions = inner;
        g.RasterizerState = OverflowHiddenRasterizerState;
        spriteBatch.Begin(ss with { RasterizerState = OverflowHiddenRasterizerState });

        ref var dye = ref WorldNPC.GetPlayerDye(Dummy, Type);
        ref var equip = ref WorldNPC.GetPlayerEquip(Dummy, Type);
        var oldDye = dye;
        var old = equip;
        var finalWidth = _fit * _width;
        var xAdjust = (_preDims.Width - finalWidth) * 0.5f;
        var search = DyeMenu ? ItemID.Search : EquipLoader.GetSearch(Type);
        var nodeName = Type.ToString() + (DyeMenu ? "Dye" : string.Empty);
        if (HideDyes)
            dye = 0;
        var slots = (IEnumerable<int>)search._idToName.Keys;
        if (!search._idToName.ContainsKey(0))
            slots = Enumerable.Repeat(0, 1).Concat(slots);
        foreach (var slot in DyeMenu ? Dyes : slots)
        {
            var pos = gridTl + new Vector2(x * _width + xAdjust, (y + _smoothScroll) * HEIGHT);
            if (pos.Y + 2f < _gridSpace.Bottom && pos.Y + HEIGHT - 2f > _gridSpace.Top)
            {
                if (DyeMenu)
                    dye = GameShaders.Armor.GetShaderIdFromItemId(slot);
                else
                    equip = slot;
                var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)_width, (int)HEIGHT);
                if (slot == old)
                {
                    DrawUtils.Draw9Slice(spriteBatch, TextButton.HighlightTex.Value, rect, Color.White);
                    spriteBatch.FlushBatch();
                }
                if (rect.Contains(Main.mouseX, Main.mouseY))
                {
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        if (slot == 0)
                            CadastralUIState.AppearanceNode.Remove(nodeName);
                        else
                            CadastralUIState.AppearanceNode[nodeName] = search.GetName(slot);
                    }
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
        dye = oldDye;
        equip = old;

        /*
        foreach (var item in EquipLoader.slotToId[Type].Values)
        {

        }
        */

        spriteBatch.End();
        g.ScissorRectangle = oldScissor;
        g.RasterizerState = oldRasterizer;
        spriteBatch.Begin(in ss);
    }
}
