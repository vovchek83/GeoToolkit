using FluentAssertions;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoToolkit.Services;

namespace GeoToolkit.Tests;

public class SpatialQueryServiceTests
{
    private readonly SpatialQueryService _sut = new();

    private static Polygon MakeRect(double minLat, double minLon, double maxLat, double maxLon) =>
        new(new[]
        {
            new LineString(new[]
            {
                new Position(minLat, minLon),
                new Position(minLat, maxLon),
                new Position(maxLat, maxLon),
                new Position(maxLat, minLon),
                new Position(minLat, minLon),
            })
        });

    [Fact]
    public void PointInPolygon_PointInside_ReturnsTrue()
    {
        var polygon = MakeRect(50.0, 30.0, 51.0, 31.0);
        var point = new Position(50.5, 30.5);

        _sut.PointInPolygon(point, polygon).Should().BeTrue();
    }

    [Fact]
    public void PointInPolygon_PointOutside_ReturnsFalse()
    {
        var polygon = MakeRect(50.0, 30.0, 51.0, 31.0);
        var point = new Position(55.0, 35.0);

        _sut.PointInPolygon(point, polygon).Should().BeFalse();
    }

    [Fact]
    public void GetBoundingBox_MultipleFeatures_EncompassesAll()
    {
        var fc = new FeatureCollection(new List<Feature>
        {
            new(MakeRect(50.0, 30.0, 51.0, 31.0)),
            new(MakeRect(52.0, 32.0, 53.0, 33.0)),
        });

        var bbox = _sut.GetBoundingBox(fc);

        bbox.Geometry.Should().BeAssignableTo<Polygon>();
        var coords = ((Polygon)bbox.Geometry).Coordinates[0].Coordinates;
        coords.Min(c => c.Latitude).Should().BeApproximately(50.0, 0.001);
        coords.Min(c => c.Longitude).Should().BeApproximately(30.0, 0.001);
        coords.Max(c => c.Latitude).Should().BeApproximately(53.0, 0.001);
        coords.Max(c => c.Longitude).Should().BeApproximately(33.0, 0.001);
    }

    [Fact]
    public void GetCentroid_Rectangle_ReturnsCenterPoint()
    {
        var feature = new Feature(MakeRect(50.0, 30.0, 52.0, 32.0));

        var centroid = _sut.GetCentroid(feature);

        centroid.Latitude.Should().BeApproximately(51.0, 0.01);
        centroid.Longitude.Should().BeApproximately(31.0, 0.01);
    }
}
