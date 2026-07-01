using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Effects;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class EffectButton<T> : UIElement where T : GameEffect
{
    public string EffectType;
    public string EffectName;
    public EffectManager<T> Manager;
    public JsonArray EffectsNode;
    public bool ContainsEffect;
    public EffectButton(string effectType, string effectName, EffectManager<T> manager)
    {
        EffectType = effectType;
        EffectName = effectName;
        Manager = manager;
        Append(new UIText(effectName)
        {
            TextOriginX = 0.5f,
            TextOriginY = 0.5f,
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
        });
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (EffectsNode is null)
            SchemaVal.AnalyzingSchema[EffectType] = new JsonArray(EffectName);
        else if (ContainsEffect)
        {
            for (int j = EffectsNode.Count - 1; j >= 0; j--)
            {
                var n = (string)EffectsNode[j];
                if (n.Equals(EffectName, StringComparison.Ordinal))
                {
                    EffectsNode.RemoveAt(j);
                    var fk = Manager._effects[EffectName];
                    Manager.OnDeactivate(fk);
                    fk.Deactivate();
                }
            }
        }
        else
            EffectsNode.Add(EffectName);
    }
    public override void Update(GameTime gameTime)
    {
        EffectsNode = SchemaVal.AnalyzingSchema[EffectType] as JsonArray;
        ContainsEffect = EffectsNode?.Any(n => ((string)n).Equals(EffectName)) is true;
        base.Update(gameTime);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var tex = ContainsEffect ? TextButton.HighlightTex.Value : TextButton.Texture.Value;
        DrawUtils.Draw9Slice(spriteBatch, tex, this.Dimensions, Color.White);
        if (IsMouseHovering)
            DrawUtils.Draw9Slice(spriteBatch, TextButton.BorderTex.Value, this.Dimensions, Color.White);
    }
}
