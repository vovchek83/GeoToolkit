using GeoJSON.Net.Converters;
using GeoJSON.Net.Geometry;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;

namespace GeoToolkit.Helpers;

internal static class GeoJsonNtsConverter
{
    private static readonly JsonSerializer NtsSerializer = GeoJsonSerializer.Create();

    public static NtsGeometry ToNts(IGeometryObject geometry)
    {
        var json = JsonConvert.SerializeObject(geometry);
        using var reader = new JsonTextReader(new StringReader(json));
        return NtsSerializer.Deserialize<NtsGeometry>(reader)!;
    }

    public static IGeometryObject ToGeoJson(NtsGeometry geometry)
    {
        var sb = new System.Text.StringBuilder();
        using var writer = new JsonTextWriter(new StringWriter(sb));
        NtsSerializer.Serialize(writer, geometry);
        return JsonConvert.DeserializeObject<IGeometryObject>(sb.ToString(), new GeometryConverter())!;
    }
}
