using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class SpawnRuleElement : UIElement
{
    private static readonly NPCPortraitInfoElement PortraitProvider = new();
    private UIElement _portrait;
    private UIBestiaryEntryIcon _icon;
    private UIElement _infoPanel;
    private BestiaryEntry _entry;
    public JsonArray Rules;
    public int Index;
    public SpawnRuleElement(JsonArray rules, int index)
    {
        Rules = rules;
        Index = index;
        var rule = Rules[Index].AsObject();
        var type = NPCID.Search.GetId((string)rule["Type"]);
        _entry = Main.BestiaryDB.FindEntryByNPCID(type);
        _portrait = PortraitProvider.ProvideUIElement(new BestiaryUICollectionInfo { OwnerEntry = _entry, UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1 });
        _portrait.Left.Pixels += 2f;
        _portrait.Width = new(196f, 0f);
        _portrait.VAlign = 0.5f;
        _icon = _portrait.Children.OfType<UIBestiaryNPCEntryPortrait>().First().Children.First().Children.OfType<UIBestiaryEntryIcon>().First();
        _infoPanel = new() { Width = new(-196f, 1f), Height = StyleDimension.Fill, HAlign = 1f};
        Append(_portrait);
        Append(_infoPanel);
        var ratio = 1f;
        var rate = rule["Rate"];
        if (rate != null && rate.GetValueKind() == JsonValueKind.Number)
            ratio = (float)rate;
        var slider = new Slider { Width = StyleDimension.Fill, Height = new(16f, 0f), Ratio = ratio };
        slider.OnChanged += Slider_OnChanged;
        _infoPanel.Append(slider);
        var scaling = new ScalingModule(rule, "Scaling") { Width = StyleDimension.Fill, Height = StyleDimension.Fill - 16f, VAlign = 1f };
        _infoPanel.Append(scaling);
    }
    private void Slider_OnChanged(Slider s)
    {
        var rule = Rules[Index].AsObject();
        rule["Rate"] = s.Ratio;
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        DrawUtils.Draw9Slice(spriteBatch, TextButton.Texture.Value, this.Dimensions);
        ref var info = ref _icon._collectionInfo;
        var old = info;
        info.UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1;
        info.OwnerEntry = _entry;
        base.Draw(spriteBatch);
        info = old;
    }
}
