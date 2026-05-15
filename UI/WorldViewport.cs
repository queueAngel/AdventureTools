using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.GameContent;

namespace AdventureTools.UI;

public sealed class WorldViewport : UIElement
{
    public UIElement Alternate;
    private float _realZoom = 1f;
    public float targetZoom = 1f;
    public float zoomSpeed = 0.1f;
    private Vector2 _realCam;
    public Vector2 targetCam; // world
    public float camSpeed = 0.1f;
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        _realCam = Vector2.Lerp(_realCam, targetCam, camSpeed);
        _realZoom = float.Lerp(_realZoom, targetZoom, zoomSpeed);
        var dims = GetDimensions();
        var size = new Vector2(dims.Width / _realZoom, dims.Height / _realZoom);
        var rect = Utils.CenteredRectangle(targetCam - Main.screenPosition, size);
        var outOfScreen = rect.X < 0 || rect.Y < 0;
        if (outOfScreen && Alternate != null)
        {
            Alternate.Draw(spriteBatch);
            return;
        }
        var target = outOfScreen ? TextureAssets.MagicPixel.Value : Main.screenTarget ?? Main.screenTargetSwap ?? TextureAssets.MagicPixel.Value;
        spriteBatch.Draw(target, dims.ToRectangle(), rect, Color.White);
    }
}
