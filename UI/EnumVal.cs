using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class EnumVal<TEnum>(TEnum start = default, float side = 16f) : UIElement where TEnum : struct
{
    public TEnum Current = start;
    public float Side = side;

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (evt.MousePosition.X < _dimensions.X + Side)
            Current = Current.PreviousEnum();
        else if (evt.MousePosition.X > _dimensions.X + _dimensions.Width - Side)
            Current = Current.NextEnum();
        base.LeftMouseDown(evt);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var tex = TextButton.Texture.Value;
        var rect = this.Dimensions;
        DrawUtils.Draw9Slice(spriteBatch, tex, rect, Color.White);
        var text = Current.ToString();
        var font = FontAssets.MouseText.Value;
        var size = ChatManager.GetStringSize(font, text, Vector2.One);
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, _dimensions.Center(), Color.White, 0f, size * 0.5f, Vector2.One);
        rect.Width = (int)Side;
        DrawUtils.Draw9Slice(spriteBatch, tex, rect, Color.White);
        rect.X += (int)(_dimensions.Width - Side);
        DrawUtils.Draw9Slice(spriteBatch, tex, rect, Color.White);
    }
}
