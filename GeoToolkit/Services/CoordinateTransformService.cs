using GeoToolkit.Interfaces;
using GeoToolkit.Models;

namespace GeoToolkit.Services;

/// <summary>
/// Converts coordinates between WGS84 geographic (latitude/longitude) and
/// UTM (Universal Transverse Mercator) projected systems using the standard
/// Snyder (1987) Transverse Mercator series expansion with WGS84 ellipsoid parameters.
/// Accuracy is sub-millimeter within the valid UTM zone bounds.
/// </summary>
public class CoordinateTransformService : ICoordinateTransformService, ISingletonService
{
    private const double A = 6378137.0;           // WGS84 semi-major axis (meters)
    private const double F = 1.0 / 298.257223563; // WGS84 flattening
    private const double K0 = 0.9996;             // UTM scale factor
    private const double E0 = 500000.0;           // UTM false easting (meters)

    private static readonly double E2 = 2 * F - F * F;       // first eccentricity squared
    private static readonly double E4 = E2 * E2;
    private static readonly double E6 = E4 * E2;
    private static readonly double Ep2 = E2 / (1 - E2);      // second eccentricity squared

    /// <summary>
    /// Converts a WGS84 geographic coordinate to a UTM coordinate.
    /// The UTM zone and hemisphere letter are determined automatically.
    /// Includes special zone handling for Norway (zone 32) and Svalbard (zones 31–37).
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees (-80 to +84).</param>
    /// <param name="longitude">Longitude in decimal degrees (-180 to +180).</param>
    /// <returns>The corresponding <see cref="UtmCoordinate"/>.</returns>
    public UtmCoordinate WGS84ToUtm(double latitude, double longitude)
    {
        int zone = GetZoneNumber(latitude, longitude);
        char letter = GetZoneLetter(latitude);
        double lon0Rad = ((zone - 1) * 6.0 - 180 + 3) * Math.PI / 180;

        double lat = latitude * Math.PI / 180;
        double lon = longitude * Math.PI / 180;

        double N = A / Math.Sqrt(1 - E2 * Math.Sin(lat) * Math.Sin(lat));
        double T = Math.Tan(lat) * Math.Tan(lat);
        double C = Ep2 * Math.Cos(lat) * Math.Cos(lat);
        double AL = Math.Cos(lat) * (lon - lon0Rad);

        double M = A * ((1 - E2 / 4 - 3 * E4 / 64 - 5 * E6 / 256) * lat
                      - (3 * E2 / 8 + 3 * E4 / 32 + 45 * E6 / 1024) * Math.Sin(2 * lat)
                      + (15 * E4 / 256 + 45 * E6 / 1024) * Math.Sin(4 * lat)
                      - (35 * E6 / 3072) * Math.Sin(6 * lat));

        double easting = K0 * N * (AL
            + (1 - T + C) * Math.Pow(AL, 3) / 6
            + (5 - 18 * T + T * T + 72 * C - 58 * Ep2) * Math.Pow(AL, 5) / 120)
            + E0;

        double northing = K0 * (M + N * Math.Tan(lat) * (AL * AL / 2
            + (5 - T + 9 * C + 4 * C * C) * Math.Pow(AL, 4) / 24
            + (61 - 58 * T + T * T + 600 * C - 330 * Ep2) * Math.Pow(AL, 6) / 720));

        if (latitude < 0) northing += 10_000_000.0; // southern hemisphere false northing

        return new UtmCoordinate(easting, northing, zone, letter);
    }

    /// <summary>
    /// Converts a UTM coordinate back to WGS84 geographic coordinates.
    /// The hemisphere is inferred from the zone letter ('N' or above = northern).
    /// </summary>
    /// <param name="utm">The UTM coordinate to convert.</param>
    /// <returns>A tuple of (Latitude, Longitude) in decimal degrees.</returns>
    public (double Latitude, double Longitude) UtmToWGS84(UtmCoordinate utm)
    {
        bool isNorth = utm.ZoneLetter >= 'N';
        double lon0 = (utm.ZoneNumber - 1) * 6.0 - 180 + 3;

        double x = utm.Easting - E0;
        double y = isNorth ? utm.Northing : utm.Northing - 10_000_000.0;

        double M = y / K0;
        double mu = M / (A * (1 - E2 / 4 - 3 * E4 / 64 - 5 * E6 / 256));

        double e1 = (1 - Math.Sqrt(1 - E2)) / (1 + Math.Sqrt(1 - E2));
        double phi1 = mu
            + (3 * e1 / 2 - 27 * Math.Pow(e1, 3) / 32) * Math.Sin(2 * mu)
            + (21 * e1 * e1 / 16 - 55 * Math.Pow(e1, 4) / 32) * Math.Sin(4 * mu)
            + (151 * Math.Pow(e1, 3) / 96) * Math.Sin(6 * mu)
            + (1097 * Math.Pow(e1, 4) / 512) * Math.Sin(8 * mu);

        double N1 = A / Math.Sqrt(1 - E2 * Math.Sin(phi1) * Math.Sin(phi1));
        double T1 = Math.Tan(phi1) * Math.Tan(phi1);
        double C1 = Ep2 * Math.Cos(phi1) * Math.Cos(phi1);
        double R1 = A * (1 - E2) / Math.Pow(1 - E2 * Math.Sin(phi1) * Math.Sin(phi1), 1.5);
        double D = x / (N1 * K0);

        double lat = phi1 - N1 * Math.Tan(phi1) / R1
            * (D * D / 2
            - (5 + 3 * T1 + 10 * C1 - 4 * C1 * C1 - 9 * Ep2) * Math.Pow(D, 4) / 24
            + (61 + 90 * T1 + 298 * C1 + 45 * T1 * T1 - 252 * Ep2 - 3 * C1 * C1) * Math.Pow(D, 6) / 720);

        double lon = (D
            - (1 + 2 * T1 + C1) * Math.Pow(D, 3) / 6
            + (5 - 2 * C1 + 28 * T1 - 3 * C1 * C1 + 8 * Ep2 + 24 * T1 * T1) * Math.Pow(D, 5) / 120)
            / Math.Cos(phi1);

        return (lat * 180 / Math.PI, lon0 + lon * 180 / Math.PI);
    }

    /// <summary>
    /// Determines the UTM zone number for the given coordinate.
    /// Handles the special irregular zones for Norway and Svalbard as defined by the UTM standard.
    /// </summary>
    private static int GetZoneNumber(double lat, double lon)
    {
        if (lat >= 56 && lat < 64 && lon >= 3 && lon < 12) return 32;
        if (lat >= 72 && lat < 84)
        {
            if (lon >= 0 && lon < 9) return 31;
            if (lon >= 9 && lon < 21) return 33;
            if (lon >= 21 && lon < 33) return 35;
            if (lon >= 33 && lon < 42) return 37;
        }
        return (int)Math.Floor((lon + 180) / 6) + 1;
    }

    /// <summary>
    /// Returns the UTM latitude band letter for the given latitude.
    /// Returns 'Z' for polar regions outside the valid UTM range (-80 to +84).
    /// </summary>
    private static char GetZoneLetter(double lat)
    {
        if (lat < -80 || lat > 84) return 'Z';
        return "CDEFGHJKLMNPQRSTUVWXX"[(int)Math.Floor((lat + 80) / 8)];
    }
}
