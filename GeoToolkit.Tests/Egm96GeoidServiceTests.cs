using FluentAssertions;
using GeoToolkit.Services;

namespace GeoToolkit.Tests;

public class Egm96GeoidServiceTests
{
    private readonly Egm96GeoidService _sut = new();

    // Reference value from NGS EGM96 online calculator: ~-31.63 m at (38.6281, 269.7791)
    private const double RefLat = 38.6281550;
    private const double RefLon = 269.7791550;
    private const double RefUndulation = -31.63;
    private const double Tolerance = 0.5;

    [Fact]
    public void GetUndulation_KnownPoint_ReturnsExpectedValue()
    {
        var result = _sut.GetUndulation(RefLat, RefLon);

        result.Should().BeApproximately(RefUndulation, Tolerance);
    }

    [Fact]
    public void GetUndulation_ReturnsDouble()
    {
        var result = _sut.GetUndulation(50.4501, 30.5234);

        result.Should().NotBe(double.NaN);
        result.Should().BeInRange(-120, 90);
    }

    [Fact]
    public void EllipsoidalToOrthometric_EqualsHaeMinusUndulation()
    {
        double lat = 50.4501, lon = 30.5234, hae = 200.0;

        var undulation = _sut.GetUndulation(lat, lon);
        var result = _sut.EllipsoidalToOrthometric(lat, lon, hae);

        result.Should().BeApproximately(hae - undulation, 1e-10);
    }

    [Fact]
    public void OrthometricToEllipsoidal_EqualsOrthometricPlusUndulation()
    {
        double lat = 50.4501, lon = 30.5234, msl = 200.0;

        var undulation = _sut.GetUndulation(lat, lon);
        var result = _sut.OrthometricToEllipsoidal(lat, lon, msl);

        result.Should().BeApproximately(msl + undulation, 1e-10);
    }

    [Fact]
    public void RoundTrip_EllipsoidalToOrthometricAndBack_ReturnsOriginalHeight()
    {
        double lat = 50.4501, lon = 30.5234, hae = 350.0;

        var msl = _sut.EllipsoidalToOrthometric(lat, lon, hae);
        var result = _sut.OrthometricToEllipsoidal(lat, lon, msl);

        result.Should().BeApproximately(hae, 1e-10);
    }
}
