using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class LocalizedTextElement : UIElement
{
    public static JsonObject Editing;
    public JsonObject BaseObject;
    public InputField Default;
    public UIList List;
    public override void OnInitialize()
    {
        Default = new("...") { Width = StyleDimension.Fill - 32f, Height = StyleDimension.Fill };
        Default.OnEnter += field => BaseObject["en-US"] = field.Text;
        Append(Default);
        Append(new TextButton
        {
            Height = StyleDimension.Fill,
            Width = new(32f, 0f),
            HAlign = 1f,
            Action = () =>
            {
                Editing = BaseObject;
                CadastralUIState.Instance.OpenSecondPanel(SubPanelScr.LocalizedText);
            }
        });
    }
    public static UIElement Make(string key, JsonObject json, StyleDimension h = default)
    {
        var element = new UIElement
        {
            Width = StyleDimension.Fill,
            Height = h
        };
        element.OnDraw += static e => e.DrawConfigPanel(Main.spriteBatch, out _);
        var keyField = new InputField("...") { Text = key };
        keyField.OnEnter += field =>
        {
            if (json.Remove(field.lastText, out var move))
                json.Add(field.Text, move);
        };
        var valueField = new InputField("...") { Text = (string)json[key] ?? "" };
        valueField.OnEnter += field => json[keyField.Text] = field.Text;
        valueField.Height = keyField.Height = StyleDimension.Fill;
        keyField.Width = valueField.Left = StyleDimension.FromPercent(0.2f);
        valueField.Width.Percent = 0.8f;
        valueField.Width.Pixels -= 22f;
        element.Append(keyField);
        element.Append(valueField);
        var delete = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) { HAlign = 1f };
        delete.OnLeftClick += (_, _) =>
        {
            if (element.Parent?.Parent is UIList l)
            {
                l.Remove(element);
                json.Remove(keyField.Text);
            }
        };
        element.Append(delete);
        return element;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dimensions);
    }
}
