using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Text.Json.Nodes;
using Terraria.Localization;

namespace AdventureTools.Utilities;

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
}
