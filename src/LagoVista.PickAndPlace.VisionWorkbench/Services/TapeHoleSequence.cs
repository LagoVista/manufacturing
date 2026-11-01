namespace LagoVista.PickAndPlace.VisionWorkbench.Services;

/// <summary>Read-only hole-row screening against independently configured nominal pitch and calibration.</summary>
public static class TapeHoleSequence
{
    public sealed record Result(IReadOnlyList<TapeGeometry.Point> Accepted, int Rejected,
        int Missing, bool Valid, string Reason, TapeGeometry.Result? Geometry);

    public static Result Evaluate(IReadOnlyList<TapeGeometry.Point> detections,
        double holePitchMm, double calibratedPixelsPerMm)
    {
        if (detections.Count < 3 || holePitchMm <= 0 || calibratedPixelsPerMm <= 0 ||
            !double.IsFinite(holePitchMm) || !double.IsFinite(calibratedPixelsPerMm))
            return new Result(Array.Empty<TapeGeometry.Point>(), detections.Count, 0, false,
                "Need at least 3 circles and positive reference pitch/calibration.", null);

        double expected = holePitchMm * calibratedPixelsPerMm;
        var points = detections.OrderBy(p => p.Y).ToArray();
        // Seed the expected pitch from every candidate pair, allowing for skipped holes.
        // Keep the largest internally consistent sequence; never silently claim calibration.
        List<TapeGeometry.Point> best = new();
        int bestMissing = int.MaxValue;
        double bestError = double.PositiveInfinity;
        foreach (var origin in points)
        foreach (var second in points)
        {
            if (second.Y <= origin.Y) continue;
            for (int step = 1; step <= 4; step++)
            {
                double dx = (second.X - origin.X) / step;
                double dy = (second.Y - origin.Y) / step;
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (Math.Abs(length - expected) > expected * 0.15 || dy <= 0) continue;
                var selected = new List<(int index, TapeGeometry.Point point, double error)>();
                foreach (var candidate in points)
                {
                    double offsetX = candidate.X - origin.X, offsetY = candidate.Y - origin.Y;
                    int index = (int)Math.Round((offsetX * dx + offsetY * dy) / (length * length));
                    if (index < 0) continue;
                    double error = Math.Sqrt(Math.Pow(offsetX - index * dx, 2) + Math.Pow(offsetY - index * dy, 2));
                    if (error <= Math.Max(3.0, expected * 0.04))
                        selected.Add((index, candidate, error));
                }
                // One circle per hole index, preferring the closest candidate.
                var row = selected.GroupBy(s => s.index).Select(g => g.MinBy(s => s.error))
                    .OrderBy(s => s.index).ToArray();
                if (row.Length < 3) continue;
                int missing = row[^1].index - row[0].index + 1 - row.Length;
                double errorTotal = row.Sum(s => s.error);
                if (row.Length > best.Count || (row.Length == best.Count &&
                    (missing < bestMissing || (missing == bestMissing && errorTotal < bestError))))
                {
                    best = row.Select(s => s.point).ToList();
                    bestMissing = missing;
                    bestError = errorTotal;
                }
            }
        }
        if (best.Count < 3)
            return new Result(Array.Empty<TapeGeometry.Point>(), detections.Count, 0, false,
                "No row matches nominal pitch and alignment tolerance.", null);

        var geometry = TapeGeometry.Analyze(best, holePitchMm);
        // Missing-hole sequences cannot use the consecutive-hole scale estimator.
        bool valid = geometry is not null && bestMissing == 0 &&
            Math.Abs(geometry.PitchPixels - expected) <= expected * 0.10 &&
            geometry.PitchVariationPixels <= expected * 0.04 &&
            geometry.MaxLineErrorPixels <= Math.Max(2, expected * 0.02);
        string reason = valid ? "Consistent consecutive hole row (screened)" :
            bestMissing > 0 ? "Possible missing holes; calibration withheld" :
            "Row found but failed pitch or straightness check";
        return new Result(best, detections.Count - best.Count, bestMissing, valid, reason,
            bestMissing == 0 ? geometry : null);
    }
}
