using FluentAssertions;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoToolkit.Services;

namespace GeoToolkit.Tests;

public class GeometryOperationsServiceTests
{
    private readonly GeometryOperationsService _sut = new();

    private static Feature MakeRectFeature(double minLat, double minLon, double maxLat, double maxLon) =>
        new(new Polygon(new[]
        {
            new LineString(new[]
            {
                new Position(minLat, minLon),
                new Position(minLat, maxLon),
                new Position(maxLat, maxLon),
                new Position(maxLat, minLon),
                new Position(minLat, minLon),
            })
        }));

    [Fact]
    public void Intersection_OverlappingPolygons_ReturnsOverlapArea()
    {
        var a = MakeRectFeature(50.0, 30.0, 51.0, 31.0);
        var b = MakeRectFeature(50.5, 30.5, 51.5, 31.5);

        var result = _sut.Intersection(a, b);

        result.Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void Intersection_DisjointPolygons_ReturnsEmpty()
    {
        var a = MakeRectFeature(50.0, 30.0, 51.0, 31.0);
        var b = MakeRectFeature(55.0, 35.0, 56.0, 36.0);

        var result = _sut.Intersection(a, b);

        result.Geometry.Type.Should().Be(GeoJSON.Net.GeoJSONObjectType.Polygon);
    }

    [Fact]
    public void Difference_OverlappingPolygons_ReturnsRemainder()
    {
        var a = MakeRectFeature(50.0, 30.0, 52.0, 32.0);
        var b = MakeRectFeature(51.0, 31.0, 53.0, 33.0);

        var result = _sut.Difference(a, b);

        result.Should().NotBeNull();
        result.Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void Buffer_Polygon_ExpandsGeometry()
    {
        var feature = MakeRectFeature(50.0, 30.0, 51.0, 31.0);

        var result = _sut.Buffer(feature, 1000);

        result.Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void Simplify_Polygon_ReturnsGeometry()
    {
        var feature = MakeRectFeature(50.0, 30.0, 51.0, 31.0);

        var result = _sut.Simplify(feature, 0.001);

        result.Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void IsValid_ValidPolygon_ReturnsTrue()
    {
        var feature = MakeRectFeature(50.0, 30.0, 51.0, 31.0);

        _sut.IsValid(feature).Should().BeTrue();
    }

    [Fact]
    public void Repair_ValidPolygon_ReturnsSamePolygon()
    {
        var feature = MakeRectFeature(50.0, 30.0, 51.0, 31.0);

        var result = _sut.Repair(feature);

        result.Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void ToWkt_Feature_ReturnsPolygonWkt()
    {
        var feature = MakeRectFeature(50.0, 30.0, 51.0, 31.0);

        var wkt = _sut.ToWkt(feature);

        wkt.Should().StartWith("POLYGON");
        wkt.Should().Contain("30");
        wkt.Should().Contain("50");
    }

    [Fact]
    public void ToWkt_Polygon_ReturnsPolygonWkt()
    {
        var polygon = (Polygon)MakeRectFeature(50.0, 30.0, 51.0, 31.0).Geometry;

        var wkt = _sut.ToWkt(polygon);

        wkt.Should().StartWith("POLYGON");
        wkt.Should().Contain("30");
        wkt.Should().Contain("50");
    }

    [Fact]
    public void ToWkt_FeatureCollection_ReturnsGeometryCollectionWkt()
    {
        var fc = new FeatureCollection();
        fc.Features.Add(MakeRectFeature(50.0, 30.0, 51.0, 31.0));
        fc.Features.Add(MakeRectFeature(52.0, 32.0, 53.0, 33.0));

        var wkt = _sut.ToWkt(fc);

        wkt.Should().StartWith("GEOMETRYCOLLECTION");
        wkt.Should().Contain("POLYGON");
    }

    [Fact]
    public void ToWkt_EmptyFeatureCollection_ReturnsEmptyGeometryCollectionWkt()
    {
        var fc = new FeatureCollection();

        var wkt = _sut.ToWkt(fc);

        wkt.Should().StartWith("GEOMETRYCOLLECTION");
        wkt.Should().Contain("EMPTY");
    }
}
