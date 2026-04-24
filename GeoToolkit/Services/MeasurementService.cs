using GeoJSON.Net.Geometry;
using GeoToolkit.Interfaces;

namespace GeoToolkit.Services;

/// <summary>
/// Provides geodetic measurement operations including distance, perimeter, and area
/// calculations on WGS84 geographic coordinates.
/// </summary>
public class MeasurementService : IMeasurementService, ISingletonService
{
    /// <summary>Mean Earth radius used in Haversine and spherical area formulas (meters).</summary>
    private const double EarthRadiusM = 6371000.0;

    /// <summary>
    /// Calculates the great-circle distance between two points using the Haversine formula.
    /// Accurate to within ~0.5% for most distances on Earth.
    /// </summary>
    /// <param name="from">Starting position in WGS84 coordinates.</param>
    /// <param name="to">Ending position in WGS84 coordinates.</param>
    /// <returns>Distance in meters.</returns>
    public double CalculateDistance(Position from, Position to)
    {
        var lat1 = from.Latitude * Math.PI / 180;
        var lat2 = to.Latitude * Math.PI / 180;
        var dLat = (to.Latitude - from.Latitude) * Math.PI / 180;
        var dLon = (to.Longitude - from.Longitude) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1) * Math.Cos(lat2)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return EarthRadiusM * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    /// <summary>
    /// Calculates the perimeter of a polygon's outer ring by summing the
    /// Haversine distances between consecutive vertices.
    /// </summary>
    /// <param name="polygon">The polygon to measure.</param>
    /// <returns>Perimeter length in meters.</returns>
    public double CalculatePerimeter(Polygon polygon)
    {
        var coords = polygon.Coordinates[0].Coordinates;
        double total = 0;
        for (int i = 0; i < coords.Count - 1; i++)
            total += CalculateDistance((Position)coords[i], (Position)coords[i + 1]);
        return total;
    }

    /// <summary>
    /// Calculates the area of a polygon's outer ring using the spherical trapezoidal
    /// rule (Girard's theorem approximation). Accurate for polygons spanning
    /// up to a few degrees; for very large polygons consider a projected coordinate system.
    /// Holes (inner rings) are not subtracted.
    /// </summary>
    /// <param name="polygon">The polygon to measure.</param>
    /// <returns>Area in square meters.</returns>
    public double CalculateArea(Polygon polygon)
    {
        var coords = polygon.Coordinates[0].Coordinates;
        int n = coords.Count - 1; // last coordinate equals first

        double area = 0;
        for (int i = 0; i < n; i++)
        {
            var lon1 = coords[i].Longitude * Math.PI / 180;
            var lon2 = coords[(i + 1) % n].Longitude * Math.PI / 180;
            var lat1 = coords[i].Latitude * Math.PI / 180;
            var lat2 = coords[(i + 1) % n].Latitude * Math.PI / 180;
            area += (lon2 - lon1) * (2 + Math.Sin(lat1) + Math.Sin(lat2));
        }

        return Math.Abs(area * EarthRadiusM * EarthRadiusM / 2);
    }
}
