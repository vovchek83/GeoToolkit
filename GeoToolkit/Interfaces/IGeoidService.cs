namespace GeoToolkit.Interfaces;

public interface IGeoidService
{
    /// <summary>
    /// Returns EGM96 geoid undulation (meters) for the given WGS84 coordinates.
    /// Positive value means geoid is above the ellipsoid.
    /// </summary>
    double GetUndulation(double latitude, double longitude);

    /// <summary>
    /// Converts ellipsoidal height (HAE) to orthometric height (MSL) using EGM96.
    /// </summary>
    double EllipsoidalToOrthometric(double latitude, double longitude, double ellipsoidalHeight);

    /// <summary>
    /// Converts orthometric height (MSL) to ellipsoidal height (HAE) using EGM96.
    /// </summary>
    double OrthometricToEllipsoidal(double latitude, double longitude, double orthometricHeight);
}
