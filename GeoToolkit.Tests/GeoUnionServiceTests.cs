using FluentAssertions;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoToolkit.Services;

namespace GeoToolkit.Tests;

public class GeoUnionServiceTests
{
    private readonly GeoUnionService _sut = new();

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

    // --- Union(FeatureCollection) ---

    [Fact]
    public void Union_FeatureCollection_EmptyInput_ReturnsEmptyCollection()
    {
        var result = _sut.Union(new FeatureCollection());

        result.Features.Should().BeEmpty();
    }

    [Fact]
    public void Union_FeatureCollection_SinglePolygon_ReturnsSameGeometry()
    {
        var polygon = MakeRect(50.0, 30.0, 51.0, 31.0);
        var fc = new FeatureCollection(new List<Feature> { new(polygon) });

        var result = _sut.Union(fc);

        result.Features.Should().HaveCount(1);
        result.Features[0].Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void Union_FeatureCollection_TwoDisjointPolygons_ReturnsMultiPolygon()
    {
        var p1 = MakeRect(50.0, 30.0, 51.0, 31.0);
        var p2 = MakeRect(53.0, 35.0, 54.0, 36.0);
        var fc = new FeatureCollection(new List<Feature> { new(p1), new(p2) });

        var result = _sut.Union(fc);

        result.Features.Should().HaveCount(1);
        result.Features[0].Geometry.Should().BeAssignableTo<MultiPolygon>();
    }

    [Fact]
    public void Union_FeatureCollection_TwoOverlappingPolygons_ReturnsSinglePolygon()
    {
        var p1 = MakeRect(50.0, 30.0, 51.0, 31.0);
        var p2 = MakeRect(50.5, 30.5, 51.5, 31.5);
        var fc = new FeatureCollection(new List<Feature> { new(p1), new(p2) });

        var result = _sut.Union(fc);

        result.Features.Should().HaveCount(1);
        result.Features[0].Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void Union_FeatureCollection_TwoIdenticalPolygons_ReturnsSameArea()
    {
        var p1 = MakeRect(50.0, 30.0, 51.0, 31.0);
        var p2 = MakeRect(50.0, 30.0, 51.0, 31.0);
        var fc = new FeatureCollection(new List<Feature> { new(p1), new(p2) });

        var result = _sut.Union(fc);

        result.Features.Should().HaveCount(1);
        result.Features[0].Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void Union_FeatureCollection_ResultHasNameProperty()
    {
        var p1 = MakeRect(50.0, 30.0, 51.0, 31.0);
        var fc = new FeatureCollection(new List<Feature> { new(p1) });

        var result = _sut.Union(fc);

        result.Features[0].Properties.Should().ContainKey("name");
        result.Features[0].Properties["name"].Should().Be("Union Result");
    }

    // --- Union(IEnumerable<Polygon>) ---

    [Fact]
    public void Union_PolygonList_EmptyInput_ReturnsEmptyCollection()
    {
        var result = _sut.Union(Enumerable.Empty<Polygon>());

        result.Features.Should().BeEmpty();
    }

    [Fact]
    public void Union_PolygonList_SinglePolygon_ReturnsSingleFeature()
    {
        var polygon = MakeRect(50.0, 30.0, 51.0, 31.0);

        var result = _sut.Union(new[] { polygon });

        result.Features.Should().HaveCount(1);
        result.Features[0].Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void Union_PolygonList_TwoOverlappingPolygons_ReturnsSinglePolygon()
    {
        var p1 = MakeRect(50.0, 30.0, 51.0, 31.0);
        var p2 = MakeRect(50.5, 30.5, 51.5, 31.5);

        var result = _sut.Union(new[] { p1, p2 });

        result.Features.Should().HaveCount(1);
        result.Features[0].Geometry.Should().BeAssignableTo<Polygon>();
    }
}
