using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace GeoToolkit.Interfaces;

public interface IGeoUnionService
{
    FeatureCollection Union(FeatureCollection featureCollection);
    FeatureCollection Union(IEnumerable<Polygon> polygons);
    Feature Union(Feature a, Feature b);
    Feature SymmetricDifference(Feature a, Feature b);
    FeatureCollection Dissolve(FeatureCollection featureCollection, string propertyKey);
}
