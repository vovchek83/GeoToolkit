using GeoJSON.Net.Feature;

namespace GeoToolkit.Interfaces;

public interface IGeometryOperationsService
{
    Feature Intersection(Feature a, Feature b);
    Feature Difference(Feature a, Feature b);
    Feature Buffer(Feature feature, double distanceInMeters);
    Feature Simplify(Feature feature, double tolerance);
    bool IsValid(Feature feature);
    Feature Repair(Feature feature);
}
