using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoToolkit.Helpers;
using GeoToolkit.Interfaces;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;

namespace GeoToolkit.Services;

/// <summary>
/// Provides geometric set operations and repair utilities for GeoJSON features
/// using NetTopologySuite as the underlying engine.
/// </summary>
public class GeometryOperationsService : IGeometryOperationsService, ISingletonService
{
    /// <summary>
    /// Returns the geometric intersection of two features — the area covered by both geometries.
    /// Returns an empty geometry if the features do not overlap.
    /// </summary>
    /// <param name="a">First feature.</param>
    /// <param name="b">Second feature.</param>
    /// <returns>A new <see cref="Feature"/> whose geometry is the intersection of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public Feature Intersection(Feature a, Feature b)
    {
        var result = GeoJsonNtsConverter.ToNts(a.Geometry).Intersection(GeoJsonNtsConverter.ToNts(b.Geometry));
        return new Feature(GeoJsonNtsConverter.ToGeoJson(result));
    }

    /// <summary>
    /// Returns the geometric difference — the part of <paramref name="a"/> not covered by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">Feature to subtract from.</param>
    /// <param name="b">Feature to subtract.</param>
    /// <returns>A new <see cref="Feature"/> representing the area of <paramref name="a"/> outside <paramref name="b"/>.</returns>
    public Feature Difference(Feature a, Feature b)
    {
        var result = GeoJsonNtsConverter.ToNts(a.Geometry).Difference(GeoJsonNtsConverter.ToNts(b.Geometry));
        return new Feature(GeoJsonNtsConverter.ToGeoJson(result));
    }

    /// <summary>
    /// Expands a feature's geometry outward by the specified distance.
    /// The distance in meters is converted to degrees using the cosine approximation
    /// at the centroid latitude, which is accurate for small buffers.
    /// </summary>
    /// <param name="feature">Feature to buffer.</param>
    /// <param name="distanceInMeters">Buffer distance in meters.</param>
    /// <returns>A new <see cref="Feature"/> with the buffered geometry.</returns>
    public Feature Buffer(Feature feature, double distanceInMeters)
    {
        var nts = GeoJsonNtsConverter.ToNts(feature.Geometry);
        var latRad = nts.Centroid.Y * Math.PI / 180.0;
        var distanceDeg = distanceInMeters / (111320.0 * Math.Cos(latRad));
        return new Feature(GeoJsonNtsConverter.ToGeoJson(nts.Buffer(distanceDeg)));
    }

    /// <summary>
    /// Reduces the number of vertices in a feature's geometry while preserving its topology.
    /// Uses the Douglas-Peucker algorithm via NTS <c>TopologyPreservingSimplifier</c>.
    /// </summary>
    /// <param name="feature">Feature to simplify.</param>
    /// <param name="tolerance">Simplification tolerance in degrees. Larger values remove more vertices.</param>
    /// <returns>A new <see cref="Feature"/> with a simplified geometry.</returns>
    public Feature Simplify(Feature feature, double tolerance)
    {
        var simplified = TopologyPreservingSimplifier.Simplify(GeoJsonNtsConverter.ToNts(feature.Geometry), tolerance);
        return new Feature(GeoJsonNtsConverter.ToGeoJson(simplified));
    }

    /// <summary>
    /// Checks whether a feature's geometry is topologically valid (e.g. no self-intersections).
    /// </summary>
    /// <param name="feature">Feature to validate.</param>
    /// <returns><c>true</c> if the geometry is valid; otherwise <c>false</c>.</returns>
    public bool IsValid(Feature feature)
        => GeoJsonNtsConverter.ToNts(feature.Geometry).IsValid;

    /// <summary>
    /// Attempts to fix an invalid geometry by applying a zero-distance buffer,
    /// which is the standard NTS technique for resolving self-intersections and ring orientation issues.
    /// </summary>
    /// <param name="feature">Feature with potentially invalid geometry.</param>
    /// <returns>A new <see cref="Feature"/> with a repaired geometry.</returns>
    public Feature Repair(Feature feature)
    {
        var repaired = GeoJsonNtsConverter.ToNts(feature.Geometry).Buffer(0);
        return new Feature(GeoJsonNtsConverter.ToGeoJson(repaired));
    }

    /// <summary>
    /// Returns the Well-Known Text (WKT) representation of a feature's geometry.
    /// </summary>
    /// <param name="feature">Feature whose geometry to serialize.</param>
    /// <returns>WKT string, e.g. <c>POLYGON ((30 50, 31 50, ...))</c>.</returns>
    public string ToWkt(Feature feature)
        => GeoJsonNtsConverter.ToNts(feature.Geometry).ToText();

    /// <summary>
    /// Returns the Well-Known Text (WKT) representation of all features in a collection
    /// as a <c>GEOMETRYCOLLECTION</c>.
    /// </summary>
    /// <param name="featureCollection">Collection of features to serialize.</param>
    /// <returns>WKT string, e.g. <c>GEOMETRYCOLLECTION (POLYGON (...), POLYGON (...))</c>.</returns>
    public string ToWkt(FeatureCollection featureCollection)
    {
        var geometries = featureCollection.Features
            .Select(f => GeoJsonNtsConverter.ToNts(f.Geometry))
            .ToArray();
        return NtsGeometryFactory.Default.CreateGeometryCollection(geometries).ToText();
    }

    /// <summary>
    /// Returns the Well-Known Text (WKT) representation of a polygon.
    /// </summary>
    /// <param name="polygon">Polygon to serialize.</param>
    /// <returns>WKT string, e.g. <c>POLYGON ((30 50, 31 50, ...))</c>.</returns>
    public string ToWkt(Polygon polygon)
        => GeoJsonNtsConverter.ToNts(polygon).ToText();

    /// <summary>
    /// Parses a Well-Known Text (WKT) string into a <see cref="Feature"/>.
    /// </summary>
    /// <param name="wkt">WKT string, e.g. <c>POLYGON ((30 50, 31 50, ...))</c>.</param>
    /// <returns>A <see cref="Feature"/> wrapping the parsed geometry.</returns>
    /// <exception cref="NetTopologySuite.IO.ParseException">Thrown when <paramref name="wkt"/> is not valid WKT.</exception>
    public Feature FromWkt(string wkt)
        => new(GeoJsonNtsConverter.ToGeoJson(new WKTReader().Read(wkt)));
}
