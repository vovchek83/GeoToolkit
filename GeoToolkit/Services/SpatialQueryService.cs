using GeoJSON.Net.Feature;
using GeoToolkit.Helpers;
using GeoToolkit.Interfaces;
using NetTopologySuite.Geometries;
using GeoJsonPolygon = GeoJSON.Net.Geometry.Polygon;
using GeoJsonPosition = GeoJSON.Net.Geometry.Position;
using NtsPoint = NetTopologySuite.Geometries.Point;

namespace GeoToolkit.Services;

/// <summary>
/// Provides spatial query operations such as point-in-polygon tests,
/// bounding box calculation, and centroid extraction.
/// </summary>
public class SpatialQueryService : ISpatialQueryService, ISingletonService
{
    /// <summary>
    /// Determines whether a geographic point lies inside a polygon.
    /// Points on the boundary are not considered inside (uses NTS <c>Contains</c> semantics).
    /// </summary>
    /// <param name="point">The point to test, in WGS84 coordinates.</param>
    /// <param name="polygon">The polygon to test against.</param>
    /// <returns><c>true</c> if the point is strictly inside the polygon; otherwise <c>false</c>.</returns>
    public bool PointInPolygon(GeoJsonPosition point, GeoJsonPolygon polygon)
    {
        var ntsPolygon = GeoJsonNtsConverter.ToNts(polygon);
        var ntsPoint = new NtsPoint(new Coordinate(point.Longitude, point.Latitude));
        return ntsPolygon.Contains(ntsPoint);
    }

    /// <summary>
    /// Computes the minimum bounding rectangle that encompasses all features in the collection.
    /// </summary>
    /// <param name="featureCollection">Collection of features to enclose.</param>
    /// <returns>
    /// A <see cref="Feature"/> whose geometry is a rectangular <c>Polygon</c>
    /// representing the bounding box in WGS84 coordinates.
    /// </returns>
    public Feature GetBoundingBox(FeatureCollection featureCollection)
    {
        var envelope = new Envelope();
        foreach (var f in featureCollection.Features)
            envelope.ExpandToInclude(GeoJsonNtsConverter.ToNts(f.Geometry).EnvelopeInternal);

        var bbox = new Polygon(new LinearRing(new[]
        {
            new Coordinate(envelope.MinX, envelope.MinY),
            new Coordinate(envelope.MaxX, envelope.MinY),
            new Coordinate(envelope.MaxX, envelope.MaxY),
            new Coordinate(envelope.MinX, envelope.MaxY),
            new Coordinate(envelope.MinX, envelope.MinY),
        }));

        return new Feature(GeoJsonNtsConverter.ToGeoJson(bbox));
    }

    /// <summary>
    /// Computes the geometric centroid of a feature's geometry.
    /// For convex polygons this equals the center of mass; for complex geometries
    /// the centroid may fall outside the geometry itself.
    /// </summary>
    /// <param name="feature">Feature whose centroid is required.</param>
    /// <returns>The centroid as a WGS84 <see cref="GeoJsonPosition"/>.</returns>
    public GeoJsonPosition GetCentroid(Feature feature)
    {
        var centroid = GeoJsonNtsConverter.ToNts(feature.Geometry).Centroid;
        return new GeoJsonPosition(centroid.Y, centroid.X);
    }
}
