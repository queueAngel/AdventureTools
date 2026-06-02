using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class BoolVal<T>(T baseObj, Func<T, bool?> getter, Action<T, bool> setter) : UIElement
{
    private static readonly Asset<Texture2D> _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

    public Func<T, bool?> GetValue = getter;
    public Action<T, bool> SetValue = setter;
    public T BaseObject = baseObj;
    private float _anim;
    public LocalizedText Label;
    public override void OnInitialize()
    {
        Append(CadastralUIState.SimpleLabel(Label));
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
    public override void Update(GameTime gameTime)
    {
        if (IsMouseHovering && Label != null)
        {
            var hoverHint = Label.Key + "_Tip";
            if (Language.Exists(hoverHint))
                UICommon.TooltipMouseText(Language.GetTextValue(hoverHint));
        }
        base.Update(gameTime);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dimensions);
        var value = GetValue(BaseObject);
        if (!value.HasValue)
            return;
        var tex = _toggleTexture.Value;
        var Value = value.Value;
        var mid = dimensions.Y + (dimensions.Height * 0.5f);
        var togText = Value ? Lang.menu[126].Value : Lang.menu[124].Value;
        var font = FontAssets.ItemStack.Value;
        var scale = new Vector2(0.8f);
        var measure = ChatManager.GetStringSize(font, togText, scale);
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, togText, new Vector2(dimensions.X + dimensions.Width - measure.X - 50, mid - measure.Y * 0.25f), Color.White, 0f, default, scale);
        var sourceRectangle = new Rectangle(0, 0, tex.Width / 2, tex.Height);
        var sourceFull = sourceRectangle;
        sourceFull.X += sourceFull.Width + 1;
        sourceFull.Width -= 1;
        var drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, mid);
        spriteBatch.Draw(tex, drawPosition, new Rectangle(sourceFull.X, 0, 8, sourceFull.Height), Color.Black, 0f, new Vector2(16f, sourceFull.Height * 0.5f), 1, 0, 0);
        spriteBatch.Draw(tex, drawPosition, new Rectangle(sourceFull.X + 6, 0, 2, sourceFull.Height), Color.Black, 0f, new Vector2(1f, sourceFull.Height * 0.5f), new Vector2(8f, 1f), 0, 0);
        spriteBatch.Draw(tex, drawPosition, new Rectangle(sourceFull.X + 5, 0, 9, sourceFull.Height), Color.Black, 0f, new Vector2(-6f, sourceFull.Height * 0.5f), 1, 0, 0);

        _anim = float.Lerp(_anim, Value ? 1f : 0f, 0.25f);
        var useRect = Value ? sourceFull : sourceRectangle;
        spriteBatch.Draw(tex, drawPosition + new Vector2((_anim * 2f - 1f) * 8f, 0f), useRect, Color.White, 0f, new Vector2(useRect.Width * 0.5f, useRect.Height * 0.5f), Vector2.One, SpriteEffects.None, 0f);
        // spriteBatch.Draw(TextureAssets.MagicPixel.Value, dimensions.ToRectangle(), Color.Red * 0.5f);
    }
}
