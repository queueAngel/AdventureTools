using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class BoolVal<T> : UIElement
{
    private static Asset<Texture2D> _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

    public Func<T, bool?> GetValue;
    public Action<T, bool> SetValue;
    public T BaseObject;
    public BoolVal(T baseObj, Func<T, bool?> getter, Action<T, bool> setter)
    {
        BaseObject = baseObj;
        GetValue = getter;
        SetValue = setter;
    }
    public override void MouseOver(UIMouseEvent evt)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        base.MouseOver(evt);
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        var value = GetValue(BaseObject);
        if (value.HasValue)
            SetValue(BaseObject, !value.Value);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dimensions);
        // "Yes" and "No" since no "True" and "False" translation available
        var value = GetValue(BaseObject);
        if (!value.HasValue)
            return;
        var Value = value.Value;
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Value ? Lang.menu[126].Value : Lang.menu[124].Value, new Vector2(dimensions.X + dimensions.Width - 60, dimensions.Y + 8f), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
        var sourceRectangle = new Rectangle(Value ? ((_toggleTexture.Width() - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width() - 2) / 2, _toggleTexture.Height());
        var drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
        spriteBatch.Draw(_toggleTexture.Value, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
        // spriteBatch.Draw(TextureAssets.MagicPixel.Value, dimensions.ToRectangle(), Color.Red * 0.5f);
    }
}
