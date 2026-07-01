using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Text.Json.Nodes;
using Terraria.Localization;

namespace AdventureTools.Utilities;

public enum RoundingType : byte
{
    None,
    Floor,
    Round,
    Ceiling,
}
public static class SchemaUtils
{
    public static bool TryGetLocalizedText(JsonNode json, out string text)
    {
        var current = LanguageManager.Instance.ActiveCulture.Name;
        var nn = json.AsObject();
        if (nn.TryGetPropertyValue(current, out var nameNode))
        {
            text = (string)nameNode;
            return true;
        }
        if (nn.TryGetPropertyValue("en-US", out nameNode))
        {
            text = (string)nameNode;
            return true;
        }
        text = null;
        return false;
    }
    public static Color? Hex(JsonObject a, string property)
    {
        if (a is null)
            return null;
        if (!a.TryGetPropertyValue(property, out var propNode))
            return null;
        return Hex((string)propNode);
    }
    public static Color? Hex(string hex)
    {
        if (!int.TryParse(hex.StartsWith('#') ? hex.AsSpan()[1..] : hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result))
            return null;
        return new Color((result >> 16) & 0xFF, (result >> 8) & 0xFF, result & 0xFF);
    }
    public static int Round(float value, RoundingType type) => type switch
    {
        RoundingType.None => (int)value,
        RoundingType.Floor => (int)MathF.Floor(value),
        RoundingType.Round => (int)MathF.Round(value),
        RoundingType.Ceiling => (int)MathF.Ceiling(value),
        _ => throw null,
    };
    public static int Manipulate(int value, JsonNode manipulator, RoundingType rounding = 0)
    {
        if (manipulator is null)
            return value;
        if (manipulator is JsonValue jv)
            return Round(value * (float)manipulator, rounding);
        var m = manipulator.AsObject();
        if (m.TryGetPropertyValue("Set", out var setProp))
            return (int)setProp;
        var val = (float)value;
        if (m.TryGetPropertyValue("PreAdd", out var preAddProp))
            val += (float)preAddProp;
        if (m.TryGetPropertyValue("Multiply", out var multiplyProp))
            val *= (float)multiplyProp;
        if (m.TryGetPropertyValue("Add", out var addProp))
            val += (float)addProp;
        return Round(val, rounding);
    }
}
