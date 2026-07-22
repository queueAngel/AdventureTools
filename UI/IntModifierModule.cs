using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class IntModifierModule : UIElement
{
    public delegate bool CorrectRoot(ref JsonObject Root, ref string Property);
    private enum ManipulationType
    {
        Set,
        PreAdd,
        Multiply,
        Add
    }
    public InputField Set, PreAdd, Multiply, Add;
    public JsonObject Root;
    public string Property;
    public CorrectRoot CorrectSelf;
    public IntModifierModule(JsonObject root, string property, CorrectRoot correctSelf = null)
    {
        Root = root;
        Property = property;
        CorrectSelf = correctSelf;
        Register(ref Set, ManipulationType.Set);
        Register(ref PreAdd, ManipulationType.PreAdd);
        Register(ref Multiply, ManipulationType.Multiply);
        Register(ref Add, ManipulationType.Add);
    }
    private void Register(ref InputField field, ManipulationType type)
    {
        field = new(string.Empty) { Width = StyleDimension.Quarter, Height = StyleDimension.Fill, Left = StyleDimension.Quarter * (float)type };
        field.OnEnter += (s) =>
        {
            if (!float.TryParse(s.Text, out float value))
                return;
            if (CorrectSelf?.Invoke(ref Root, ref Property) == false)
                return;
            if (type is ManipulationType.Multiply && Root[Property] is null or JsonValue)
                Root[Property] = (float)value;
            else if (Root[Property] is not JsonObject obj)
            {
                Root[Property] = obj = Root[Property] is JsonValue jv ? new(Enumerable.Repeat(KeyValuePair.Create(ManipulationType.Multiply.ToString(), (JsonNode)jv), 1)) : [];
                obj[type.ToString()] = value;
            }
            else
                obj[type.ToString()] = value;
        };
        for (char c = '0'; c <= '9'; c++)
            field.WhitelistedChars.Add(c);
        if (type != ManipulationType.Set)
        {
            field.WhitelistedChars.Add(',');
            field.WhitelistedChars.Add('.');
        }
        Append(field);
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        Handle(Set, ManipulationType.Set);
        Handle(PreAdd, ManipulationType.PreAdd);
        Handle(Multiply, ManipulationType.Multiply);
        Handle(Add, ManipulationType.Add);
        base.Draw(spriteBatch);
    }
    private void Handle(InputField field, ManipulationType type)
    {
        if (field.currentlyWriting)
            return;
        if (Root is null)
            field.Text = string.Empty;
        else if (type == ManipulationType.Multiply && Root[Property] is JsonValue jv)
            field.Text = ((float)jv).ToString();
        else if (Root[Property] is not JsonObject obj || !obj.TryGetPropertyValue(type.ToString(), out var value) || value.GetValueKind() != JsonValueKind.Number)
            field.Text = string.Empty;
        else
            field.Text = ((float)value).ToString();
    }
}
