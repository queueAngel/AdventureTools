using AdventureTools.Utilities;
using Daybreak.Common.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class IDPicker : UIElement
{
    public object Label;
    public Rectangle? Frame;
    public SubPanelScr OpenTo;
    private static readonly Asset<Texture2D> _searchIcon = ModContent.Request<Texture2D>("Daybreak/Assets/Images/UI/SearchIcon");
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Target != this)
            return;
        CadastralUIState.Instance.OpenSecondPanel(OpenTo);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var d);
        var t = _searchIcon.Value;
        // if (CadastralUIState.Instance.OpenTo == OpenTo)
        var y = d.Y + d.Height * 0.5f;
        spriteBatch.Draw(new DrawParameters(t)
        { 
            Position = new Vector2(d.X + d.Width, y),
            Origin = new Vector2(t.Width + 8f, t.Height * 0.5f),
        });

        if (Label is Asset<Texture2D> tex)
        {
            spriteBatch.Draw(new DrawParameters(tex)
            {
                Position = new Vector2(d.X, y),
                Origin = Frame.HasValue ? Frame.Value.Size() * 0.5f : new Vector2(-8f, tex.Height() * 0.5f),
                Source = Frame,
            });
        }
    }
}
