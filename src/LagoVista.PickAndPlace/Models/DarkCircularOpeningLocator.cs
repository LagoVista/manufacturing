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

        /// <summary>Find a single unambiguous, circular dark core in raw grayscale.
        /// expectedDiameterMm = 0 discovers the opening; a positive value
        /// locks subsequent tracking to the verified physical feature.</summary>
        public static Observation Find(byte[,,] gray, int width, int height,
            double pixelsPerMm, double searchRadiusMm, double expectedDiameterMm = 0)
        {
            if (gray == null || width < 32 || height < 32 ||
                gray.GetLength(0) < height || gray.GetLength(1) < width ||
                gray.GetLength(2) < 1 || !Finite(pixelsPerMm) || pixelsPerMm <= 0 ||
                !Finite(searchRadiusMm) || searchRadiusMm <= 0 ||
                !Finite(expectedDiameterMm) || expectedDiameterMm < 0)
                return null;

            var cx = width / 2.0;
            var cy = height / 2.0;
            var searchPx = Math.Min(Math.Min(width, height) / 2.0 - 2,
                searchRadiusMm * pixelsPerMm);
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
            var low = Percentile(histogram, total, 0.01);
            var high = Percentile(histogram, total, 0.80);
            if (high - low < 25) return null;

            // A bounded, repeatable parameter sweep from this frame's
            // histogram, not from manually tuned profile thresholds.
            var thresholds = new SortedSet<int>();
            foreach (var fraction in new[] { 0.13, 0.23, 0.33, 0.43, 0.53, 0.63 })
                thresholds.Add(Math.Max(0, Math.Min(254,
                    (int)Math.Round(low + (high - low) * fraction))));

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

                    if (touchesEdge || area < 12 || perimeter == 0) continue;
                    var radius = Math.Sqrt(area / Math.PI);
                    var diameterMm = 2 * radius / pixelsPerMm;
                    if (diameterMm < 0.10 || diameterMm > 1.40) continue;
                    if (expectedDiameterMm > 0 &&
                        Math.Abs(diameterMm - expectedDiameterMm) >
                        Math.Max(0.12, expectedDiameterMm * 0.30))
                        continue;
                    var aspect = (maxX - minX + 1.0) / (maxY - minY + 1.0);
                    if (aspect < 0.72 || aspect > 1.38) continue;
                    var circularity = Math.Min(1.0, 4.0 * Math.PI * area /
                        (perimeter * perimeter));
                    if (circularity < 0.42) continue;
                    var centerX = sx / area;
                    var centerY = sy / area;
                    var offset = Math.Sqrt(DistanceSquared(centerX, centerY, cx, cy));
                    if (offset + radius > searchPx) continue;

                    // Compare the filled core to its immediate illuminated
                    // surround. Dark camera background and dark concentric
                    // rings without a bright inner surround cannot pass.
                    double surrounding = 0;
                    var ringCount = 0;
                    foreach (var ringScale in new[] { 1.45, 1.85 })
                        for (var a = 0; a < 24; a++)
                        {
                            var theta = 2 * Math.PI * a / 24.0;
                            var x = (int)Math.Round(centerX +
                                Math.Cos(theta) * radius * ringScale);
                            var y = (int)Math.Round(centerY +
                                Math.Sin(theta) * radius * ringScale);
                            if (x >= 0 && x < width && y >= 0 && y < height)
                            {
                                surrounding += gray[y, x, 0];
                                ringCount++;
                            }
                        }
                    if (ringCount < 36) continue;
                    var contrast = surrounding / ringCount - darkSum / area;
                    if (contrast < 35) continue;
                    candidates.Add(new Observation {
                        X = centerX, Y = centerY, RadiusPixels = radius,
                        DiameterMm = diameterMm, Contrast = contrast,
                        Circularity = circularity,
                        Score = contrast + circularity * 35 -
                            offset / pixelsPerMm * 14 - radius * 0.55
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
                    return null;
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
