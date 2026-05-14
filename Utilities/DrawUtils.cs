using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace AdventureTools.Utilities;

public static class DrawUtils
{
    private static readonly int[] indexData = [0, 1];
    public static void DrawLine(Vector2 a, Vector2 b, Color color, float thickness = 1f)
    {
        var gd = Main.graphics.GraphicsDevice;
        var aVert = new VertexPositionColor(new Vector3(a.X, a.Y, 0f), color);
        var bVert = new VertexPositionColor(new Vector3(b.X, b.Y, 0f), color);
        gd.DrawUserIndexedPrimitives(PrimitiveType.LineList, [aVert, bVert], 0, 2, indexData, 0, 1);
    }
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
    public static void Draw9Slice(SpriteBatch sb, Texture2D tex, Rectangle rect, bool skipCorners = false)
    {
        var col = Color.White;
        Vector2 quadSize = new(tex.Width / 3f, tex.Height / 3f);
        var xScale = (rect.Width - quadSize.X * 2) / quadSize.X;
        var yScale = (rect.Height - quadSize.Y * 2) / quadSize.Y;

        Rectangle topSideFrame = tex.Frame(3, 3, 1);
        sb.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y), topSideFrame, col, 0, default, new Vector2(xScale, 1), SpriteEffects.None, 0f);
        Rectangle leftSideFrame = tex.Frame(3, 3, 0, 1);
        sb.Draw(tex, new Vector2(rect.X, rect.Y + quadSize.Y), leftSideFrame, col, 0, default, new Vector2(1, yScale), SpriteEffects.None, 0f);
        Rectangle centerFrame = tex.Frame(3, 3, 1, 1);
        sb.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y + quadSize.Y), centerFrame, Color.Red, 0, default, new Vector2(xScale, yScale), SpriteEffects.None, 0f);
        Rectangle rightSideFrame = tex.Frame(3, 3, 2, 1);
        sb.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y + quadSize.Y), rightSideFrame, col, 0, default, new Vector2(1, yScale), SpriteEffects.None, 0f);
        Rectangle bottomSideFrame = tex.Frame(3, 3, 1, 2);
        sb.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y + rect.Height - quadSize.Y), bottomSideFrame, col, 0, default, new Vector2(xScale, 1), SpriteEffects.None, 0f);

        if (skipCorners)
            return;

        Rectangle topLeftCorner = tex.Frame(3, 3);
        sb.Draw(tex, new Vector2(rect.X, rect.Y), topLeftCorner, col, 0, default, 1, SpriteEffects.None, 0f);
        Rectangle topRightCorner = tex.Frame(3, 3, 2);
        sb.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y), topRightCorner, col, 0, default, 1, SpriteEffects.None, 0f);
        Rectangle bottomLeftCorner = tex.Frame(3, 3, 0, 2);
        sb.Draw(tex, new Vector2(rect.X, rect.Y + rect.Height - quadSize.Y), bottomLeftCorner, col, 0, default, 1, SpriteEffects.None, 0f);
        Rectangle bottomRightCorner = tex.Frame(3, 3, 2, 2);
        sb.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y + rect.Height - quadSize.Y), bottomRightCorner, col, 0, default, 1, SpriteEffects.None, 0f);
    }
}
