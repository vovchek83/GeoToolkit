using GeoToolkit.Models;

namespace GeoToolkit.Interfaces;

public interface ICoordinateTransformService
{
    UtmCoordinate WGS84ToUtm(double latitude, double longitude);
    (double Latitude, double Longitude) UtmToWGS84(UtmCoordinate utm);
}
