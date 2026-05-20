using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.GameContent;
using System;
using Terraria.ModLoader.UI;
using Daybreak.Common.Rendering;

namespace AdventureTools.UI;

public sealed class WorldViewport : UIElement
{
    public UIElement Alternate;
    private float _realZoom = 1f;
    public float targetZoom = 1f;
    public float zoomSpeed = 0.1f;
    private Vector2 _realCam;
    public Func<Vector2> targetCam; // world
    public float camSpeed = 0.1f;
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var tgtCam = targetCam();
        _realCam = Vector2.Lerp(_realCam, tgtCam, camSpeed);
        _realZoom = float.Lerp(_realZoom, targetZoom, zoomSpeed);
        var dims = GetDimensions();
        var reducedDims = dims;
        reducedDims.X += 2;
        reducedDims.Y += 2;
        reducedDims.Width -= 4;
        reducedDims.Height -= 4;
        var z = Main.GameZoomTarget / _realZoom;
        var size = new Vector2(reducedDims.Width * z, reducedDims.Height * z);
        var rect = Utils.CenteredRectangle((tgtCam - Main.screenPosition).Transform(Main.GameViewMatrix.TransformationMatrix), size);
        var outOfScreen = rect.X < 0 || rect.Y < 0;
        if (outOfScreen && Alternate != null)
        {
            Alternate.Draw(spriteBatch);
            return;
        }
        var target = Main.screenTarget ?? Main.screenTargetSwap ?? TextureAssets.MagicPixel.Value;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, dims.ToRectangle(), UICommon.DefaultUIBlue.MultiplyRGB(Color.LightGray));
        spriteBatch.Draw(target, reducedDims.ToRectangle(), rect, Color.White);
    }
}
