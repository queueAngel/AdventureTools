using AdventureTools.Utilities;
using AdventureTools.WorldNPCs;
using Daybreak.Common.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace AdventureTools.UI;

public sealed class BackgroundMarquee : MarqueeText<string>
{
    private static readonly Asset<Texture2D> _statPanel = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel");
    public Color PanelColor = UICommon.DefaultUIBlue;
    public BackgroundMarquee(string text, float scale = 1, bool large = false) : base(text, scale, large) { }
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
    public static JsonObject AnalyzingSchema;
    public object BaseObject;
    public BackgroundMarquee Name = new(null) { Width = StyleDimension.FromPixelsAndPercent(-140f, 1f), Height = StyleDimension.Fill, Left = StyleDimension.FromPixels(96f), TextAlignY = 0.5f, TextAlignX = 0.5f, VAlign = 0.5f};
    public override void OnInitialize()
    {
        Name.PanelColor *= 0.5f;
        Name.PanelColor.A = byte.MaxValue;
        var m = AdventureTools.Instance;
        Append(new UIText(m.GetLocalization("SchemaUI.Schema")) { Left = StyleDimension.FromPixels(10f), TextOriginX = 0f, TextOriginY = 0.5f, VAlign = 0.5f, MinWidth = StyleDimension.FromPixels(64f), MinHeight = StyleDimension.FromPixels(16f)});
        Append(Name);
        Append(new TextButton() { HAlign = 1f, VAlign = 0.5f, Left = StyleDimension.FromPixels(-3f), MinWidth = StyleDimension.FromPixels(32f), MinHeight = StyleDimension.FromPixels(32f), Text = ItemTagHandler.GenerateTag(ContentSamples.ItemsByType[ItemID.WireKite]), Action = () =>
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
            {
                Name.SetText(name);
                Name.textScale = 1f; // i don't understand why settext calculates scale based on height lol
            }
    }
    private JsonObject GetSchema() => BaseObject is WorldNPC w ? w.Schema : BaseObject is WorldNPCTileEntity wT ? wT.Schema : ((CustomBiome)BaseObject).Schema;
}
