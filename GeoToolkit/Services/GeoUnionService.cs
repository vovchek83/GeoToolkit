using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoToolkit.Helpers;
using GeoToolkit.Interfaces;

namespace GeoToolkit.Services;

/// <summary>
/// Computes the geometric union of GeoJSON polygons using NetTopologySuite.
/// </summary>
public class GeoUnionService : IGeoUnionService, ISingletonService
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

    /// <summary>
    /// Unions two features into a single feature.
    /// </summary>
    public Feature Union(Feature a, Feature b)
    {
        var result = GeoJsonNtsConverter.ToNts(a.Geometry).Union(GeoJsonNtsConverter.ToNts(b.Geometry));
        return new Feature(GeoJsonNtsConverter.ToGeoJson(result), new Dictionary<string, object>
        {
            { "name", "Union Result" }
        });
    }

    /// <summary>
    /// Returns the area present in either feature but not in both (XOR).
    /// </summary>
    public Feature SymmetricDifference(Feature a, Feature b)
    {
        var result = GeoJsonNtsConverter.ToNts(a.Geometry).SymmetricDifference(GeoJsonNtsConverter.ToNts(b.Geometry));
        return new Feature(GeoJsonNtsConverter.ToGeoJson(result), new Dictionary<string, object>
        {
            { "name", "Symmetric Difference Result" }
        });
    }

    /// <summary>
    /// Groups features by <paramref name="propertyKey"/> and unions all features
    /// within each group into one feature, preserving the group value as a property.
    /// </summary>
    public FeatureCollection Dissolve(FeatureCollection featureCollection, string propertyKey)
    {
        var groups = featureCollection.Features.GroupBy(f =>
            f.Properties != null && f.Properties.TryGetValue(propertyKey, out var v)
                ? v?.ToString() ?? string.Empty
                : string.Empty);

        var dissolved = new List<Feature>();
        foreach (var group in groups)
        {
            var unionFc = Union(new FeatureCollection(group.ToList()));
            if (unionFc.Features.Count == 0) continue;

            dissolved.Add(new Feature(
                unionFc.Features[0].Geometry,
                new Dictionary<string, object> { { propertyKey, group.Key } }
            ));
        }

        return new FeatureCollection(dissolved);
    }
}
