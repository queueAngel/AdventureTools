using AdventureTools.Items;
using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.Features.Configuration;
using Daybreak.Common.Rendering;
using Daybreak.Common.UI;
using Json.Pointer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using ReLogic.OS;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AdventureTools.UI;

public enum SelectionState
{
    None,
    Selecting,
    Selected
}
public enum PanelScreen
{
    None,
    Multiple,
    Biome,
    BiomeSchema,
    NPC,
    NPCTile,
    NPCSchema,
}
public enum SubPanelScr
{
    None,
    Hair,
    HairDye,
    Style,
    Accessories,
    Dialogue,
    Chat
}
public sealed class CadastralUIState : UIState
{
    internal static readonly List<UPoint16> _processingPolygon = [];
    internal static bool _processingPolygonFinished;
    internal static readonly HashSet<object> _selection = [];
    public static CadastralUIState Instance { get; } = new();
    public DynamicPanel Panel;
    public DynamicPanel SecondaryPanel;
    public PanelScreen Screen;
    public UIList List = new()
    {
        Width = StyleDimension.Fill,
        Height = StyleDimension.Fill,
    };
    public UIText Title = new(string.Empty, 0.7f, true) { HAlign = 0.5f };
    public UIHorizontalSeparator Separator = new()
    {
        Width = StyleDimension.Fill,
        Color = new Color(89, 116, 213, 255) * 0.9f
    };
    public CadastralButton[] Buttons = new CadastralButton[(int)CadastralAction.Max];
    static CadastralUIState()
    {
        // allows zooming even while in this UI
        IL_Main.UpdateViewZoomKeys += static il =>
        {
            var c = new ILCursor(il);
            var enterBlock = il.DefineLabel();
            c.EmitDelegate(() => Main.LocalPlayer.GetModPlayer<CadastralPlayer>().operatingCrawler);
            c.EmitBrtrue(enterBlock);
            c.GotoNext(i => i.MatchLdcR4(out _)).MarkLabel(enterBlock);
        };
    }
    public override void OnActivate()
    {
        foreach (var button in Buttons)
        {
            if (button.Type == CadastralAction.Select)
            {
                button.Down = true;
                continue;
            }
            button.Down = false;
        }
    }
    public static PanelScreen DetermineScreen()
    {
        if (_selection.Count == 0)
            return PanelScreen.None;
        if (_selection.Count != 1)
            return PanelScreen.Multiple;
        return _selection.ElementAt(0) switch
        {
            CustomBiome => PanelScreen.Biome,
            NPC => PanelScreen.NPC,
            TileEntity => PanelScreen.NPCTile,
            _ => PanelScreen.None,
        };
    }
    public void AppendTitle()
    {
        var text = Screen is PanelScreen.None ? Lang.inter[23] : AdventureTools.Instance.GetLocalization($"Panel.{Screen}Title");
        Title.SetText(text);
        List.Add(Title);
        List.Add(Separator);
    }
    public void PopulatePanel()
    {
        var mod = AdventureTools.Instance;
        switch (Screen)
        {
            case PanelScreen.None:
                List.Add(new UIText(mod.GetLocalization("Panel.NoneSelected")) { TextOriginX = 0f, IsWrapped = true, Width = StyleDimension.Fill, Height = StyleDimension.Fill }.WithPadding(4f));
                break;
            case PanelScreen.Multiple:
                List.Add(new UIText(mod.GetLocalization("Panel.MultipleSelected")));
                foreach (var selected in _selection)
                {
                    
                }
                break;
            case PanelScreen.Biome:
                var selectedBiome = (CustomBiome)_selection.ElementAt(0);
                AddLocalizedNode(selectedBiome.Schema["Name"]?.AsObject());
                break;
            case PanelScreen.BiomeSchema:
                var schema = SchemaVal.AnalyzingSchema;
                break;
            case PanelScreen.NPC:
                var selectedNPC = (NPC)_selection.ElementAt(0);
                var wNPC = (WorldNPC)selectedNPC.ModNPC;
                var panel = new UIElement() { Height = StyleDimension.FromPixels(128f), Width = StyleDimension.FromPercent(1f) }.WithPadding(16f);
                var viewport = new WorldViewport() { targetCam = () => selectedNPC.Center, MinWidth = StyleDimension.FromPixels(96f), Height = StyleDimension.Fill };
                var boolVal = new BoolVal<WorldNPC>(wNPC, w => w.Static, (w, b) => w.Static = b) { Label = wNPC.Mod.GetLocalization("SchemaUI.NPC.Static"), Left = StyleDimension.FromPixels(108f), Height = StyleDimension.FromPixelsAndPercent(-8f, 0.5f), Width = StyleDimension.FromPixelsAndPercent(-112f, 1f) };
                var schVal = new SchemaVal() { BaseObject = wNPC, Left = boolVal.Left, Height = boolVal.Height, Width = boolVal.Width, Top = StyleDimension.Half};
                panel.Append(viewport);
                panel.Append(boolVal);
                panel.Append(schVal);
                boolVal.Activate();
                schVal.Activate();
                List.Add(panel);
                break;
            case PanelScreen.NPCSchema:
                schema = SchemaVal.AnalyzingSchema;
                if (schema?.TryGetPropertyValue("Appearance", out var appNode) == true)
                    AppearanceNode = appNode.AsObject();

                var headerPanel = new UIElement() { Height = StyleDimension.FromPixels(64f), Width = StyleDimension.Fill };

                // character display
                var cha = new UICharacter(WorldNPC.Dummy, true) { HAlign = 1f, MinWidth = StyleDimension.FromPixels(64f), MinHeight = StyleDimension.FromPixels(54f) };
                cha.OnDraw += static _ =>
                {
                    savedEngine = Lighting._activeEngine;
                    Lighting._activeEngine = FullbrightEngine.Instance;
                    var d = WorldNPC.Dummy;
                    d.direction = 1;
                    WorldNPC.PrintOnDummy(SchemaVal.AnalyzingSchema);
                };
                headerPanel.Append(cha);

                // body type
                const string clothesStyle = "Images/UI/CharCreation/ClothStyle";
                var fem = new UIColoredImageButton(Main.Assets.Request<Texture2D>(clothesStyle + "Female"), true);
                var masc = new UIColoredImageButton(Main.Assets.Request<Texture2D>(clothesStyle + "Male"), true);
                if (AppearanceNode?["BodyType"]?.ToString() is "Female")
                    fem.SetSelected(true);
                else
                    masc.SetSelected(true);
                fem.OnLeftMouseDown += (_, _) =>
                {
                    masc.SetSelected(false);
                    fem.SetSelected(true);
                    AppearanceNode["BodyType"] = "Female";
                };
                masc.OnLeftMouseDown += (_, _) =>
                {
                    fem.SetSelected(false);
                    masc.SetSelected(true);
                    AppearanceNode["BodyType"] = "Male";
                };
                fem.HAlign = masc.HAlign = 1f;
                fem.Left.Pixels = masc.Left.Pixels = -cha.MinWidth.Pixels;
                masc.Top.Pixels += fem.Height.Pixels;
                headerPanel.Append(fem);
                headerPanel.Append(masc);

                // name display
                var labelLabel = Language.GetText("UI.PlayerNameSlot");
                var labelLabelW = ChatManager.GetStringSize(FontAssets.MouseText.Value, labelLabel.Value, Vector2.One).X + 16f;
                var subPanel = new UIElement() { Left = StyleDimension.FromPixels(labelLabelW), Width = StyleDimension.FromPixelsAndPercent(-(102f + labelLabelW), 1f), Height = StyleDimension.Fill };
                var name = new LocalizedTextElement() { BaseObject = schema?["Name"]?.AsObject(), Width = StyleDimension.Fill, Height = StyleDimension.FromPercent(0.5f) };
                subPanel.Append(name);
                name.Activate();
                name.Default.Text = schema?["Name"]?["en-US"]?.ToString() ?? string.Empty;
                var nLabel = new UIText(Language.GetText("UI.PlayerNameSlot")) { Height = name.Height, Width = name.Left, TextOriginY = 0.5f };
                headerPanel.Append(subPanel);
                headerPanel.Append(nLabel);
                // buttons
                var m = AdventureTools.Instance;
                var dialogueLabel =$"{ItemTagHandler.GenerateTag(ContentSamples.ItemsByType[ItemID.AnnouncementBox])} {m.GetLocalization("SchemaUI.NPC.EditDialogue")}";
                var shopLabel = $"{ItemTagHandler.GenerateTag(ContentSamples.ItemsByType[ItemID.DiscountCard])} {Lang.inter[28]}";
                var dialogueButton = new TextButton() { Text = dialogueLabel, Width = StyleDimension.FromPercent(0.5f), Height = name.Height, Top = name.Height };
                var shopButton = new TextButton() { Text = shopLabel, Width = dialogueButton.Width, Height = dialogueButton.Height, Top = dialogueButton.Height, HAlign = 1f };
                subPanel.Append(dialogueButton);
                subPanel.Append(shopButton);

                // middle panel
                var pickerPanel = new UIElement() { Height = StyleDimension.FromPixels(152f), Width = StyleDimension.Fill };

                // PROBABLY IL EDIT COLOR PICKER FOR CUSTOM GRAYED OUT LOOK WHEN INPUT IS DISABLED
                var picker = new ColorPicker() { Width = StyleDimension.FromPixels(128f), Height = StyleDimension.Fill, IgnoresMouseInteraction = true };
                picker.OnChanged += static self =>
                {
                    if (AppearanceNode != null && CurrentColorPick != null)
                        AppearanceNode[CurrentColorPick] = self.Color.Hex3();
                };

                var hair = PickerButton(picker, "Hair");
                var eye = PickerButton(picker, "Eye");
                eye.SetMiddleTexture(Main.Assets.Request<Texture2D>("Images/UI/CharCreation/ColorEyeBack"));
                var skin = PickerButton(picker, "Skin");
                var shirt = PickerButton(picker, "Shirt");
                var undershirt = PickerButton(picker, "Undershirt");
                var pants = PickerButton(picker, "Pants");
                var shoes = PickerButton(picker, "Shoes", "Shoe");

                var px = hair.Width.Pixels;
                hair.Top = eye.Top = skin.Top = StyleDimension.FromPixels(px);
                undershirt.Left.Pixels += px;
                pants.Left.Pixels += px * 2f;
                shoes.Left.Pixels += px * 3f;
                hair.Left = eye.Left = skin.Left = StyleDimension.FromPixels(px * 0.5f);
                eye.Left.Pixels += px;
                skin.Left.Pixels += px * 2f;
                picker.Left.Pixels = px * 0.5f;

                var bottomPanel = new UIElement() { Height = StyleDimension.FromPixels(px * 2f), Width = StyleDimension.Fill };

                bottomPanel.Append(hair);
                bottomPanel.Append(eye);
                bottomPanel.Append(skin);
                bottomPanel.Append(shirt);
                bottomPanel.Append(undershirt);
                bottomPanel.Append(pants);
                bottomPanel.Append(shoes);

                // acc and json buttons
                var acc = new TextButton() { Text = Lang.inter[79], Width = StyleDimension.Fill, Height = StyleDimension.Half, Action = () => OpenSecondPanel(SubPanelScr.Accessories) };
                var jsonCopy = new TextButton() { Text = mod.GetLocalization("SchemaUI.CopyJson"), Top = StyleDimension.Half, Width = StyleDimension.Half, Height = StyleDimension.Half, Action = static () => Platform.Get<IClipboard>().Value = SchemaVal.AnalyzingSchema.ToString() };
                var jsonPaste = new TextButton() { Text = mod.GetLocalization("SchemaUI.PasteJson"), Top = StyleDimension.Half, HAlign = 1f, Width = StyleDimension.Half, Height = StyleDimension.Half, Action = static () =>
                {
                    /*
                    var parsed = default(JsonNode);
                    try
                    {
                        parsed = JsonNode.Parse(Platform.Get<IClipboard>().Value);
                    }
                    catch
                    {
                        Main.NewText("parse errror");
                        return;
                    }
                    // DOESN'T REMOVE REFERENCES TO SCHEMAS IN DIFFERENT PLACES
                    // PERHAPS WRITE SOME WAY TO DEEP COPY?
                    SchemaVal.AnalyzingSchema.ReplaceWith(parsed);
                    */
                }};
                var toBr = px * 4.25f;
                var brButtons = new UIElement() { Height = StyleDimension.Fill, Width = new(-toBr - 7f, 1f), Left = StyleDimension.FromPixels(toBr) };
                brButtons.Append(acc);
                brButtons.Append(jsonCopy);
                brButtons.Append(jsonPaste);
                bottomPanel.Append(brButtons);

                // pickers
                var pickersLeft = new StyleDimension(picker.Width.Pixels + picker.Left.Pixels + 8f, picker.Width.Percent + picker.Left.Percent);
                var pickersWidth = new StyleDimension(-pickersLeft.Pixels - 8f, 1f);
                var pickersHeight = StyleDimension.FromPercent(1f / 3f);
                var hairStylePicker = new IDPicker
                {
                    OpenTo = SubPanelScr.Hair,
                    Label = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/HairStyle_Hair"),
                    Left = pickersLeft,
                    Width = pickersWidth,
                    Height = pickersHeight
                };
                pickerPanel.Append(hairStylePicker);

                Main.instance.LoadItem(ItemID.HairDyeRemover);
                var hairDyePicker = new IDPicker
                {
                    OpenTo = SubPanelScr.HairDye,
                    Label = TextureAssets.Item[ItemID.HairDyeRemover],
                    Left = pickersLeft,
                    Width = pickersWidth,
                    Height = pickersHeight,
                    Top = pickersHeight,
                };
                pickerPanel.Append(hairDyePicker);

                var stylePicker = new IDPicker
                {
                    OpenTo = SubPanelScr.Style,
                    Label = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/ColorUndershirt"),
                    Left = pickersLeft,
                    Width = pickersWidth,
                    Height = pickersHeight,
                    Top = new(0f, pickersHeight.Percent * 2f)
                };
                pickerPanel.Append(stylePicker);

                pickerPanel.Append(picker);

                // VOICE PICKER SHOULD GO HERE
                // 1.4.5 SEPARATES VOICE INTO VARIANT (WHICH IS SAVED AND INHERENT TO THE CHARACTER) AND OVERRIDE (WHICH COMES FROM ITEMS AND OTHER EFFECTS)
                // ALSO VOICE PITCH

                List.Add(headerPanel);
                List.Add(pickerPanel);
                List.Add(bottomPanel);
                break;
        }
    }
    private static UIColoredImageButton _lastPicked;
    private static UIColoredImageButton PickerButton(ColorPicker picker, string thingName, string thingJsonName = null)
    {
        const string path = "Images/UI/CharCreation/Color";
        var elem = new UIColoredImageButton(Main.Assets.Request<Texture2D>(path + thingName));
        var pick = (thingJsonName ?? thingName) + "Color";
        elem.OnLeftMouseDown += (_, _) =>
        {
            picker.IgnoresMouseInteraction = false;
            CurrentColorPick = pick;
            picker.Color = elem._color;
            _lastPicked?.SetSelected(false);
            _lastPicked = elem;
            elem.SetSelected(true);
        };
        elem.OnDraw += self =>
        {
            ((UIColoredImageButton)self).SetColor(SchemaUtils.Hex(AppearanceNode, pick) ?? Color.White);
        };
        return elem;
    }
    internal static JsonObject AppearanceNode;
    internal static string CurrentColorPick;
    internal static ILightingEngine savedEngine;
    public sealed class FullbrightEngine : ILightingEngine
    {
        public static readonly FullbrightEngine Instance = new();
        public void AddLight(int x, int y, Vector3 color) { }
        public void Clear() { }
        public Vector3 GetColor(int x, int y) => Vector3.One;
        public void ProcessArea(Rectangle area) { }
        public void Rebuild() { }
    }
    public static UIText SimpleLabel(LocalizedText t) => new(t) { Left = StyleDimension.FromPixels(10f), TextOriginX = 0f, TextOriginY = 0.5f, VAlign = 0.5f, MinWidth = StyleDimension.FromPixels(64f), MinHeight = StyleDimension.FromPixels(16f) };
    public void AddLocalizedNode(JsonObject node)
    {
        List.Add(new LocalizedTextElement { BaseObject = node, Height = StyleDimension.FromPixels(32f) });
    }
    public SubPanelScr OpenTo;
    public void OpenSecondPanel(SubPanelScr screen)
    {
        OpenTo = screen;
        var panel = SecondaryPanel;
        panel.RemoveAllChildren();
        switch (screen)
        {
            case SubPanelScr.Hair:
                panel.Width.Pixels = 32f * 8f;
                panel.Height.Pixels = panel.Width.Pixels * 1.25f;
                panel.Recalculate();
                var grid = new DynamicGrid()
                {
                    Width = StyleDimension.Fill,
                    Height = StyleDimension.Fill,
                    XSpacing = 4f,
                    YSpacing = 4f,
                    ElementWidth = 44f,
                    ElementHeight = 44f,
                    OverflowHidden = true,
                };
                panel.Handles.Add(grid);
                panel.Append(grid);
                grid.Add(Enumerable.Range(0, HairLoader.Count - 1).Select(i =>
                {
                    var but = new UIHairStyleButton(WorldNPC.Dummy, i);
                    but.OnLeftMouseDown += static (_, e) => AppearanceNode?["Hair"] = ((UIHairStyleButton)e).HairStyleId;
                    return but;
                }));
                break;
            case SubPanelScr.HairDye:
                grid = new DynamicGrid()
                {
                    Width = StyleDimension.Fill,
                    Height = StyleDimension.Fill,
                    XSpacing = 4f,
                    YSpacing = 4f,
                    ElementWidth = 72f,
                    ElementHeight = 50f,
                    OverflowHidden = true,
                };
                panel.Handles.Add(grid);
                panel.Append(grid);
                panel.Width.Pixels = 32f * 8f;
                panel.Height.Pixels = panel.Width.Pixels * 1.25f;
                grid.Add(new HairDyeDisplay(ContentSamples.ItemsByType[ItemID.HairDyeRemover], WorldNPC.Dummy));
                grid.Add(Enumerable.Range(0, ItemLoader.ItemCount - 1).Where(i => ContentSamples.ItemsByType[i].hairDye != -1 && i != ItemID.HairDyeRemover).Select(i =>
                {
                    return new HairDyeDisplay(ContentSamples.ItemsByType[i], WorldNPC.Dummy);
                }));
                panel.Recalculate();
                break;
            case SubPanelScr.Style:
                grid = new DynamicGrid()
                {
                    Width = StyleDimension.Fill,
                    Height = StyleDimension.Fill,
                    XSpacing = 4f,
                    YSpacing = 4f,
                    ElementWidth = 64f,
                    ElementHeight = 82f,
                    OverflowHidden = true,
                };
                panel.Handles.Add(grid);
                panel.Append(grid);
                panel.Width.Pixels = 32f * 8f;
                panel.Height.Pixels = panel.Width.Pixels * 1.25f;
                grid.Add(Enumerable.Range(0, PlayerVariantID.Count - 1).Where(i => PlayerVariantID.Sets.Male[i]).Select(i =>
                {
                    return new VariantDisplay(i, WorldNPC.Dummy);
                }));
                break;
            case SubPanelScr.Accessories:
                for (var i = EquipType.Head; i <= EquipType.Beard; i++)
                {
                    
                }
                break;
        }
        Append(panel);
    }
    public void ReinitializePanel(PanelScreen withScreen = 0)
    {
        List.Clear();
        Screen = withScreen != 0 ? withScreen : DetermineScreen();
        AppendTitle();
        PopulatePanel();
    }
    public override void OnInitialize()
    {
        SecondaryPanel = new();
        // prep second panel for drawing player dummy
        SecondaryPanel.OnDraw += static _ =>
        {
            WorldNPC.PrintOnDummy(SchemaVal.AnalyzingSchema);
            WorldNPC.Dummy.direction = 1;
        };
        Title.SetPadding(4f);

        Panel = new(true, List._innerList, Title, Separator);
        Panel.MinWidth.Pixels = 308f;
        Panel.MinHeight.Pixels = 206f;
        Panel.Append(List);

        for (CadastralAction i = 0; i < CadastralAction.Max; i++)
        {
            var button = new CadastralButton(i)
            {
                ExclusivityGroup = i switch
                {
                    <= CadastralAction.AddNPC => 1,
                    _ => 0,
                },
                DirectCommand = i switch
                {
                    CadastralAction.TogglePanel => () =>
                    {
                        if (Panel.Parent != null)
                            Panel.Parent.RemoveChild(Panel);
                        else
                        {
                            ReinitializePanel();
                            Append(Panel);
                        }
                    },
                    _ => null,
                }
            };
            button.Height.Pixels = 39; // prevents gaps between sub buttons
            button.Width.Pixels = 38;
            button.Top.Pixels = 8;
            button.Left.Pixels = 8 + (38 * (int)i);
            Append(button);

            var parent = button;
            var search = i + 100;
            while (Enum.IsDefined(search))
            {
                var sub = new CadastralButton(search)
                {
                    Sup = parent,
                    ExclusivityGroup = button.ExclusivityGroup,
                    Width = button.Width,
                    Height = button.Height,
                    Top = button.Top,
                    Left = button.Left,
                    IgnoresMouseInteraction = true,
                };
                sub.Top.Pixels += sub.Width.Pixels * ((int)search / 100);
                parent = parent.Sub = sub;
                Append(sub);

                search += 100;
            }

            Buttons[(int)i] = button;
        }
    }
    public static void GoBack()
    {
        IngameFancyUI.Close();
        Main.LocalPlayer.GetModPlayer<CadastralPlayer>().operatingCrawler = false;
        DynamicPanel.HoverPanel = null;
        _selectState = SelectionState.None;
    }
    internal static Vector2 _selectStart;
    internal static Vector2 _selectEnd;
    internal static SelectionState _selectState;
    public override void RightMouseDown(UIMouseEvent evt)
    {
        if (evt.Target != this)
            return;
        if (_selectState != SelectionState.None)
        {
            _selection.Clear();
            _selectState = 0;
        }
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Target != this)
            return;
        foreach (var button in Buttons)
        {
            if (!button.Down)
                continue;
            switch (button.Type)
            {
                case CadastralAction.Select:
                    _selectStart = CadastralPlayer.realMouse;
                    _selectState = SelectionState.Selecting;
                    break;
                case CadastralAction.Demarcate:
                    break;
                case CadastralAction.DemarcatePolygon:
                    SoundEngine.PlaySound(SoundID.Item132);
                    if (_processingPolygonFinished)
                    {
                        var area = _processingPolygon.ToArray();
                        var box = GeometryUtils.BoundingBox(area);
                        var wBox = new Rectangle(box.Left * 16, box.Top * 16, (box.Right - box.Left) * 16, (box.Bottom - box.Top) * 16);
                        BiomeSystem.customBiomes.Add(new CustomBiome()
                        {
                            Area = area,
                            BoundingBox = box,
                            WorldBox = wBox,
                            Schema = BiomeSystem.biomesNode["Biomes"][0].AsObject()
                        }); // debug
                        _processingPolygon.Clear();
                        _processingPolygonFinished = false;
                    }
                    else
                    {
                        var up = CadastralPlayer.realPosition;
                        if (_processingPolygon.Count > 2 && up == _processingPolygon[0])
                            _processingPolygonFinished = true;
                        else
                            _processingPolygon.Add(up);
                    }
                    break;
                case CadastralAction.AddNPC:
                    SoundEngine.PlaySound(SoundID.Item132);
                    var npc = NPC.NewNPCDirect(Main.LocalPlayer.GetSource_FromThis(), CadastralPlayer.realMouse, ModContent.NPCType<WorldNPC>());
                    var modNpc = (WorldNPC)npc.ModNPC;
                    modNpc.Schema = BiomeSystem.biomesNode["NPCs"][0].AsObject();
                    break;
                case CadastralAction.AddNPCTile:
                    var tileCoords = CadastralPlayer.realPosition;
                    var tile = Main.tile[tileCoords.X, tileCoords.Y];
                    if (tile.HasTile)
                    {
                        SoundEngine.PlaySound(SoundID.Item150 with { Pitch = -0.4f, PitchVariance = 0f, Volume = 0.2f });
                        break;
                    }
                    SoundEngine.PlaySound(SoundID.Item132);
                    tile.TileType = WorldNPCTile.TileType;
                    tile.HasTile = true;
                    var tE = ModContent.GetInstance<WorldNPCTileEntity>();
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendTileSquare(Main.myPlayer, tileCoords.X, tileCoords.Y, 1, 1);
                        NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, tileCoords.X, tileCoords.Y, tE.Type);
                    }
                    tE.Place(tileCoords.X, tileCoords.Y);
                    ((WorldNPCTileEntity)TileEntity.ByPosition[new Point16(tileCoords.X, tileCoords.Y)]).Schema = BiomeSystem.biomesNode["NPCs"][0].AsObject();
                    break;
            }
        }
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_selectState is SelectionState.Selecting)
            _selectEnd = CadastralPlayer.realMouse;
    }
    public override void LeftMouseUp(UIMouseEvent evt)
    {
        if (evt.Target != this)
            return;
        foreach (var button in Buttons)
        {
            if (!button.Down)
                continue;
            switch (button.Type)
            {
                case CadastralAction.Select when _selectState == SelectionState.Selecting:
                    if (DoSelect())
                    {
                        _selectState = SelectionState.Selected;
                        ReinitializePanel();
                    }
                    else
                        _selectState = SelectionState.None;
                    break;
            }
        }
    }
    private static bool DoSelect()
    {
        var cleared = false;
        void ValidateClearedState()
        {
            if (cleared)
                return;
            cleared = true;
            _selection.Clear();
        }
        var shortSelection = _selectStart.DistanceSQ(_selectEnd) < 64f;
        var selectBox = GeometryUtils.RectangleFromPoints(_selectStart, _selectEnd);
        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is not WorldNPC)
                continue;
            if (npc.Hitbox.Intersects(selectBox))
            {
                ValidateClearedState();
                _selection.Add(npc);
                if (shortSelection)
                    break;
            }
        }
        if (shortSelection && _selection.Count != 0)
            return true;
        foreach (var kvp in TileEntity.ByPosition)
        {
            if (kvp.Value is not WorldNPCTileEntity)
                continue;
            var pos = kvp.Key;
            if (selectBox.Intersects(new Rectangle(pos.X * 16, (pos.Y - 1) * 16, 16, 32)))
            {
                ValidateClearedState();
                _selection.Add(kvp.Value);
                if (shortSelection)
                    break;
            }
        }
        if (shortSelection && _selection.Count != 0)
            return true;
        foreach (var biome in CollectionsMarshal.AsSpan(BiomeSystem.customBiomes))
        {
            if (!biome.WorldBox.Intersects(selectBox))
                continue;
            foreach (var vertex in biome.Area)
                if (selectBox.Contains(vertex.X * 16, vertex.Y * 16))
                {
                    ValidateClearedState();
                    _selection.Add(biome);
                    if (shortSelection)
                        return true;
                }
        }
        return _selection.Count != 0;
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.End(out var ss);
        spriteBatch.Begin(ss with { SamplerState = SamplerState.PointClamp });
        base.Draw(spriteBatch);
        spriteBatch.Restart(ss);
        if (savedEngine != null)
        {
            Lighting._activeEngine = savedEngine;
            savedEngine = null;
        }
    }
}

