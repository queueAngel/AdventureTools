using AdventureTools.Utilities;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private TextButton _pick;
    public JsonArray Rules;
    public int Index;
    private static StyleDimension PORTRAIT_WIDTH = new(196f, 0f);
    public static SpawnRuleElement Current;
    public SpawnRuleElement(JsonArray rules, int index)
    {
        Rules = rules;
        Index = index;
        var rule = Rules[Index].AsObject();
        _pick = new() { Left = new(10f, 0f), Top = new(10f, 0f), Width = PORTRAIT_WIDTH, Action = () =>
        {
            Current = this;
            CadastralUIState.Instance.OpenSecondPanel(SubPanelScr.NPC);
        }};
        if (SetPortrait(NPCID.Search.GetId((string)rule["Type"])))
            Append(_portrait);
        _infoPanel = new() { Width = new(-234f, 1f), Height = StyleDimension.Fill, HAlign = 1f, Left = new(-20f, 0f)};
        Append(_infoPanel);
        Append(_pick);
        var close = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel")) { HAlign = 1f };
        close.OnLeftClick += (_, _) =>
        {
            if (Parent is { Parent: UIList list })
            {
                var realList = list._items;
                foreach (var element in CollectionsMarshal.AsSpan(realList).Slice(realList.IndexOf(this)))
                    if (element is SpawnRuleElement s)
                        s.Index--;
                list.Remove(this);
            }
            Rules.Remove(rule);
        };
        Append(close);
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
    private static string Label(int type) => AdventureTools.Instance.GetLocalization("NPCTypeLabel").Format(Lang.GetNPCName(type));
    public void SetType(int type)
    {
        _portrait?.Remove();
        if (SetPortrait(type))
            Append(_portrait);
        Rules[Index]["Type"] = NPCID.Search.GetName(type);
    }
    public bool SetPortrait(int type)
    {
        const float padding = 10f;
        _pick.Text = Label(type);
        if (type == NPCID.None)
        {
            _pick.Height = StyleDimension.Fill - padding;
            _pick.Height.Pixels -= padding;
            return false;
        }
        _entry = BestiaryDatabaseNPCsPopulator.FindEntryByNPCID(type);
        if (_entry.Icon is null)
        {
            _entry = null;
            _portrait = new NPCDisplay(type) { IgnoresMouseInteraction = true, Width = PORTRAIT_WIDTH, Height = new(112f, 0f), Left = new(padding, 0f), Top = new(-padding, 0f), VAlign = 1f };
            _pick.Height = StyleDimension.Fill - (_portrait.Height - _portrait.Top);
            _pick.Height.Pixels -= padding;
            return true;
        }
        _portrait = PortraitProvider.ProvideUIElement(new BestiaryUICollectionInfo { OwnerEntry = _entry, UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1 });
        _portrait.Left.Pixels += padding - 4f;
        _portrait.Top.Pixels -= padding;
        _portrait.Width = PORTRAIT_WIDTH;
        _portrait.VAlign = 1f;
        _icon = _portrait.Children.OfType<UIBestiaryNPCEntryPortrait>().First().Children.First().Children.OfType<UIBestiaryEntryIcon>().First();
        _pick.Height = StyleDimension.Fill - (_portrait.Height - _portrait.Top);
        _pick.Height.Pixels -= padding;
        return true;
    }
    private void Slider_OnChanged(Slider s)
    {
        var rule = Rules[Index].AsObject();
        rule["Rate"] = s.Ratio;
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        DrawUtils.Draw9Slice(spriteBatch, TextButton.Texture.Value, this.Dimensions);
        if (_entry is null)
        {
            base.Draw(spriteBatch);
            return;
        }
        ref var info = ref _icon._collectionInfo;
        var old = info;
        info.UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1;
        info.OwnerEntry = _entry;
        base.Draw(spriteBatch);
        info = old;
    }
}
