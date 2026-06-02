using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader.UI;
using System;

namespace AdventureTools.UI;

public sealed class HairDyeDisplay : UIElement
{
    public Item Dye;
    public Player Player;
    public MarqueeText<LocalizedText> Text;
    public HairDyeDisplay(Item dye, Player player)
    {
        Dye = dye;
        Player = player;
        Text = new(Lang.GetItemName(dye.type)) { TextAlignX = 0.5f, HAlign = 0.5f, Height = StyleDimension.FromPercent(0.5f), Width = StyleDimension.Fill, VAlign = 1f };
        Append(Text);
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        CadastralUIState.AppearanceNode?["HairDye"] = ItemID.Search.GetName(Dye.type);
        base.LeftMouseDown(evt);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dims);
        spriteBatch.FlushBatch(); // spritebatch is deferred here...,. but for some reason this is drawn on top of the head if i don't flush
        var prev = Player.hairDye;
        Player.hairDye = Dye.hairDye;
        Main.PlayerRenderer.DrawPlayerHead(Main.Camera, Player, new Vector2(-8f + dims.X + dims.Width * 0.5f,-2f + dims.Y + dims.Height * 0.25f));
        Player.hairDye = prev;
    }
}
