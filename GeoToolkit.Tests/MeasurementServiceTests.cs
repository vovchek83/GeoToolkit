using FluentAssertions;
using GeoJSON.Net.Geometry;
using GeoToolkit.Services;

namespace GeoToolkit.Tests;

public class MeasurementServiceTests
{
    private readonly MeasurementService _sut = new();

    [Fact]
    public void CalculateDistance_SamePoint_ReturnsZero()
    {
        var p = new Position(50.0, 30.0);

        _sut.CalculateDistance(p, p).Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void CalculateDistance_KnownPoints_ReturnsCorrectMeters()
    {
        // Kyiv to Kharkiv ~410 km
        var kyiv = new Position(50.4501, 30.5234);
        var kharkiv = new Position(49.9935, 36.2304);

        var distance = _sut.CalculateDistance(kyiv, kharkiv);

        distance.Should().BeInRange(400_000, 420_000);
    }

    [Fact]
    public void CalculatePerimeter_UnitSquare_ReturnsExpectedValue()
    {
        // 1° lat ≈ 111 320 m, 1° lon at 50° ≈ 71 700 m → perimeter ≈ 2*(111320+71700) ≈ 366 040 m
        var polygon = new Polygon(new[]
        {
            new LineString(new[]
            {
                new Position(50.0, 30.0),
                new Position(51.0, 30.0),
                new Position(51.0, 31.0),
                new Position(50.0, 31.0),
                new Position(50.0, 30.0),
            })
        });

        var perimeter = _sut.CalculatePerimeter(polygon);

        perimeter.Should().BeInRange(340_000, 400_000);
    }

    [Fact]
    public void CalculateArea_LargerPolygon_ReturnsLargerArea()
    {
        var small = new Polygon(new[]
        {
            new LineString(new[]
            {
                new Position(50.0, 30.0),
                new Position(50.5, 30.0),
                new Position(50.5, 30.5),
                new Position(50.0, 30.5),
                new Position(50.0, 30.0),
            })
        });

        var large = new Polygon(new[]
        {
            new LineString(new[]
            {
                new Position(50.0, 30.0),
                new Position(51.0, 30.0),
                new Position(51.0, 31.0),
                new Position(50.0, 31.0),
                new Position(50.0, 30.0),
            })
        });

        _sut.CalculateArea(large).Should().BeGreaterThan(_sut.CalculateArea(small));
    }

    [Fact]
    public void CalculateArea_ReturnsPositiveValue()
    {
        var polygon = new Polygon(new[]
        {
            new LineString(new[]
            {
                new Position(50.0, 30.0),
                new Position(51.0, 30.0),
                new Position(51.0, 31.0),
                new Position(50.0, 31.0),
                new Position(50.0, 30.0),
            })
        });

        _sut.CalculateArea(polygon).Should().BePositive();
    }
}
