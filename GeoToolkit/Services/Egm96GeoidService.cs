using GeoidHeightsDotNet;
using GeoToolkit.Interfaces;

namespace GeoToolkit.Services;

/// <summary>
/// Provides EGM96 geoid undulation values and height conversions using the
/// <c>GeoidHeightsDotNet</c> library, which embeds the full EGM96 spherical
/// harmonic model (degree and order 360).
/// </summary>
public class Egm96GeoidService : IGeoidService, ISingletonService
{
    /// <summary>
    /// Returns the EGM96 geoid undulation <c>N</c> at the given WGS84 position.
    /// A positive value means the geoid surface is above the WGS84 ellipsoid at that point.
    /// </summary>
    /// <param name="latitude">WGS84 latitude in decimal degrees (-90 to +90).</param>
    /// <param name="longitude">WGS84 longitude in decimal degrees (-180 to +180).</param>
    /// <returns>Geoid undulation in meters.</returns>
    public double GetUndulation(double latitude, double longitude)
        => GeoidHeights.undulation(latitude, longitude);

    /// <summary>
    /// Converts an ellipsoidal height <c>h</c> (HAE, height above WGS84 ellipsoid)
    /// to orthometric height <c>H</c> (MSL) using the relation <c>H = h - N</c>.
    /// </summary>
    /// <param name="latitude">WGS84 latitude in decimal degrees.</param>
    /// <param name="longitude">WGS84 longitude in decimal degrees.</param>
    /// <param name="ellipsoidalHeight">Ellipsoidal height in meters.</param>
    /// <returns>Orthometric (MSL) height in meters.</returns>
    public double EllipsoidalToOrthometric(double latitude, double longitude, double ellipsoidalHeight)
        => ellipsoidalHeight - GetUndulation(latitude, longitude);

    /// <summary>
    /// Converts an orthometric height <c>H</c> (MSL) to ellipsoidal height <c>h</c> (HAE)
    /// using the relation <c>h = H + N</c>.
    /// </summary>
    /// <param name="latitude">WGS84 latitude in decimal degrees.</param>
    /// <param name="longitude">WGS84 longitude in decimal degrees.</param>
    /// <param name="orthometricHeight">Orthometric (MSL) height in meters.</param>
    /// <returns>Ellipsoidal height in meters.</returns>
    public double OrthometricToEllipsoidal(double latitude, double longitude, double orthometricHeight)
        => orthometricHeight + GetUndulation(latitude, longitude);
}
