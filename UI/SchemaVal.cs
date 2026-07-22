using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class BackgroundMarquee(string text, float scale = 1, bool large = false) : MarqueeText<string>(text, scale, large)
{
    public Color PanelColor = UICommon.DefaultUIBlue;
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dims = this.Dimensions;
        dims.Y += 5;
        dims.Height -= 10;
        var col = PanelColor;
        if (Parent is SchemaVal s && !s.IsMouseHovering)
            col = col.MultiplyRGBA(new Color(180, 180, 180));
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, dims, col);
        base.DrawSelf(spriteBatch);
    }
}

public sealed class SchemaVal : UIElement
{
    private const float Side = 32f;
    public static JsonObject AnalyzingSchema;
    public object BaseObject;
    public BackgroundMarquee Name = new(null) { Width = StyleDimension.FromPixelsAndPercent(-172f - (Side * 2f), 1f), Height = StyleDimension.Fill, Left = StyleDimension.FromPixels(96f + Side), TextAlignY = 0.5f, TextAlignX = 0.5f };
    public override void OnInitialize()
    {
        Name.PanelColor *= 0.5f;
        Name.PanelColor.A = byte.MaxValue;
        var m = AdventureTools.Instance;
        Append(CadastralUIState.SimpleLabel(m.GetLocalization("SchemaUI.Schema")));
        Append(Name);
        Append(new TextButton { Height = StyleDimension.Fill, Width = new(Side, 0f), Left = Name.Left - Side, Text = "<", Action = () => Cycle(-1) });
        Append(new TextButton { Height = StyleDimension.Fill, Width = new(Side, 0f), Left = Name.Left + Name.Width, Text = ">", Action = () => Cycle(1) });
        Append(new TextButton { Height = StyleDimension.Fill, Width = new(Side, 0f), Left = Name.Left + Name.Width + Side + 5f, Text = "+", Action = () =>
        {
            ref var schema = ref GetSchema();
            var npc = BaseObject is WorldNPC or WorldNPCTileEntity;
            var nuThing = new JsonObject();
            var nameNode = new JsonObject() { ["en-US"] = npc ? "Unnamed NPC" : "Unnamed Biome" };
            nuThing["Name"] = nameNode;
            var arr = (JsonArray)(npc ? BiomeSystem.biomesNode["NPCs"] : BiomeSystem.biomesNode["Biomes"]);
            arr.Add(nuThing);
            schema = nuThing;
        }});
        Append(new TextButton { HAlign = 1f, VAlign = 0.5f, Left = StyleDimension.FromPixels(-3f), MinWidth = StyleDimension.FromPixels(32f), MinHeight = StyleDimension.FromPixels(32f), Text = ItemTagHandler.GenerateTag(ContentSamples.ItemsByType[ItemID.WireKite]), Action = () =>
        {
            var schema = AnalyzingSchema = GetSchema();
            if (schema is null)
                return;
            CadastralUIState.Instance.ReinitializePanel(schema.Parent.GetPropertyName() is "Biomes" ? PanelScreen.BiomeSchema : PanelScreen.NPCSchema);
        }});
    }
    private void Cycle(int direction)
    {
        ref var schema = ref GetSchema();
        schema ??= (JsonObject)(BaseObject is WorldNPC or WorldNPCTileEntity ? BiomeSystem.biomesNode["NPCs"][0] : BiomeSystem.biomesNode["Biomes"][0]);
        if (schema.Parent is not JsonArray arr)
            return;
        var a = schema.GetElementIndex() + direction;
        var b = arr.Count;
        var idx = ((a % b) + b) % b;
        schema = (JsonObject)arr[idx];
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dimensions);
        if (GetSchema()?.TryGetPropertyValue("Name", out var nameNode) == true)
            if (SchemaUtils.TryGetLocalizedText(nameNode, out var name))
            {
                Name.SetText(name);
                Name.textScale = 1f; // i don't understand why settext calculates scale based on height lol
            }
    }
    private ref JsonObject GetSchema()
    {
        if (BaseObject is WorldNPC w)
            return ref w.Schema;
        if (BaseObject is WorldNPCTileEntity wT)
            return ref wT.Schema;
        return ref ((CustomBiome)BaseObject).Schema;
    }
}
