using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.ID;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class BuffButton : UIElement
{
    public int BuffType;
    public JsonArray BuffsNode;
    public string ContainsBuff;
    public override void Update(GameTime gameTime)
    {
        BuffsNode = SchemaVal.AnalyzingSchema["Buffs"] as JsonArray;
        var name = BuffID.Search.GetName(BuffType);
        ContainsBuff = BuffsNode?.Any(n => ((string)n).Equals(name)) is true ? name : null;
        base.Update(gameTime);
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (BuffsNode is null)
            SchemaVal.AnalyzingSchema["Buffs"] = new JsonArray(BuffID.Search.GetName(BuffType));
        else if (ContainsBuff is not null)
        {
            for (int j = BuffsNode.Count - 1; j >= 0; j--)
            {
                var n = (string)BuffsNode[j];
                if (n.Equals(ContainsBuff, StringComparison.Ordinal))
                    BuffsNode.RemoveAt(j);
            }
        }
        else
            BuffsNode.Add(BuffID.Search.GetName(BuffType));
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var p = Main.LocalPlayer;
        ref var buff = ref p.buffType[0];
        ref var time = ref p.buffTime[0];
        ref var alpha = ref Main.buffAlpha[0];
        var prevBuff = buff;
        var prevTime = time;
        var prevAlpha = alpha;
        buff = BuffType;
        time = 1;
        alpha = ContainsBuff != null || IsMouseHovering ? 1f : 0.6f;
        Main.DrawBuffIcon(-1, 0, (int)_dimensions.X, (int)_dimensions.Y);
        buff = prevBuff;
        time = prevTime;
        alpha = prevAlpha;
        if (ContainsBuff != null)
            DrawUtils.Draw9Slice(spriteBatch, TextButton.BorderTex.Value, this.Dimensions, Color.White * Main.essScale);
    }
}
