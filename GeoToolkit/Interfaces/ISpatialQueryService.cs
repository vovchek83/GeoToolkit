using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace GeoToolkit.Interfaces;

public interface ISpatialQueryService
{
    bool PointInPolygon(Position point, Polygon polygon);
    Feature GetBoundingBox(FeatureCollection featureCollection);
    Position GetCentroid(Feature feature);
}
