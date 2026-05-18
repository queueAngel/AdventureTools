using AdventureTools.Core;
using AdventureTools.Items;
using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.Rendering;
using Json.Pointer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

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
public sealed class CadastralUIState : UIState
{
    internal static readonly List<UPoint16> _processingPolygon = [];
    internal static bool _processingPolygonFinished;
    internal static readonly HashSet<object> _selection = [];
    public static CadastralUIState Instance { get; } = new();
    public DynamicPanel Panel;
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
    public PanelScreen DetermineScreen()
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
            JsonObject j => j.Parent.GetPropertyName() is "Biomes" ? PanelScreen.BiomeSchema : PanelScreen.NPCSchema,
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
                    AddPreviewElement(selected);
                }
                break;
            case PanelScreen.BiomeSchema:
                var selectedBiome = (CustomBiome)_selection.ElementAt(0);
                AddLocalizedNode("Name", selectedBiome.Schema);
                break;
            case PanelScreen.NPC:
                var selectedNPC = (NPC)_selection.ElementAt(0);
                var wNPC = (WorldNPC)selectedNPC.ModNPC;
                var panel = new UIElement() { Height = StyleDimension.FromPixels(128f), Width = StyleDimension.FromPixelsAndPercent(-112f, 1f) }.WithPadding(16f);
                var viewport = new WorldViewport() { targetCam = () => selectedNPC.Center, Width = StyleDimension.FromPixels(96f), Height = StyleDimension.Fill };
                var boolVal = new BoolVal<WorldNPC>(wNPC, w => w.Static, (w, b) => w.Static = b) { Left = StyleDimension.FromPixels(108f), Height = StyleDimension.FromPixelsAndPercent(-8f, 0.5f), Width = StyleDimension.FromPercent(1f) };
                panel.Append(viewport);
                panel.Append(boolVal);
                List.Add(panel);
                break;
        }
    }
    public void AddLocalizedNode(string id, JsonObject schema)
    {
        List.Add(new LocalizedTextElement { BaseObject = schema, Pointer = JsonPointer.Parse("/" + id), Height = StyleDimension.FromPixels(32f) });
    }
    public void AddPreviewElement(object obj)
    {

    }
    public void ReinitializePanel()
    {
        List.Clear();
        Screen = DetermineScreen();
        AppendTitle();
        PopulatePanel();
    }
    public override void OnInitialize()
    {
        Title.SetPadding(4f);

        Panel = new();
        Panel.MinWidth.Pixels = 200;
        Panel.MinHeight.Pixels = 400;
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
                    _selectState = SelectionState.Selected;
                    DoSelect();
                    ReinitializePanel();
                    break;
            }
        }
    }
    private static void DoSelect()
    {
        _selection.Clear();
        var shortSelection = _selectStart.DistanceSQ(_selectEnd) < 64f;
        var selectBox = GeometryUtils.RectangleFromPoints(_selectStart, _selectEnd);
        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is not WorldNPC)
                continue;
            if (npc.Hitbox.Intersects(selectBox))
            {
                _selection.Add(npc);
                if (shortSelection)
                    break;
            }
        }
        if (shortSelection && _selection.Count != 0)
            return;
        foreach (var kvp in TileEntity.ByPosition)
        {
            if (kvp.Value is not WorldNPCTileEntity)
                continue;
            var pos = kvp.Key;
            if (selectBox.Intersects(new Rectangle(pos.X * 16, (pos.Y - 1) * 16, 16, 32)))
            {
                _selection.Add(kvp.Value);
                if (shortSelection)
                    return;
            }
        }
        if (shortSelection && _selection.Count != 0)
            return;
        foreach (var biome in CollectionsMarshal.AsSpan(BiomeSystem.customBiomes))
        {
            if (!biome.WorldBox.Intersects(selectBox))
                continue;
            foreach (var vertex in biome.Area)
                if (selectBox.Contains(vertex.X * 16, vertex.Y * 16))
                {
                    _selection.Add(biome);
                    if (shortSelection)
                        return;
                }
        }
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.End(out var ss);
        spriteBatch.Begin(ss with { SamplerState = SamplerState.PointClamp });
        base.Draw(spriteBatch);
        spriteBatch.Restart(ss);
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