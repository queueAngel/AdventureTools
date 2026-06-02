using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class TextButton : UIElement
{
    private static readonly Asset<Texture2D> _prButtonTex = ModContent.Request<Texture2D>(nameof(AdventureTools) + "/PRButton");
    public Asset<Texture2D> Image;
    public Rectangle? ImageFrame;
    public Vector2 ImageOffset;
    public bool ImageAboveText;
    public object Text;
    public Action Action;
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Target != this)
            return;
        Action?.Invoke();
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dims = GetDimensions();
        var color = IsMouseHovering ? Color.White : new Color(180, 180, 180);
        DrawUtils.Draw9Slice(spriteBatch, _prButtonTex.Value, dims.ToRectangle(), col: color);
        var f = FontAssets.ItemStack.Value;
        var s = Vector2.One;
        var t = Text?.ToString();
        var c = dims.Center();
        if (!ImageAboveText && Image != null)
            DrawImage();
        if (t != null)
        {
            var m = ChatManager.GetStringSize(f, t, s);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, t, c - (m * 0.5f), color, 0f, Vector2.Zero, s);
        }
        if (ImageAboveText && Image != null)
            DrawImage();
        void DrawImage() =>
            spriteBatch.Draw(Image.Value, c + ImageOffset, ImageFrame, color, 0f, ImageFrame.HasValue ? new Vector2(ImageFrame.Value.Width * 0.5f, ImageFrame.Value.Height * 0.5f) : Image.Size() * 0.5f, 1f, 0, 0);
    }
}
