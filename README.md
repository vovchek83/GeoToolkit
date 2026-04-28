# GeoToolkit

A .NET utility library for geospatial operations — coordinate transformations, geometric set operations on GeoJSON, elevation adjustments, distance/area measurements, and spatial queries.

## Features

| Service | Description |
|---|---|
| `ICoordinateTransformService` | WGS84 ↔ UTM conversions (Snyder 1987 formulas) |
| `IElevationAdjustmentService` | Ellipsoidal ↔ orthometric height using EGM96 |
| `IGeoidService` | EGM96 geoid undulation lookups |
| `IGeoUnionService` | Union multiple GeoJSON polygons into one |
| `IGeometryOperationsService` | Intersection, difference, buffer, simplify, validate, repair |
| `IMeasurementService` | Haversine distance, spherical area and perimeter |
| `ISpatialQueryService` | Point-in-polygon, bounding box, centroid |

## Requirements

- .NET 8.0 or later

## Installation

Add the project reference or package reference to your project:

```xml
<PackageReference Include="GeoToolkit" Version="1.0.0" />
```

## Getting Started

Register all services with a single extension method:

```csharp
using GeoToolkit;

builder.Services.AddGeoToolkit();
```

Then inject the interface you need:

```csharp
public class MyService(ICoordinateTransformService transform, IMeasurementService measure)
{
    public void Example()
    {
        var utm = transform.ToUtm(new GeoPoint(55.751244, 37.618423));
        // utm.ZoneNumber = 37, utm.ZoneLetter = 'U'

        double distanceM = measure.DistanceMeters(
            new GeoPoint(55.751244, 37.618423),
            new GeoPoint(59.934280, 30.335099));
    }
}
```

## Service Reference

### ICoordinateTransformService

```csharp
UtmCoordinate ToUtm(GeoPoint point);
GeoPoint      FromUtm(UtmCoordinate utm);
```

### IElevationAdjustmentService

```csharp
// Convert ellipsoidal height (GPS) to orthometric (MSL) height
double ToOrthometric(GeoPoint point);
// Convert orthometric height to ellipsoidal
double ToEllipsoidal(GeoPoint point);
```

### IGeoidService

```csharp
double GetUndulation(double latitude, double longitude); // EGM96 geoid height in metres
```

### IGeoUnionService

```csharp
// Returns a single GeoJSON Feature whose geometry is the union of all inputs
Feature Union(IEnumerable<Feature> polygonFeatures);
```

### IGeometryOperationsService

```csharp
Feature Intersection(Feature a, Feature b);
Feature Difference(Feature a, Feature b);
Feature Buffer(Feature feature, double distanceMeters);
Feature Simplify(Feature feature, double toleranceMeters);
bool    IsValid(Feature feature);
Feature Repair(Feature feature);

// WKT serialization
string ToWkt(Feature feature);
string ToWkt(FeatureCollection featureCollection); // returns GEOMETRYCOLLECTION (...)
string ToWkt(Polygon polygon);
```

### IMeasurementService

```csharp
double DistanceMeters(GeoPoint from, GeoPoint to);  // Haversine
double AreaSquareMeters(Feature polygon);
double PerimeterMeters(Feature polygon);
```

### ISpatialQueryService

```csharp
bool    Contains(Feature polygon, GeoPoint point);
Bbox    BoundingBox(Feature feature);              // { MinLon, MinLat, MaxLon, MaxLat }
GeoPoint Centroid(Feature feature);
```

## Helpers

### AngleExtensions

Extension methods on `double` and `float` for angle unit conversion:

```csharp
using GeoToolkit.Helpers;

double rad = 90.0.ToRadians();    // → 1.5707963...
double deg = Math.PI.ToDegrees(); // → 180.0

float rad2 = 45f.ToRadians();
float deg2 = MathF.PI.ToDegrees();
```

## Models

```csharp
record GeoPoint(double Latitude, double Longitude, double Height = 0);
record UtmCoordinate(double Easting, double Northing, int ZoneNumber, char ZoneLetter);
```

## Dependencies

- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) — geometry engine
- [GeoJSON.Net](https://github.com/GeoJSON-Net/GeoJSON.Net) — GeoJSON model
- [GeoidHeightsDotNet](https://github.com/sibristow/GeoidHeightsDotNet) — EGM96 geoid model
- [Scrutor](https://github.com/khellang/Scrutor) — DI assembly scanning

## License

MIT
