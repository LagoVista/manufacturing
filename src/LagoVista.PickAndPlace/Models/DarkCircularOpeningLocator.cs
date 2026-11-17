using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Deterministic, profile-independent optical locator for a dark circular
    /// opening surrounded by a brighter annulus. No hardware calls, motion,
    /// Hough settings, or mutable global state. Operates on raw gray pixels.
    /// </summary>
    public static class DarkCircularOpeningLocator
    {
        public sealed class Observation
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double DiameterMm { get; set; }
            public double RadiusPixels { get; set; }
            public double Contrast { get; set; }
            public double Circularity { get; set; }
            public double Score { get; set; }
        }

        /// <summary>
        /// Per-frame rejection evidence. The detector still returns null for
        /// ambiguous/unproven objects; diagnostics never relax acceptance.
        /// </summary>
        public sealed class RejectionDiagnostics
        {
            public int ThresholdCount { get; internal set; }
            public int TouchingRoiEdge { get; internal set; }
            public int RejectedTooSmall { get; internal set; }
            public int RejectedTooLarge { get; internal set; }
            public int RejectedLockedDiameter { get; internal set; }
            public int RejectedShape { get; internal set; }
            public int RejectedOutsideSearch { get; internal set; }
            public int RejectedContrast { get; internal set; }
            public int RejectedAnnulus { get; internal set; }
            public bool Ambiguous { get; internal set; }
            public double LargestOversizeDiameterMm { get; internal set; }

            public string PrimaryReason
            {
                get
                {
                    if (RejectedTooLarge > 0)
                        return $"OVERSIZE {LargestOversizeDiameterMm:F2} MM (> {MaximumOpeningDiameterMm:F2})";
                    if (RejectedLockedDiameter > 0)
                        return "LOCKED DIAMETER MISMATCH";
                    if (RejectedAnnulus > 0)
                        return "INSUFFICIENT BRIGHT RING";
                    if (RejectedContrast > 0)
                        return "LOW RING CONTRAST";
                    if (RejectedShape > 0)
                        return "NONCIRCULAR DARK REGION";
                    if (RejectedOutsideSearch > 0)
                        return "OUTSIDE SEARCH RADIUS";
                    if (TouchingRoiEdge > 0)
                        return "DARK REGION TOUCHES ROI EDGE";
                    if (RejectedTooSmall > 0)
                        return "DARK REGION TOO SMALL";
                    if (ThresholdCount == 0)
                        return "NO USABLE GRAYSCALE THRESHOLD";
                    return Ambiguous ? "MULTIPLE SIMILAR CANDIDATES" :
                        "NO VERIFIED DARK OPENING";
                }
            }

            public string ShortSummary =>
                $"EDGE {TouchingRoiEdge} SIZE {RejectedTooLarge} " +
                $"SHAPE {RejectedShape} RING {RejectedAnnulus} " +
                $"CONTRAST {RejectedContrast}" +
                (Ambiguous ? " AMBIGUOUS" : String.Empty);
        }

        public sealed class TuneResult
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double DiameterMm { get; set; }
            public double MedianContrast { get; set; }
            public double MaxCenterDeviationMm { get; set; }
            public int GoodFrames { get; set; }
            public int TotalFrames { get; set; }
        }

        // Coarse acquisition requires enough image pixels to locate a nozzle
        // before optical centering. A physical 2 mm search radius would shrink
        // fourfold after a 50 -> 12.5 px/mm calibration, even though the same
        // opening is at the same pixel location in the camera image.
        // Only adaptive nozzle acquisition uses this floor; fiducial/feeder
        // locators retain their existing physical search area.
        public const int MinimumAcquisitionRadiusPixels = 72;

        // Hardware diagnostics on Head 2 showed a plausible 3.52 mm dark
        // opening at the calibrated 12.5 px/mm. The earlier 1.70 mm cap
        // discarded it before circularity or annular-support checks could run.
        // This 4 mm bound is a supervised acquisition ceiling, not proof that
        // the largest dark component is the nozzle. The surrounding-ring,
        // ambiguity and multi-frame checks must still validate the target.
        // Future: use the installed nozzle catalog's InnerDiameter rather
        // than a global acquisition ceiling.
        public const double MinimumOpeningDiameterMm = 0.10;
        public const double MaximumOpeningDiameterMm = 4.00;

        public static double EffectiveAcquisitionRadiusPixels(int width, int height,
            double pixelsPerMm, double searchRadiusMm)
        {
            if (width < 32 || height < 32 || !Finite(pixelsPerMm) ||
                pixelsPerMm <= 0 || !Finite(searchRadiusMm) || searchRadiusMm <= 0)
                return 0;

            var maxRadius = Math.Min(width, height) / 2.0 - 2;
            return Math.Min(maxRadius, Math.Max(MinimumAcquisitionRadiusPixels,
                pixelsPerMm * searchRadiusMm));
        }

        /// <summary>Find a single unambiguous, circular dark core in raw grayscale.
        /// expectedDiameterMm = 0 discovers the opening; a positive value
        /// locks subsequent tracking to the verified physical feature.</summary>
        public static Observation Find(byte[,,] gray, int width, int height,
            double pixelsPerMm, double searchRadiusMm, double expectedDiameterMm = 0,
            RejectionDiagnostics diagnostics = null)
        {
            if (gray == null || width < 32 || height < 32 ||
                gray.GetLength(0) < height || gray.GetLength(1) < width ||
                gray.GetLength(2) < 1 || !Finite(pixelsPerMm) || pixelsPerMm <= 0 ||
                !Finite(searchRadiusMm) || searchRadiusMm <= 0 ||
                !Finite(expectedDiameterMm) || expectedDiameterMm < 0)
                return null;

            var cx = width / 2.0;
            var cy = height / 2.0;
            var searchPx = EffectiveAcquisitionRadiusPixels(
                width, height, pixelsPerMm, searchRadiusMm);
            if (searchPx < 8) return null;
            var left = Math.Max(1, (int)Math.Floor(cx - searchPx));
            var right = Math.Min(width - 2, (int)Math.Ceiling(cx + searchPx));
            var top = Math.Max(1, (int)Math.Floor(cy - searchPx));
            var bottom = Math.Min(height - 2, (int)Math.Ceiling(cy + searchPx));
            var sw = right - left + 1;
            var sh = bottom - top + 1;

            var histogram = new int[256];
            var total = 0;
            for (var y = top; y <= bottom; y++)
                for (var x = left; x <= right; x++)
                    if (DistanceSquared(x, y, cx, cy) <= searchPx * searchPx)
                    {
                        histogram[gray[y, x, 0]]++;
                        total++;
                    }
            if (total < 50) return null;
            var high = Percentile(histogram, total, 0.80);
            var darkest = Percentile(histogram, total, 0.001);
            if (high - darkest < 25)
            {
                // A reflective nozzle ring can occupy less than 20% of the
                // acquisition window, especially against a nearly black field.
                // The 80th percentile then describes only the background and
                // prevents ALL candidate thresholds from being generated.
                high = Percentile(histogram, total, 0.98);
                if (high - darkest < 25)
                    high = Percentile(histogram, total, 0.995);
            }

            // At 12.5 px/mm, a 0.6 mm nozzle core occupies roughly 40
            // pixels out of a 72-pixel-radius (~16,000 pixel) search area.
            // The old 1st-percentile dark reference skipped this opening
            // entirely whenever the surrounding field was mostly flat.
            // Use several deterministic lower-tail references instead of
            // allowing the whole search area's background to drown out the
            // small dark core. Candidate geometry and bright-annulus contrast
            // still gate acceptance below.
            var thresholds = new SortedSet<int>();
            foreach (var percentile in new[] { 0.001, 0.003, 0.006, 0.01 })
            {
                var low = Percentile(histogram, total, percentile);
                if (high - low < 25) continue;
                foreach (var fraction in new[] { 0.04, 0.08, 0.13, 0.23, 0.33, 0.43, 0.53, 0.63 })
                    thresholds.Add(Math.Max(0, Math.Min(254,
                        (int)Math.Round(low + (high - low) * fraction))));
            }
            if (diagnostics != null) diagnostics.ThresholdCount = thresholds.Count;
            if (thresholds.Count == 0) return null;

            var candidates = new List<Observation>();
            var queue = new int[sw * sh];
            foreach (var threshold in thresholds)
            {
                var visited = new bool[sw * sh];
                for (var ry = 0; ry < sh; ry++)
                for (var rx = 0; rx < sw; rx++)
                {
                    var index = ry * sw + rx;
                    if (visited[index] || gray[top + ry, left + rx, 0] > threshold)
                        continue;

                    var read = 0;
                    var write = 1;
                    queue[0] = index;
                    visited[index] = true;
                    var area = 0;
                    var perimeter = 0;
                    var touchesEdge = false;
                    double sx = 0, sy = 0, darkSum = 0;
                    var minX = int.MaxValue; var maxX = 0;
                    var minY = int.MaxValue; var maxY = 0;

                    while (read < write)
                    {
                        var cell = queue[read++];
                        var lx = cell % sw;
                        var ly = cell / sw;
                        var x = left + lx;
                        var y = top + ly;
                        area++;
                        sx += x; sy += y; darkSum += gray[y, x, 0];
                        minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                        if (lx == 0 || ly == 0 || lx == sw - 1 || ly == sh - 1)
                            touchesEdge = true;
                        // Four-neighbour connectivity and perimeter. In-row
                        // neighbours cannot wrap into the next row.
                        for (var side = 0; side < 4; side++)
                        {
                            var nx = lx + (side == 0 ? -1 : side == 1 ? 1 : 0);
                            var ny = ly + (side == 2 ? -1 : side == 3 ? 1 : 0);
                            if (nx < 0 || ny < 0 || nx >= sw || ny >= sh ||
                                gray[top + ny, left + nx, 0] > threshold)
                            {
                                perimeter++;
                                continue;
                            }
                            var neighbour = ny * sw + nx;
                            if (!visited[neighbour])
                            {
                                visited[neighbour] = true;
                                queue[write++] = neighbour;
                            }
                        }
                    }

                    if (touchesEdge)
                    {
                        if (diagnostics != null) diagnostics.TouchingRoiEdge++;
                        continue;
                    }
                    if (area < 12 || perimeter == 0) continue;
                    var radius = Math.Sqrt(area / Math.PI);
                    var diameterMm = 2 * radius / pixelsPerMm;
                    if (diameterMm < MinimumOpeningDiameterMm)
                    {
                        if (diagnostics != null) diagnostics.RejectedTooSmall++;
                        continue;
                    }
                    // Do not count an arbitrary large dark connected region as
                    // an oversize nozzle before proving its geometry/annulus.
                    // That produced misleading OVERSIZE diagnostics for the
                    // large dark background around Head 2.
                    if (expectedDiameterMm > 0 &&
                        Math.Abs(diameterMm - expectedDiameterMm) >
                        Math.Max(0.12, expectedDiameterMm * 0.30))
                    {
                        if (diagnostics != null) diagnostics.RejectedLockedDiameter++;
                        continue;
                    }
                    var aspect = (maxX - minX + 1.0) / (maxY - minY + 1.0);
                    if (aspect < 0.72 || aspect > 1.38)
                    {
                        if (diagnostics != null) diagnostics.RejectedShape++;
                        continue;
                    }
                    var circularity = Math.Min(1.0, 4.0 * Math.PI * area /
                        (perimeter * perimeter));
                    if (circularity < 0.42)
                    {
                        if (diagnostics != null) diagnostics.RejectedShape++;
                        continue;
                    }
                    var centerX = sx / area;
                    var centerY = sy / area;
                    var offset = Math.Sqrt(DistanceSquared(centerX, centerY, cx, cy));
                    if (offset + radius > searchPx)
                    {
                        if (diagnostics != null) diagnostics.RejectedOutsideSearch++;
                        continue;
                    }

                    // Require a dark-to-bright transition around MOST of the
                    // opening, not just a high mean from one specular hotspot.
                    // The wide metal nozzle can have a missing/washed-out arc,
                    // so allow sectors to fail while requiring spatial coverage.
                    // This is still the same central dark opening, not its
                    // larger exterior concentric metal rings.
                    double surrounding = 0;
                    var ringCount = 0;
                    // Hold radial measurements until the candidate's global
                    // contrast is known. Relative support rejects background-
                    // only sectors that can look bright beside a tiny dark speck.
                    var radialContrast = new double[24];
                    var radialMeasured = new bool[24];
                    for (var a = 0; a < 24; a++)
                    {
                        var theta = 2 * Math.PI * a / 24.0;
                        var cos = Math.Cos(theta);
                        var sin = Math.Sin(theta);
                        var innerX = (int)Math.Round(centerX + cos * radius * 0.60);
                        var innerY = (int)Math.Round(centerY + sin * radius * 0.60);
                        double localRing = 0;
                        var localCount = 0;
                        foreach (var ringScale in new[] { 1.45, 1.85 })
                        {
                            var x = (int)Math.Round(centerX + cos * radius * ringScale);
                            var y = (int)Math.Round(centerY + sin * radius * ringScale);
                            if (x >= 0 && x < width && y >= 0 && y < height)
                            {
                                localRing += gray[y, x, 0];
                                localCount++;
                            }
                        }
                        surrounding += localRing;
                        ringCount += localCount;
                        if (localCount == 2 && innerX >= 0 && innerX < width &&
                            innerY >= 0 && innerY < height)
                        {
                            radialContrast[a] = localRing / localCount -
                                gray[innerY, innerX, 0];
                            radialMeasured[a] = true;
                        }
                    }
                    if (ringCount < 36)
                    {
                        if (diagnostics != null) diagnostics.RejectedAnnulus++;
                        continue;
                    }
                    var contrast = surrounding / ringCount - darkSum / area;
                    if (contrast < 35)
                    {
                        if (diagnostics != null) diagnostics.RejectedContrast++;
                        continue;
                    }
                    // A dark speck on a moderately bright background otherwise
                    // gets 24/24 support without having an annulus at all.
                    // Require sectors to be meaningfully stronger than that
                    // candidate's own average dark-to-light transition.
                    var supportThreshold = Math.Max(24, contrast * 0.35);
                    var supportedAngles = 0;
                    var supportedQuadrants = new bool[4];
                    for (var a = 0; a < 24; a++)
                    {
                        if (!radialMeasured[a] || radialContrast[a] < supportThreshold)
                            continue;
                        supportedAngles++;
                        supportedQuadrants[a / 6] = true;
                    }
                    if (supportedAngles < 13 ||
                        supportedQuadrants.Count(supported => supported) < 3)
                    {
                        if (diagnostics != null) diagnostics.RejectedAnnulus++;
                        continue;
                    }
                    // Only a sufficiently circular, illuminated opening is
                    // allowed to reach the physical diameter limit.
                    if (diameterMm > MaximumOpeningDiameterMm)
                    {
                        if (diagnostics != null)
                        {
                            diagnostics.RejectedTooLarge++;
                            diagnostics.LargestOversizeDiameterMm =
                                Math.Max(diagnostics.LargestOversizeDiameterMm, diameterMm);
                        }
                        continue;
                    }
                    candidates.Add(new Observation {
                        X = centerX, Y = centerY, RadiusPixels = radius,
                        DiameterMm = diameterMm, Contrast = contrast,
                        Circularity = circularity,
                        // Favor the complete opening over a smaller bright-edged
                        // fragment. The physical diameter and ambiguity limits
                        // above are deliberately NOT relaxed.
                        Score = contrast + circularity * 35 +
                            supportedAngles * 1.8 -
                            offset / pixelsPerMm * 14 - radius * 0.10
                    });
                }
            }
            if (candidates.Count == 0) return null;
            var ranked = candidates.OrderByDescending(c => c.Score).ToList();
            var best = ranked[0];
            // Similar-confidence, physically different targets are ambiguous.
            // Threshold variants of the same core are not alternatives.
            foreach (var other in ranked.Skip(1))
            {
                var gap = Math.Sqrt(DistanceSquared(best.X, best.Y, other.X, other.Y));
                if (gap > Math.Max(3, pixelsPerMm * 0.16) &&
                    best.Score - other.Score < 12)
                {
                    if (diagnostics != null) diagnostics.Ambiguous = true;
                    return null;
                }
            }
            return best;
        }

        /// <summary>Prove one physical feature across distinct captured frames.
        /// A missing or jumping circle is a failure, not a best-effort lock.</summary>
        public static bool TryValidate(IReadOnlyList<Observation> samples,
            double pixelsPerMm, out TuneResult result, out string reason)
        {
            result = null;
            reason = null;
            if (samples == null || samples.Count < 20 ||
                !Finite(pixelsPerMm) || pixelsPerMm <= 0)
            {
                reason = "At least 20 fresh frames and calibrated pixels/mm are required.";
                return false;
            }
            var valid = samples.Where(s => s != null).ToList();
            if (valid.Count < Math.Max(18, (int)Math.Ceiling(samples.Count * 0.8)))
            {
                reason = $"Only {valid.Count}/{samples.Count} frames found an unambiguous dark opening.";
                return false;
            }
            var x = Median(valid.Select(s => s.X));
            var y = Median(valid.Select(s => s.Y));
            var diameter = Median(valid.Select(s => s.DiameterMm));
            var maxOffset = valid.Max(s =>
                Math.Sqrt(DistanceSquared(s.X, s.Y, x, y)) / pixelsPerMm);
            var diameterSpread = valid.Max(s => Math.Abs(s.DiameterMm - diameter));
            if (maxOffset > 0.12 || diameterSpread > Math.Max(0.10, diameter * 0.20))
            {
                reason = $"Nozzle feature moved between frames (max center deviation {maxOffset:F3} mm; diameter deviation {diameterSpread:F3} mm).";
                return false;
            }
            result = new TuneResult {
                X = x, Y = y, DiameterMm = diameter,
                MedianContrast = Median(valid.Select(s => s.Contrast)),
                MaxCenterDeviationMm = maxOffset, GoodFrames = valid.Count,
                TotalFrames = samples.Count
            };
            return true;
        }

        private static double Median(IEnumerable<double> values)
        {
            var a = values.OrderBy(v => v).ToArray();
            return a.Length % 2 == 0 ? (a[a.Length / 2 - 1] + a[a.Length / 2]) / 2 : a[a.Length / 2];
        }
        private static int Percentile(int[] histogram, int total, double percentile)
        {
            var target = (int)Math.Ceiling(total * percentile);
            var cumulative = 0;
            for (var i = 0; i < histogram.Length; i++)
            {
                cumulative += histogram[i];
                if (cumulative >= target) return i;
            }
            return 255;
        }
        private static double DistanceSquared(double x, double y, double cx, double cy)
        {
            var dx = x - cx;
            var dy = y - cy;
            return dx * dx + dy * dy;
        }
        private static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
    }
}