public sealed class CadastralButton(CadastralAction type) : UIElement
{
    public bool Down;
    public Action DirectCommand;
    public int ExclusivityGroup;
    public CadastralAction Type = type;
    public CadastralButton Sup;
    public CadastralButton Sub;
    public static readonly Asset<Texture2D> Sheet = ModContent.Request<Texture2D>("AdventureTools/UI/Buttons");
    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        var current = this;
        while (current.Sup != null)
        {
            current.Sup.IgnoresMouseInteraction = false;
            current = current.Sup;
        }
        current = this;
        while (current.Sub != null)
        {
            current.Sub.IgnoresMouseInteraction = false;
            current = current.Sub;
        }
    }
    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOut(evt);
        var current = this;
        while (current.Sup != null)
            current = current.Sup;
        while (current.Sub != null)
        {
            if (current.Sub.ContainsPoint(Main.MouseScreen))
                break;
            current.Sub.IgnoresMouseInteraction = true;
            current = current.Sub;
        }
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        if (DirectCommand != null)
        {
            DirectCommand();
            return;
        }
        if (Down ^= true)
        {
            var sup = Sup;
            var clickedType = Type;
            while (sup != null)
            {
                if (sup.Sup is null)
                {
                    (sup.Type, Type) = (Type, sup.Type);
                    (sup.Down, Down) = (Down, sup.Down);
                    (sup.DirectCommand, DirectCommand) = (DirectCommand, sup.DirectCommand);
                }
                sup = sup.Sup;
            }

            if (ExclusivityGroup == 0)
                return;
            foreach (var b in CadastralUIState.Instance.Buttons)
            {
                var cur = b;
                while (cur != null)
                {
                    if (cur.Type != clickedType && cur.ExclusivityGroup == ExclusivityGroup)
                        cur.Down = false;
                    cur = cur.Sub;
                }
            }
        }
    }
    public override void Update(GameTime gameTime)
    {
        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            var text = Type switch
            {
                CadastralAction.Select => Lang.misc[53],
                _ => AdventureTools.Instance.GetLocalization("UIHover." + Type),
            };
            UICommon.TooltipMouseText(text.Value);
        }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (IgnoresMouseInteraction)
            return;
        var dims = GetDimensions();
        var center = new Vector2(dims.X + dims.Width * 0.5f, dims.Y + dims.Height * 0.5f);
        var tex = Sheet.Value;
        var maxVert = 1 + (int)Enum.GetValues<CadastralAction>()[^1] / 100;
        var frame = tex.Frame(verticalFrames: (int)CadastralAction.Max, horizontalFrames: maxVert, frameY: (int)Type % 100, frameX: (int)Type / 100);
        var origin = new Vector2(tex.Width / (float)maxVert * 0.5f, tex.Height / (float)CadastralAction.Max * 0.5f);
        spriteBatch.Draw(tex, center, frame, Down ? Color.Gray : Color.White, 0f, origin, 2f, SpriteEffects.None, 0f);
    }
}

public enum CadastralAction
{
    Select,
    Demarcate,
    AddNPC,
    TogglePanel,
    Max,
    // he made a statement so strange even his gang were confused
    DemarcatePolygon = Demarcate + 100,
    AddNPCTile = AddNPC + 100,
}