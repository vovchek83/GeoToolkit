using FluentAssertions;
using GeoToolkit.Models;
using GeoToolkit.Services;

namespace GeoToolkit.Tests;

public class CoordinateTransformServiceTests
{
    private readonly CoordinateTransformService _sut = new();

    [Fact]
    public void WGS84ToUtm_KyivCoordinates_ReturnsZone36()
    {
        var result = _sut.WGS84ToUtm(50.4501, 30.5234);

        result.ZoneNumber.Should().Be(36);
        result.ZoneLetter.Should().Be('U');
    }

    [Fact]
    public void WGS84ToUtm_NorthernHemisphere_NorthingIsPositive()
    {
        var result = _sut.WGS84ToUtm(50.0, 30.0);

        result.Northing.Should().BePositive();
        result.Easting.Should().BeInRange(100_000, 900_000);
    }

    [Fact]
    public void WGS84ToUtm_SouthernHemisphere_NorthingBetween5And8M()
    {
        // Sydney: ~33.87° south, false northing = 10M - M(33.87°) ≈ 6.25M
        var result = _sut.WGS84ToUtm(-33.8688, 151.2093);

        result.Northing.Should().BeInRange(5_000_000, 8_000_000);
    }

    [Fact]
    public void RoundTrip_WGS84ToUtmAndBack_ReturnsOriginalCoordinates()
    {
        double lat = 50.4501, lon = 30.5234;

        var utm = _sut.WGS84ToUtm(lat, lon);
        var (resultLat, resultLon) = _sut.UtmToWGS84(utm);

        resultLat.Should().BeApproximately(lat, 0.0001);
        resultLon.Should().BeApproximately(lon, 0.0001);
    }

    [Fact]
    public void RoundTrip_SouthernHemisphere_ReturnsOriginalCoordinates()
    {
        double lat = -33.8688, lon = 151.2093;

        var utm = _sut.WGS84ToUtm(lat, lon);
        var (resultLat, resultLon) = _sut.UtmToWGS84(utm);

        resultLat.Should().BeApproximately(lat, 0.0001);
        resultLon.Should().BeApproximately(lon, 0.0001);
    }
}
