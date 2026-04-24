using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace GeoToolkit.Interfaces;

public interface IGeoUnionService
{
    FeatureCollection Union(FeatureCollection featureCollection);
    FeatureCollection Union(IEnumerable<Polygon> polygons);
}
