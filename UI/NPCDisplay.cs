using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Bestiary;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class NPCDisplay(int type) : UIElement
{
    public int Type = type;
    public UnlockableNPCEntryIcon Icon = new(type);
    public override void LeftClick(UIMouseEvent evt)
    {
        SpawnRuleElement.Current?.SetType(Type);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dims = this.Dimensions;
        DrawUtils.Draw9Slice(spriteBatch, TextButton.Texture.Value, dims);
        var set = new EntryIconDrawSettings { iconbox = dims, IsHovered = false, IsPortrait = true };
        Icon.Update(default, dims, set);
        Icon.Draw(default, spriteBatch, set);
    }
}
