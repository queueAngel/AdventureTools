using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class MountDisplay(int mount, Player player) : UIElement
{
    public int Mt = mount;
    public Player Player = player;
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        CadastralUIState.AppearanceNode?["Mount"] = MountID.Search.GetName(Mt);
        base.LeftMouseDown(evt);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dims);
        spriteBatch.FlushBatch(); // spritebatch is deferred here...,. but for some reason this is drawn on top of the head if i don't flush
        var m = Player.mount;
        var old = m._type;
        var oldNet = Main.netMode;
        Main.netMode = NetmodeID.SinglePlayer;
        Main.gameMenu = true;
        m.SetMount(Mt, Player);
        Main.PlayerRenderer.DrawPlayer(Main.Camera, Player, new Vector2(-8f + dims.X + dims.Width * 0.5f, -6f + dims.Y + dims.Height * 0.25f) + Main.screenPosition, 0f, Vector2.Zero, scale: 1f);
        m.SetMount(old, Player);
        Main.netMode = oldNet;
        Main.gameMenu = false;
        var font = FontAssets.MouseText.Value;
        var text = MountID.Search.GetName(Mt);
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, new Vector2(dims.X + dims.Width * 0.5f, dims.Y + dims.Height * 0.9f) - ChatManager.GetStringSize(font, text, Vector2.One) * 0.5f, Color.White, 0f, Vector2.Zero, Vector2.One);
    }
}

