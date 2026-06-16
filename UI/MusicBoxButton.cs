using AdventureTools.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class MusicBoxButton(int track) : UIElement
{
    public int track = track;
    private int _lastTrack = -1;
    private int _item;
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        var s = SchemaVal.AnalyzingSchema;
        if (track == -1)
            s.Remove("Music");
        else
            s["Music"] = track == 0 ? "None" : MusicID.Search.GetName(track);
        base.LeftMouseDown(evt);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dims);
        if (track != _lastTrack)
        {
            _item = track <= 0 ? ItemID.None : MusicToBoxMapper.GetBox(track);
            _lastTrack = track;
        }
        var c = dims.Center();
        if (_item > 0)
            ItemSlot.DrawItemIcon(ContentSamples.ItemsByType[_item], ItemSlot.Context.MouseItem, spriteBatch, c, 1f, 64f, Color.White);
        var name = track == -1 ? "Keep Regular Music" : track == 0 ? "Silence" : MusicID.Search.TryGetName(track, out var trackName) ? trackName : "Unknown";
        var font = FontAssets.MouseText.Value;
        var scale = Vector2.One;
        var size = ChatManager.GetStringSize(font, name, scale);
        c.Y += 32f;
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, name, c - (size * 0.5f), Color.White, 0f, default, scale);
    }
}
