namespace GeoToolkit.Helpers;

/// <summary>Extension methods for converting angles between degrees and radians.</summary>
public static class AngleExtensions
{
    public static double ToRadians(this double degrees) => degrees * (Math.PI / 180.0);
    public static double ToDegrees(this double radians) => radians * (180.0 / Math.PI);

    public static float ToRadians(this float degrees) => degrees * (MathF.PI / 180f);
    public static float ToDegrees(this float radians) => radians * (180f / MathF.PI);
}
