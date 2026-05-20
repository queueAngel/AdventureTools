using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class TextButton : UIElement
{
    private static readonly Asset<Texture2D> _prButtonTex = ModContent.Request<Texture2D>(nameof(AdventureTools) + "/PRButton");
    public string Text;
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
        var m = ChatManager.GetStringSize(f, Text, s);
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Text, dims.Center() - (m * 0.5f), color, 0f, Vector2.Zero, s);
    }
}
