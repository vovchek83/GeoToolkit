using GeoToolkit.Interfaces;
using GeoToolkit.Models;

namespace GeoToolkit.Services;

/// <summary>
/// Converts <see cref="GeoPoint"/> heights between ellipsoidal (HAE) and
/// orthometric (MSL) reference systems using the EGM96 geoid model via <see cref="IGeoidService"/>.
/// </summary>
public class ElevationAdjustmentService : IElevationAdjustmentService, ISingletonService
{
    private readonly IGeoidService _geoidService;

    /// <summary>
    /// Initialises the service with the geoid undulation provider.
    /// </summary>
    /// <param name="geoidService">Service that supplies EGM96 undulation values.</param>
    public ElevationAdjustmentService(IGeoidService geoidService)
    {
        _geoidService = geoidService;
    }

    /// <summary>
    /// Converts a point's height from ellipsoidal (HAE) to orthometric (MSL) by
    /// subtracting the EGM96 geoid undulation: <c>H = h - N</c>.
    /// </summary>
    /// <param name="point">Point with ellipsoidal height in the <c>Height</c> field.</param>
    /// <returns>A new <see cref="GeoPoint"/> with the orthometric height.</returns>
    public GeoPoint EllipsoidalToOrthometric(GeoPoint point) =>
        point with { Height = _geoidService.EllipsoidalToOrthometric(point.Latitude, point.Longitude, point.Height) };

    /// <summary>
    /// Converts a point's height from orthometric (MSL) to ellipsoidal (HAE) by
    /// adding the EGM96 geoid undulation: <c>h = H + N</c>.
    /// </summary>
    /// <param name="point">Point with orthometric height in the <c>Height</c> field.</param>
    /// <returns>A new <see cref="GeoPoint"/> with the ellipsoidal height.</returns>
    public GeoPoint OrthometricToEllipsoidal(GeoPoint point) =>
        point with { Height = _geoidService.OrthometricToEllipsoidal(point.Latitude, point.Longitude, point.Height) };

    /// <summary>
    /// Applies <see cref="EllipsoidalToOrthometric"/> to each point in the collection.
    /// Useful for batch-converting GPS tracks or point clouds from HAE to MSL.
    /// </summary>
    /// <param name="points">Points with ellipsoidal heights.</param>
    /// <returns>Sequence of points with orthometric heights.</returns>
    public IEnumerable<GeoPoint> ApplyGeoidCorrection(IEnumerable<GeoPoint> points) =>
        points.Select(EllipsoidalToOrthometric);
}
