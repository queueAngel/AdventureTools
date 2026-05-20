using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;

namespace AdventureTools.Utilities;

public static class GeometryUtils
{
    public static int IsLeft(UPoint16 p0, UPoint16 p1, UPoint16 p2) 
        => (p1.X - p0.X) * (p2.Y - p0.Y) - (p2.X - p0.X) * (p1.Y - p0.Y);
    public static bool IsInside(UPoint16 p, UPoint16[] polygon)
    {
        int wn = 0;
        for (int i = 0; i < polygon.Length; i++)
        {
            ref var b = ref polygon[(i + 1) % polygon.Length];
            if (polygon[i].Y <= p.Y)
            {          // start y <= P.y
                if (b.Y > p.Y)      // an upward crossing
                    if (IsLeft(polygon[i], b, p) > 0)  // P left of  edge
                        ++wn;            // have  a valid up intersect
            }
            else
            {                        // start y > P.y (no test needed)
                if (b.Y <= p.Y)     // a downward crossing
                    if (IsLeft(polygon[i], b, p) < 0)  // P right of  edge
                        --wn;            // have  a valid down intersect
            }
        }
        return wn != 0;
    }
    public static UQuadrat16 BoundingBox(UPoint16[] polygon)
    {
        ushort minX = ushort.MaxValue;
        ushort minY = ushort.MaxValue;
        ushort maxX = 0;
        ushort maxY = 0;
        for (int i = 0; i < polygon.Length; i++)
        {
            var vertex = polygon[i];
            if (vertex.X < minX) minX = vertex.X;
            if (vertex.X > maxX) maxX = vertex.X;
            if (vertex.Y < minY) minY = vertex.Y;
            if (vertex.Y > maxY) maxY = vertex.Y;
        }
        return new() { Left = minX, Top = minY, Right = maxX, Bottom = maxY };
    }
    public static UPoint16 ToUPoint16(Vector2 v) => new() { X = (ushort)((int)v.X >> 4), Y = (ushort)((int)v.Y >> 4) };
    public static int[] Triangulate(UPoint16[] polygon)
    {
        var toV = polygon.Select(p => new Vector2(p.X, p.Y));
        var poly = Polygon.Build(toV.ToArray()).Auto();
        var triangulator = new PolygonTriangulator(poly);
        return triangulator.BuildTriangles();
    }
    public static Rectangle RectangleFromPoints(Vector2 a, Vector2 b) => new Rectangle
    {
        X = (int)Math.Min(a.X, b.X),
        Y = (int)Math.Min(a.Y, b.Y),
        Width = (int)Math.Abs(a.X - b.X),
        Height = (int)Math.Abs(a.Y - b.Y),
    };
}

public struct GraphicsTriangle
{
    public VertexPositionColor A, B, C;
}
