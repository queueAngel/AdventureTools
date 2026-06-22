using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Nodes;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class LocalizedTextElement : UIElement
{
    public JsonObject BaseObject;
    public InputField Default;
    public UIList List;
    public override void OnInitialize()
    {
        Default = new("...") { Width = StyleDimension.Fill, Height = StyleDimension.Fill};
        Default.OnEnter += field => BaseObject["en-US"] = field.Text;
        Append(Default);
        /*
        List =
        [
            .. BaseObject.Select(x =>
            {
                var element = new UIElement();
                element.OnDraw += static e => {
                    e.DrawConfigPanel(Main.spriteBatch, out _);
                };
                var keyField = new InputField("...") { Text = x.Key };
                keyField.OnEscape += field =>
                {
                    if (BaseObject.Remove(field.lastText, out var move))
                        BaseObject.Add(field.Text, move);
                };
                var valueField = new InputField("...") { Text = (string)x.Value };
                valueField.OnEscape += field => BaseObject[keyField.Text] = field.Text;
                valueField.Height = keyField.Height = StyleDimension.Fill;
                keyField.Width = valueField.Left = StyleDimension.FromPercent(0.2f);
                valueField.Width.Percent = 0.8f;
                element.Append(keyField);
                element.Append(valueField);
                return element;
            }),
        ];
        */
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dimensions);
    }
}
