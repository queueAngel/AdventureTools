using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class VariantDisplay(int male, Player player) : UIElement
{
    public int MaleVersion = male;
    public Player Player = player;
    public LocalizedText Text = male == PlayerVariantID.MaleDisplayDoll ? Lang.GetItemName(ItemID.Mannequin) : AdventureTools.Instance.GetLocalization(string.Concat("PlayerVariantNames.", PlayerVariantID.Search.GetName(male).AsSpan(4)));

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        var maleName = PlayerVariantID.Search.GetName(MaleVersion);
        CadastralUIState.AppearanceNode?["Style"] = maleName[4..];
        base.LeftMouseDown(evt);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dims);
        spriteBatch.FlushBatch(); // spritebatch is deferred here...,. but for some reason this is drawn on top of the head if i don't flush
        var prev = Player.skinVariant;
        Player.skinVariant = Player.Male ? MaleVersion : PlayerVariantID.Sets.AltGenderReference[MaleVersion];
        Main.PlayerRenderer.DrawPlayer(Main.Camera, Player, new Vector2(-8f + dims.X + dims.Width * 0.5f, -6f + dims.Y + dims.Height * 0.25f) + Main.screenPosition, 0f, Vector2.Zero, scale: 1f);
        Player.skinVariant = prev;
        var font = FontAssets.MouseText.Value;
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, Text.Value, new Vector2(dims.X + dims.Width * 0.5f, dims.Y + dims.Height * 0.9f) - ChatManager.GetStringSize(font, Text.Value, Vector2.One) * 0.5f, Color.White, 0f, Vector2.Zero, Vector2.One);
    }
}
