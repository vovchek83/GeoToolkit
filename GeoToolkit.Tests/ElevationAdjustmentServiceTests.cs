using FluentAssertions;
using GeoToolkit.Models;
using GeoToolkit.Services;

namespace GeoToolkit.Tests;

public class ElevationAdjustmentServiceTests
{
    private readonly ElevationAdjustmentService _sut = new(new Egm96GeoidService());

    private static readonly GeoPoint Kyiv = new(50.4501, 30.5234, 200.0);

    [Fact]
    public void EllipsoidalToOrthometric_ChangesHeight()
    {
        var result = _sut.EllipsoidalToOrthometric(Kyiv);

        result.Height.Should().NotBe(Kyiv.Height);
        result.Latitude.Should().Be(Kyiv.Latitude);
        result.Longitude.Should().Be(Kyiv.Longitude);
    }

    [Fact]
    public void OrthometricToEllipsoidal_ChangesHeight()
    {
        var result = _sut.OrthometricToEllipsoidal(Kyiv);

        result.Height.Should().NotBe(Kyiv.Height);
    }

    [Fact]
    public void RoundTrip_EllipsoidalToOrthometricAndBack_ReturnsOriginal()
    {
        var orthometric = _sut.EllipsoidalToOrthometric(Kyiv);
        var result = _sut.OrthometricToEllipsoidal(orthometric);

        result.Height.Should().BeApproximately(Kyiv.Height, 1e-10);
    }

    [Fact]
    public void ApplyGeoidCorrection_BatchPoints_AdjustsAllHeights()
    {
        var points = new[]
        {
            new GeoPoint(50.4501, 30.5234, 200.0),
            new GeoPoint(48.4647, 35.0462, 150.0),
            new GeoPoint(46.4825, 30.7233, 50.0),
        };

        var result = _sut.ApplyGeoidCorrection(points).ToList();

        result.Should().HaveCount(3);
        result.Zip(points).Should().AllSatisfy(pair =>
            pair.First.Height.Should().NotBe(pair.Second.Height));
    }

    [Fact]
    public void ApplyGeoidCorrection_EmptyList_ReturnsEmpty()
    {
        var result = _sut.ApplyGeoidCorrection(Enumerable.Empty<GeoPoint>());

        result.Should().BeEmpty();
    }
}
