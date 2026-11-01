namespace LagoVista.PickAndPlace.VisionWorkbench.Services;

/// <summary>Pure geometry; no OpenCV, WPF or machine dependencies. Coordinates in image pixels.</summary>
public static class TapeGeometry
{
    public sealed record Point(double X, double Y);
    public sealed record Result(int Count, double PitchPixels, double PixelsPerMm,
        double AngleFromVerticalDegrees, double PitchVariationPixels,
        double MaxLineErrorPixels, Point Start, Point End);

    /// <summary>
    /// Fits a line through ordered hole centers. Assumes the supplied detections are
    /// consecutive holes; callers must verify the observed sequence before calibration.
    /// </summary>
    public static Result? Analyze(IReadOnlyList<Point> holes, double holePitchMm)
    {
        if (holes.Count < 2 || !double.IsFinite(holePitchMm) || holePitchMm <= 0) return null;
        var ordered = holes.OrderBy(p => p.Y).ThenBy(p => p.X).ToArray();
        // Principal-axis fit avoids bias from using only endpoints.
        double cx = ordered.Average(p => p.X), cy = ordered.Average(p => p.Y);
        double xx = 0, xy = 0, yy = 0;
        foreach (var p in ordered)
        {
            double dx = p.X - cx, dy = p.Y - cy;
            xx += dx * dx; xy += dx * dy; yy += dy * dy;
        }
        double theta = 0.5 * Math.Atan2(2 * xy, yy - xx);
        double ux = Math.Sin(theta), uy = Math.Cos(theta);
        var projections = ordered.Select(p => (p.X - cx) * ux + (p.Y - cy) * uy).OrderBy(t => t).ToArray();
        var gaps = Enumerable.Range(1, projections.Length - 1)
            .Select(i => projections[i] - projections[i - 1]).ToArray();
        double pitch = gaps.Average();
        if (pitch <= 0 || !double.IsFinite(pitch)) return null;
        double variation = gaps.Length < 2 ? 0 : Math.Sqrt(gaps.Average(g => Math.Pow(g - pitch, 2)));
        double lineError = ordered.Max(p => Math.Abs((p.X - cx) * uy - (p.Y - cy) * ux));
        return new Result(holes.Count, pitch, pitch / holePitchMm, theta * 180.0 / Math.PI,
            variation, lineError,
            new Point(cx + projections[0] * ux, cy + projections[0] * uy),
            new Point(cx + projections[^1] * ux, cy + projections[^1] * uy));
    }
}
