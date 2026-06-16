using AdventureTools.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using ReLogic.Content;
using System.Security.Cryptography;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace AdventureTools.Utilities;

public static class DrawUtils
{
    public static readonly Asset<Effect> OutlineShader = ModContent.Request<Effect>(nameof(AdventureTools) + "/Shaders/Outline", AssetRequestMode.ImmediateLoad);
    public static VertexPositionColor[] DrawPolygon(UPoint16[] poly, Color color)
    {
        var vertices = new VertexPositionColor[poly.Length];
        for (int i = 0; i < poly.Length; i++)
        {
            ref var vtx = ref poly[i];
            vertices[i] = new(new Vector3(vtx.X * 16, vtx.Y * 16, 0f), color);
        }
        DrawPolygonDirect(vertices, poly);
        return vertices;
    }
    public static void DrawPolygonDirect(VertexPositionColor[] poly, UPoint16[] ogPoly)
    {
        var triangulated = GeometryUtils.Triangulate(ogPoly);

        var gd = Main.graphics.GraphicsDevice;
        gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, poly, 0, ogPoly.Length, triangulated, 0, triangulated.Length / 3);

    }
    public static void Draw9Slice(SpriteBatch sb, Texture2D tex, Rectangle rect, Color col = default)
    {
        if (col.PackedValue is 0)
            col = Color.White;

        Vector2 quadSize = new(tex.Width / 3f, tex.Height / 3f);
        var xScale = (rect.Width - quadSize.X * 2) / quadSize.X;
        var yScale = (rect.Height - quadSize.Y * 2) / quadSize.Y;

        Rectangle topSideFrame = tex.Frame(3, 3, 1);
        sb.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y), topSideFrame, col, 0, default, new Vector2(xScale, 1), SpriteEffects.None, 0f);
        Rectangle leftSideFrame = tex.Frame(3, 3, 0, 1);
        sb.Draw(tex, new Vector2(rect.X, rect.Y + quadSize.Y), leftSideFrame, col, 0, default, new Vector2(1, yScale), SpriteEffects.None, 0f);
        Rectangle centerFrame = tex.Frame(3, 3, 1, 1);
        sb.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y + quadSize.Y), centerFrame, col, 0, default, new Vector2(xScale, yScale), SpriteEffects.None, 0f);
        Rectangle rightSideFrame = tex.Frame(3, 3, 2, 1);
        sb.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y + quadSize.Y), rightSideFrame, col, 0, default, new Vector2(1, yScale), SpriteEffects.None, 0f);
        Rectangle bottomSideFrame = tex.Frame(3, 3, 1, 2);
        sb.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y + rect.Height - quadSize.Y), bottomSideFrame, col, 0, default, new Vector2(xScale, 1), SpriteEffects.None, 0f);
        Rectangle topLeftCorner = tex.Frame(3, 3);
        sb.Draw(tex, new Vector2(rect.X, rect.Y), topLeftCorner, col, 0, default, 1, SpriteEffects.None, 0f);
        Rectangle topRightCorner = tex.Frame(3, 3, 2);
        sb.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y), topRightCorner, col, 0, default, 1, SpriteEffects.None, 0f);
        Rectangle bottomLeftCorner = tex.Frame(3, 3, 0, 2);
        sb.Draw(tex, new Vector2(rect.X, rect.Y + rect.Height - quadSize.Y), bottomLeftCorner, col, 0, default, 1, SpriteEffects.None, 0f);
        Rectangle bottomRightCorner = tex.Frame(3, 3, 2, 2);
        sb.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y + rect.Height - quadSize.Y), bottomRightCorner, col, 0, default, 1, SpriteEffects.None, 0f);
    }
    public static void DrawConfigPanel(this UIElement elem, SpriteBatch sb, out CalculatedStyle dimensions)
    {
        dimensions = elem.GetDimensions();
        var num = dimensions.Width;
        var vector = new Vector2(dimensions.X, dimensions.Y);
        var color = UICommon.DefaultUIBlue;
        if (!elem.IsMouseHovering)
            color = color.MultiplyRGBA(new Color(180, 180, 180));
        var position = vector;

        Terraria.ModLoader.Config.UI.ConfigElement.DrawPanel2(sb, position, TextureAssets.SettingsPanel.Value, num, dimensions.Height, color);
    }
    public static readonly Asset<Texture2D> ToggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");
    public static void DrawCoolToggle(SpriteBatch sB, in CalculatedStyle dimensions, ref float anim, bool value, float xOffset = -10f)
    {
        var tex = ToggleTexture.Value;
        var sourceRectangle = new Rectangle(0, 0, tex.Width / 2, tex.Height);
        var sourceFull = sourceRectangle;
        sourceFull.X += sourceFull.Width + 1;
        sourceFull.Width -= 1;
        var drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width + xOffset, dimensions.Y + dimensions.Height * 0.5f);
        var b = Color.Black * 0.9f;
        sB.Draw(tex, drawPosition, new Rectangle(sourceFull.X, 0, 8, sourceFull.Height), b, 0f, new Vector2(16f, sourceFull.Height * 0.5f), 1, 0, 0);
        sB.Draw(tex, drawPosition, new Rectangle(sourceFull.X + 6, 0, 2, sourceFull.Height), b, 0f, new Vector2(1f, sourceFull.Height * 0.5f), new Vector2(8f, 1f), 0, 0);
        sB.Draw(tex, drawPosition, new Rectangle(sourceFull.X + 5, 0, 9, sourceFull.Height), b, 0f, new Vector2(-6f, sourceFull.Height * 0.5f), 1, 0, 0);

        anim = float.Lerp(anim, value ? 1f : 0f, 0.25f);
        var useRect = value ? sourceFull : sourceRectangle;
        sB.Draw(tex, drawPosition + new Vector2((anim * 2f - 1f) * 8f, 0f), useRect, Color.White, 0f, new Vector2(useRect.Width * 0.5f, useRect.Height * 0.5f), Vector2.One, SpriteEffects.None, 0f);
    }
}
