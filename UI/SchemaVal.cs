using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Nodes;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class SchemaVal : UIElement
{
    public static JsonObject AnalyzingSchema;
    public object BaseObject;
    public MarqueeText<string> Name = new(null) { Width = StyleDimension.FromPixelsAndPercent(-112f, 1f), Left = StyleDimension.FromPixels(80f)};
    public override void OnInitialize()
    {
        var m = AdventureTools.Instance;
        Append(new UIText(m.GetLocalization("SchemaUI.Schema")) { TextOriginX = 0f, TextOriginY = 0.5f, VAlign = 0.5f, MinWidth = StyleDimension.FromPixels(64f), MinHeight = StyleDimension.FromPixels(16f)});
        Append(Name);
        Append(new TextButton() { MinWidth = StyleDimension.FromPixels(32f), MinHeight = StyleDimension.FromPixels(32f), Text = ItemTagHandler.GenerateTag(ContentSamples.ItemsByType[ItemID.WireKite]), Action = () =>
        {
            var schema = AnalyzingSchema = GetSchema();
            CadastralUIState.Instance.ReinitializePanel(schema.Parent.GetPropertyName() is "Biomes" ? PanelScreen.BiomeSchema : PanelScreen.NPCSchema);
        }});
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        this.DrawConfigPanel(spriteBatch, out var dimensions);
        if (GetSchema().TryGetPropertyValue("Name", out var nameNode))
            if (SchemaUtils.TryGetLocalizedText(nameNode, out var name))
                Name.SetText(name);
    }
    private JsonObject GetSchema() => BaseObject is WorldNPC w ? w.Schema : BaseObject is WorldNPCTileEntity wT ? wT.Schema : ((CustomBiome)BaseObject).Schema;
}
