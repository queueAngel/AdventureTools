namespace AdventureTools;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vertex = Microsoft.Xna.Framework.Vector2;

/// <summary>
/// subclass container for trapzoidation
/// </summary>
public partial class Trapezoidation
{
    /// <summary>
    /// Compares two edges
    /// </summary>
    private class EdgeComparer : IComparer<TrapezoidEdge>
    {
        private readonly IReadOnlyList<Vertex> vertices;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeComparer"/> class.
        /// </summary>
        /// <param name="vertices">the real vertices referenced by vertex ids</param>
        public EdgeComparer(IReadOnlyList<Vertex> vertices)
        {
            this.vertices = vertices;
        }

        /// <summary>
        /// Test if the left vertex of x is above y.
        /// </summary>
        /// <param name="x">the current added value</param>
        /// <param name="y">the edge that is already part of the tree</param>
        /// <returns>a comparison result</returns>
        public int Compare(TrapezoidEdge x, TrapezoidEdge y)
        {
            var value = x;
            var storage = y;
            var vertexOfValue = value.Left == storage.Left ? value.Right : value.Left;
            return this.IsVertexAbove(vertexOfValue, storage) ? 1 : -1;
        }

        /// <summary>
        /// Test if the ordering of the edges is correct, both edges have a common point on the left
        /// </summary>
        /// <param name="lower">the lower edge</param>
        /// <param name="upper">the upper edge</param>
        /// <returns>true if upper is above lower</returns>
        /// <remarks>
        /// take the wider edge (larger X span) to avoid a large slope.
        /// </remarks>
        public bool EdgeOrderingWithCommonLeftIsCorrect(TrapezoidEdge lower, TrapezoidEdge upper)
        {
            var left = this.vertices[upper.Left];
            var upperRight = this.vertices[upper.Right];
            var lowerRight = this.vertices[lower.Right];

            var leftY = left.Y;
            var upperRightX = upperRight.X;
            var upperRightY = upperRight.Y;
            var lowerRightX = lowerRight.X;
            var lowerRightY = lowerRight.Y;

            if ((upperRightY > leftY) != (lowerRightY > leftY))
            {
                return upperRightY > lowerRightY;
            }

            if (upperRightX > lowerRightX)
            {
                if (upperRightY < leftY && upperRightY > lowerRightY)
                {
                    return true;
                }
                else if (upperRightY > leftY && upperRightY < lowerRightY)
                {
                    return false;
                }
                else
                {
                    return !IsVertexAboveSlow(ref lowerRight, ref left, ref upperRight);
                }
            }
            else
            {
                if (lowerRightY > leftY && upperRightY > lowerRightY)
                {
                    return true;
                }
                else if (lowerRightY < leftY && upperRightY < lowerRightY)
                {
                    return false;
                }
                else
                {
                    return IsVertexAboveSlow(ref upperRight, ref left, ref lowerRight);
                }
            }
        }

        /// <summary>
        /// Test if the vertex is above this edge by calculating the edge.Y at vertex.X
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="left">The left vertex of the edge.</param>
        /// <param name="right">The right vertex of the edge.</param>
        /// <returns>true if the verex is above</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsVertexAboveSlow(ref Vertex vertex, ref Vertex left, ref Vertex right)
        {
            var xSpan = right.X - left.X;

            if (xSpan < Epsilon * Epsilon)
            {
                return vertex.Y > left.Y;
            }

            var yOfEdgeAtVertex = ((vertex.X - left.X) / xSpan * (right.Y - left.Y)) + left.Y;
            return yOfEdgeAtVertex < vertex.Y;
        }

        /// <summary>
        /// Test if the vertex is above the line that is formed by the edge
        /// </summary>
        /// <param name="vertexId">The vertex identifier.</param>
        /// <param name="edge">The edge.</param>
        /// <returns>true if the vertex is above the edge</returns>
        /// <remarks>
        /// This is called only during insert operations, therefore value.left is larger than storage.left.
        /// Try to find the result without calculation first, then calculate the storage.Y at value.Left.X
        /// </remarks>
        private bool IsVertexAbove(int vertexId, TrapezoidEdge edge)
        {
            var vertex = this.vertices[vertexId];
            var left = this.vertices[edge.Left];
            var right = this.vertices[edge.Right];

            // this is very likely as the points are added in order left to right
            if (vertex.X >= left.X)
            {
                if (vertex.Y > left.Y)
                {
                    if (left.Y >= right.Y || (vertex.X < right.X && vertex.Y > right.Y))
                    {
                        return true;
                    }
                }
                else
                {
                    if (left.Y < right.Y || (vertex.X < right.X && vertex.Y < right.Y))
                    {
                        return false;
                    }
                }
            }

            return IsVertexAboveSlow(ref vertex, ref left, ref right);
        }
    }
}
