using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;

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
        // The shared calculator operates in feeder coordinates (X=along tape,
        // Y=across tape). Map its output back to the detected image-space axis.
        // This deliberately exposes legacy interpolation behavior for inspection.
        var radians = geometry.AngleFromVerticalDegrees * Math.PI / 180.0;
        var ux = Math.Sin(radians);
        var uy = Math.Cos(radians);
        var pxPerMm = geometry.PixelsPerMm;
        var spanMm = geometry.PitchPixels * (geometry.Count - 1) / pxPerMm;
        if (!double.IsFinite(spanMm) || spanMm <= 0) return null;

        var local = StripFeederPickupCalculator.Calculate(
            new Point2D<double>(0, 0),
            new Point2D<double>(0, 0),
            new Point2D<double>(spanMm, 0),
            isVertical: false, holesOnTop: false, feedForward: true,
            partIndex: 1, tapePitchMm: nominalHolePitchMm, tapeWidthMm: 8,
            packageXOffsetMm: alongMm, packageYOffsetMm: crossMm,
            holesOnBothSides: false);

        // Local +Y is to image-left of the fitted downward tape axis.
        TapeGeometry.Point ToImage(double along, double across) =>
            new(geometry.Start.X + (ux * along - uy * across) * pxPerMm,
                geometry.Start.Y + (uy * along + ux * across) * pxPerMm);
        return new Result(geometry.Start,
            new TapeGeometry.Point(geometry.Start.X + ux * geometry.PitchPixels,
                geometry.Start.Y + uy * geometry.PitchPixels),
            ToImage(local.X, local.Y), ux, uy);
    }
}
