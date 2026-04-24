using GeoToolkit.Models;

namespace GeoToolkit.Interfaces;

public interface IElevationAdjustmentService
{
    /// <summary>Converts a point's ellipsoidal height to orthometric (MSL) using EGM96.</summary>
    GeoPoint EllipsoidalToOrthometric(GeoPoint point);

    /// <summary>Converts a point's orthometric height to ellipsoidal using EGM96.</summary>
    GeoPoint OrthometricToEllipsoidal(GeoPoint point);

    /// <summary>Applies EllipsoidalToOrthometric to a batch of points.</summary>
    IEnumerable<GeoPoint> ApplyGeoidCorrection(IEnumerable<GeoPoint> points);
}
