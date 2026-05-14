namespace AdventureTools;

using System;
using System.Linq;
using System.Runtime.Serialization;

/// <summary>
/// polygon data for exceptions during polygon triangulation
/// </summary>
[Serializable]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Standardausnahmekonstruktoren implementieren", Justification = "Exception must have a polygon")]
public class TriangulationException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangulationException"/> class.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <param name="edgeCreateCode">The edge create code.</param>
    /// <param name="innerException">The inner exception.</param>
    public TriangulationException(Polygon polygon, string edgeCreateCode, Exception innerException)
        : this(polygon, edgeCreateCode, innerException.Message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TriangulationException"/> class.
    /// </summary>
    /// <param name="polygon">The polygon.</param>
    /// <param name="edgeCreateCode">The edge create code.</param>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TriangulationException(Polygon polygon, string edgeCreateCode, string message, Exception innerException)
        : base(message, innerException)
    {
        this.PolygonCreateCode = BuildPolygonCode(polygon);
        this.EdgeCreateCode = edgeCreateCode;
    }

    /// <summary>
    /// Gets the code to feed the edges to a polygon builder.
    /// </summary>
    public string EdgeCreateCode { get; }

    /// <summary>
    /// Gets the code to create the polygon in a unittest
    /// </summary>
    public string PolygonCreateCode { get; }

    /// <summary>
    /// Create a polygon string that can be used to create the polygon
    /// </summary>
    /// <param name="polygon">The polygon to convert to code.</param>
    /// <returns>polygon as code</returns>
    internal static string BuildPolygonCode(Polygon polygon)
    {
        if (polygon == null)
        {
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder();
        var culture = System.Globalization.CultureInfo.InvariantCulture;

        sb.AppendLine("var vertices = new[]");
        sb.AppendLine("{");

        var vertexStrings = polygon.Vertices.Select(
            x => string.Format(culture, "    new Vertex({0:0.0000000}f, {1:0.0000000}f),", x.X, x.Y));

        sb.AppendLine(string.Join(Environment.NewLine, vertexStrings));
        sb.AppendLine("};");
        sb.AppendLine(string.Empty);

        sb.AppendLine("var polygon = Polygon.Build(vertices)");
        foreach (var subPolygonId in polygon.SubPolygonIds)
        {
            sb.AppendLine($"    .AddVertices({string.Join(", ", polygon.SubPolygonVertices(subPolygonId))})");
            sb.AppendLine($"    .ClosePartialPolygon()");
        }

        var fusionVerticex = polygon.SubPolygonIds
            .SelectMany(x => polygon.SubPolygonVertices(x))
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key);
        sb.AppendLine($"    .Close({string.Join(", ", fusionVerticex)});");

        return sb.ToString();
    }
}
