using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text.Json.Nodes;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public sealed class ScalingModule : UIElement
{
    public JsonObject Root;
    public string Property;
    public ScalingModule(JsonObject root, string property)
    {
        Root = root;
        Property = property;
        SetPadding(16f);
        var rounding = AdventureTools.Instance.GetLocalization("RoundingLabel");
        var health = Language.GetText("BestiaryInfo.Life");
        var defense = Language.GetText("BestiaryInfo.Defense");
        var attack = Language.GetText("BestiaryInfo.Attack");
        var font = FontAssets.MouseText.Value;
        var s = Vector2.One;

        var rS = ChatManager.GetStringSize(font, rounding.Value, s).X;
        var hS = ChatManager.GetStringSize(font, health.Value, s).X;
        var dS = ChatManager.GetStringSize(font, defense.Value, s).X;
        var aS = ChatManager.GetStringSize(font, attack.Value, s).X;
        var max = float.Max(rS, float.Max(hS, float.Max(dS, aS))) + 16f;

        var rT = new UIText(rounding);
        var hT = new UIText(health);
        var dT = new UIText(defense);
        var aT = new UIText(attack);
        rT.TextOriginX = hT.TextOriginX = dT.TextOriginX = aT.TextOriginX = 0f;
        rT.TextOriginY = hT.TextOriginY = dT.TextOriginY = aT.TextOriginY = 0.5f;
        rT.Width = hT.Width = dT.Width = aT.Width = StyleDimension.FromPixels(max);

        var rootForDeez = Root[Property] as JsonObject;
        var rE = new EnumVal<RoundingType>(side: 38f);
        var hE = new IntModifierModule(rootForDeez, "Health", Correct);
        var dE = new IntModifierModule(rootForDeez, "Defense", Correct);
        var aE = new IntModifierModule(rootForDeez, "Attack", Correct);
        rE.HAlign = hE.HAlign = dE.HAlign = aE.HAlign = 1f;
        rE.Width = hE.Width = dE.Width = aE.Width = StyleDimension.Fill - max;
        rT.Height = hT.Height = dT.Height = aT.Height = rE.Height = hE.Height = dE.Height = aE.Height = StyleDimension.Quarter;
        hT.Top = hE.Top = StyleDimension.Quarter;
        dT.Top = dE.Top = StyleDimension.Quarter * 2f;
        aT.Top = aE.Top = StyleDimension.Quarter * 3f;

        Append(rT);
        Append(hT);
        Append(dT);
        Append(aT);

        Append(rE);
        Append(hE);
        Append(dE);
        Append(aE);
    }
    private bool Correct(ref JsonObject root, ref string property)
    {
        if (root is null)
        {
            root = Root?[Property] as JsonObject;
            if (root is null)
                return false;
        }
        return true;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        DrawUtils.Draw9Slice(spriteBatch, TextButton.Texture.Value, this.Dimensions, Color.White);
    }
}
