namespace AdventureTools;

/// <summary>
/// Extension methods for polygon
/// </summary>
public static class PolygonExtensions
{
    /// <summary>
    /// Adds the vertices.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="vertices">The vertices.</param>
    /// <returns>the same builder</returns>
    public static IPolygonBuilder AddVertices(this IPolygonBuilder builder, params int[] vertices)
    {
        return builder.AddVertices(vertices);
    }
}
