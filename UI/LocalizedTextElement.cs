using Daybreak.Common.UI;
using Json.Patch;
using Json.Pointer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class LocalizedTextElement : UIElement
{
    public JsonNode BaseObject;
    public RelativeJsonPointer Pointer;
    public InputField Default;
    public UIList List;
    public override void OnInitialize()
    {
        Default = new("...");
        Default.OnEscape += field =>
        {
            if (Pointer.TryEvaluate(BaseObject, out var node))
                node["en-US"] = field.Text;
        };
        List = [];
        if (Pointer.TryEvaluate(BaseObject, out var node))
        {
            List.AddRange(node.AsObject().Select(x =>
            {
                var element = new UIElement();
                var keyField = new InputField("...") { Text = x.Key };
                keyField.OnEscape += field =>
                {
                    if (Pointer.TryEvaluate(BaseObject, out var node))
                    {
                        var obj = node.AsObject();
                        if (obj.Remove(field.lastText, out var move))
                            obj.Add(field.Text, move);
                    }
                };
                var valueField = new InputField("...") { Text = (string)x.Value };
                valueField.OnEscape += field =>
                {
                    if (Pointer.TryEvaluate(BaseObject, out var node))
                        node[keyField.Text] = field.Text;
                };
                valueField.Height = keyField.Height = StyleDimension.Fill;
                valueField.Width.Percent = keyField.Width.Percent = 0.4f;
                valueField.HAlign = 1f;
                element.Append(keyField);
                element.Append(valueField);
                return element;
            }));
        }
    }
}
