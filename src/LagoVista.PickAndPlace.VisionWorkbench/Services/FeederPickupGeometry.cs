namespace LagoVista.PickAndPlace.VisionWorkbench.Services;

/// <summary>Offline image-coordinate feeder predictions; never commands motion.</summary>
public static class FeederPickupGeometry
{
    public sealed record Result(TapeGeometry.Point ReferenceHole, TapeGeometry.Point NextHole,
        TapeGeometry.Point Pickup, double AxisX, double AxisY);

    /// <summary>
    /// Positive along pitch goes downward in the image. Positive cross offset goes to
    /// image-left of the downward tape axis. These are image-space offsets only.
    /// </summary>
    public static Result? Predict(TapeGeometry.Result? geometry, double alongMm, double crossMm, double nominalHolePitchMm)
    {
        if (geometry is null || !double.IsFinite(alongMm) || !double.IsFinite(crossMm) ||
            !double.IsFinite(nominalHolePitchMm) || nominalHolePitchMm <= 0 ||
            !double.IsFinite(geometry.PixelsPerMm) || geometry.PixelsPerMm <= 0) return null;
        var radians = geometry.AngleFromVerticalDegrees * Math.PI / 180.0;
        var axisX = Math.Sin(radians);
        var axisY = Math.Cos(radians);
        var pxPerMm = geometry.PixelsPerMm;
        var start = geometry.Start;
        return new Result(start,
            new TapeGeometry.Point(start.X + axisX * geometry.PitchPixels, start.Y + axisY * geometry.PitchPixels),
            new TapeGeometry.Point(start.X + axisX * alongMm * pxPerMm - axisY * crossMm * pxPerMm,
                                   start.Y + axisY * alongMm * pxPerMm + axisX * crossMm * pxPerMm),
            axisX, axisY);
    }
}
