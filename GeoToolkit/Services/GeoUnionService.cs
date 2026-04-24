using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoToolkit.Helpers;
using GeoToolkit.Interfaces;

namespace GeoToolkit.Services;

/// <summary>
/// Computes the geometric union of GeoJSON polygons using NetTopologySuite.
/// </summary>
public class GeoUnionService : IGeoUnionService, IScopedService
{
    /// <summary>
    /// Unions all polygon features in the given <see cref="FeatureCollection"/> into a single feature.
    /// Overlapping polygons are merged; disjoint polygons produce a <c>MultiPolygon</c>.
    /// Returns an empty collection when the input has no features.
    /// </summary>
    /// <param name="featureCollection">Collection of features whose geometries will be unioned.</param>
    /// <returns>A <see cref="FeatureCollection"/> containing one feature with the union geometry.</returns>
    public FeatureCollection Union(FeatureCollection featureCollection)
    {
        if (featureCollection.Features.Count == 0)
            return new FeatureCollection();

        var geometries = featureCollection.Features
            .Select(f => GeoJsonNtsConverter.ToNts(f.Geometry))
            .ToArray();

        var union = geometries.Aggregate((a, b) => a.Union(b));

        var feature = new Feature(GeoJsonNtsConverter.ToGeoJson(union), new Dictionary<string, object>
        {
            { "name", "Union Result" }
        });

        return new FeatureCollection(new List<Feature> { feature });
    }

    /// <summary>
    /// Unions a list of <see cref="Polygon"/> objects into a single feature.
    /// Each polygon is wrapped in a <see cref="Feature"/> and delegated to
    /// <see cref="Union(FeatureCollection)"/>.
    /// </summary>
    /// <param name="polygons">Polygons to union.</param>
    /// <returns>A <see cref="FeatureCollection"/> containing one feature with the union geometry.</returns>
    public FeatureCollection Union(IEnumerable<Polygon> polygons)
    {
        var features = polygons.Select(p => new Feature(p)).ToList();
        return Union(new FeatureCollection(features));
    }
}
