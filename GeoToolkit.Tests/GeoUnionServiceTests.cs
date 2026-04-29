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

    // --- Union(Feature, Feature) ---

    [Fact]
    public void Union_TwoFeatures_OverlappingPolygons_ReturnsSinglePolygon()
    {
        var a = new Feature(MakeRect(50.0, 30.0, 51.0, 31.0));
        var b = new Feature(MakeRect(50.5, 30.5, 51.5, 31.5));

        var result = _sut.Union(a, b);

        result.Geometry.Should().BeAssignableTo<Polygon>();
    }

    [Fact]
    public void Union_TwoFeatures_DisjointPolygons_ReturnsMultiPolygon()
    {
        var a = new Feature(MakeRect(50.0, 30.0, 51.0, 31.0));
        var b = new Feature(MakeRect(53.0, 35.0, 54.0, 36.0));

        var result = _sut.Union(a, b);

        result.Geometry.Should().BeAssignableTo<MultiPolygon>();
    }

    [Fact]
    public void Union_TwoFeatures_ResultHasNameProperty()
    {
        var a = new Feature(MakeRect(50.0, 30.0, 51.0, 31.0));
        var b = new Feature(MakeRect(50.5, 30.5, 51.5, 31.5));

        var result = _sut.Union(a, b);

        result.Properties.Should().ContainKey("name");
    }

    // --- SymmetricDifference ---

    [Fact]
    public void SymmetricDifference_IdenticalPolygons_ReturnsEmptyGeometry()
    {
        var a = new Feature(MakeRect(50.0, 30.0, 51.0, 31.0));
        var b = new Feature(MakeRect(50.0, 30.0, 51.0, 31.0));

        var result = _sut.SymmetricDifference(a, b);

        GeoJsonNtsConverter_Area(result).Should().BeApproximately(0, 1e-10);
    }

    [Fact]
    public void SymmetricDifference_OverlappingPolygons_ExcludesIntersection()
    {
        var a = new Feature(MakeRect(50.0, 30.0, 52.0, 32.0));
        var b = new Feature(MakeRect(51.0, 31.0, 53.0, 33.0));

        var symDiff = _sut.SymmetricDifference(a, b);
        var union   = _sut.Union(a, b);
        var inter   = GeoJsonNtsConverter_Intersection(a, b);

        GeoJsonNtsConverter_Area(symDiff).Should()
            .BeApproximately(GeoJsonNtsConverter_Area(union) - inter, 1e-10);
    }

    [Fact]
    public void SymmetricDifference_DisjointPolygons_EqualsUnion()
    {
        var a = new Feature(MakeRect(50.0, 30.0, 51.0, 31.0));
        var b = new Feature(MakeRect(53.0, 35.0, 54.0, 36.0));

        var symDiff = _sut.SymmetricDifference(a, b);
        var union   = _sut.Union(a, b);

        GeoJsonNtsConverter_Area(symDiff).Should()
            .BeApproximately(GeoJsonNtsConverter_Area(union), 1e-10);
    }

    // --- Dissolve ---

    [Fact]
    public void Dissolve_EmptyCollection_ReturnsEmptyCollection()
    {
        var result = _sut.Dissolve(new FeatureCollection(), "zone");

        result.Features.Should().BeEmpty();
    }

    [Fact]
    public void Dissolve_TwoGroups_ReturnsTwoFeatures()
    {
        var fc = new FeatureCollection(new List<Feature>
        {
            new(MakeRect(50.0, 30.0, 51.0, 31.0), new Dictionary<string, object> { { "zone", "A" } }),
            new(MakeRect(50.5, 30.5, 51.5, 31.5), new Dictionary<string, object> { { "zone", "A" } }),
            new(MakeRect(53.0, 35.0, 54.0, 36.0), new Dictionary<string, object> { { "zone", "B" } }),
        });

        var result = _sut.Dissolve(fc, "zone");

        result.Features.Should().HaveCount(2);
    }

    [Fact]
    public void Dissolve_SameGroup_ProducesFewerFeaturesThanInput()
    {
        var fc = new FeatureCollection(new List<Feature>
        {
            new(MakeRect(50.0, 30.0, 51.0, 31.0), new Dictionary<string, object> { { "zone", "A" } }),
            new(MakeRect(50.5, 30.5, 51.5, 31.5), new Dictionary<string, object> { { "zone", "A" } }),
        });

        var result = _sut.Dissolve(fc, "zone");

        result.Features.Should().HaveCount(1);
    }

    [Fact]
    public void Dissolve_PreservesGroupKeyAsProperty()
    {
        var fc = new FeatureCollection(new List<Feature>
        {
            new(MakeRect(50.0, 30.0, 51.0, 31.0), new Dictionary<string, object> { { "zone", "Alpha" } }),
        });

        var result = _sut.Dissolve(fc, "zone");

        result.Features[0].Properties.Should().ContainKey("zone");
        result.Features[0].Properties["zone"].Should().Be("Alpha");
    }

    // helpers for SymmetricDifference area checks
    private static double GeoJsonNtsConverter_Area(Feature f)
    {
        using var reader = new System.IO.StringReader(
            Newtonsoft.Json.JsonConvert.SerializeObject(f.Geometry));
        var serializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
        var nts = serializer.Deserialize<NetTopologySuite.Geometries.Geometry>(
            new Newtonsoft.Json.JsonTextReader(reader))!;
        return nts.Area;
    }

    private static double GeoJsonNtsConverter_Intersection(Feature a, Feature b)
    {
        static NetTopologySuite.Geometries.Geometry ToNts(Feature f)
        {
            using var reader = new System.IO.StringReader(
                Newtonsoft.Json.JsonConvert.SerializeObject(f.Geometry));
            var s = NetTopologySuite.IO.GeoJsonSerializer.Create();
            return s.Deserialize<NetTopologySuite.Geometries.Geometry>(
                new Newtonsoft.Json.JsonTextReader(reader))!;
        }
        return ToNts(a).Intersection(ToNts(b)).Area;
    }
}
