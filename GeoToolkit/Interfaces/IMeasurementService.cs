using GeoJSON.Net.Geometry;

namespace GeoToolkit.Interfaces;

public interface IMeasurementService
{
    /// <summary>Returns area in square meters.</summary>
    double CalculateArea(Polygon polygon);

    /// <summary>Returns perimeter in meters.</summary>
    double CalculatePerimeter(Polygon polygon);

    /// <summary>Returns distance in meters (Haversine).</summary>
    double CalculateDistance(Position from, Position to);
}
