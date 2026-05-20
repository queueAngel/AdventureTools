using Microsoft.Xna.Framework;
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
        if (!int.TryParse((string)propNode, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var result))
            return null;
        return new Color((result >> 16) & 0xFF, (result >> 8) & 0xFF, result & 0xFF);
    }
}
