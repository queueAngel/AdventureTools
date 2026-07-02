using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        _portrait.VAlign = 0.5f;
        _icon = _portrait.Children.First(e => e is UIBestiaryNPCEntryPortrait).Children.First().Children.OfType<UIBestiaryEntryIcon>().First();
        Append(_portrait);
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
