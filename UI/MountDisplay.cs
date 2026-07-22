using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class MountDisplay(int mount, Player player) : UIElement
{
    public int Mt = mount;
    public Player Player = player;
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (Mt == -2)
            CadastralUIState.AppearanceNode?.Remove("Mount");
        else
            CadastralUIState.AppearanceNode?["Mount"] = MountID.Search.GetName(Mt);
        base.LeftMouseDown(evt);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dims);
        spriteBatch.FlushBatch(); // spritebatch is deferred here...,. but for some reason this is drawn on top of the head if i don't flush
        var m = Player.mount;
        var old = m._active ? m._type : -1;
        if (Mt == -2)
            m.SetMount(((+(int)Main.GameUpdateCount) / 16) % (MountLoader.MountCount - 1), Player);
        if (Mt == -1)
            m.Dismount(Player);
        else
            m.SetMount(Mt, Player);

        Main.PlayerRenderer.DrawPlayer(Main.Camera, Player, new Vector2(-8f + dims.X + dims.Width * 0.5f, -6f + dims.Y + dims.Height * 0.25f) + Main.screenPosition, 0f, Vector2.Zero, scale: 1f);

        if (old == -1)
            m.Dismount(Player);
        else
            m.SetMount(old, Player);

        var font = FontAssets.MouseText.Value;
        var text = Mt == -2 ? "Keep" : MountID.Search.GetName(Mt);
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, new Vector2(dims.X + dims.Width * 0.5f, dims.Y + dims.Height * 0.9f) - ChatManager.GetStringSize(font, text, Vector2.One) * 0.5f, Color.White, 0f, Vector2.Zero, Vector2.One);
    }
}

