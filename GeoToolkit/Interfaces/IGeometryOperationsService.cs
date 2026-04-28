using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace GeoToolkit.Interfaces;

public interface IGeometryOperationsService
{
    Feature Intersection(Feature a, Feature b);
    Feature Difference(Feature a, Feature b);
    Feature Buffer(Feature feature, double distanceInMeters);
    Feature Simplify(Feature feature, double tolerance);
    bool IsValid(Feature feature);
    Feature Repair(Feature feature);
    string ToWkt(Feature feature);
    string ToWkt(FeatureCollection featureCollection);
    string ToWkt(Polygon polygon);
}
