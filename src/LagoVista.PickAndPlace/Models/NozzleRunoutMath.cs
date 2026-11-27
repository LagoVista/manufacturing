using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public sealed class NozzleRunoutSample
    {
        public int CommandedAngle { get; set; }
        public double Xmm { get; set; }
        public double Ymm { get; set; }
        public double PixelsPerMm { get; set; }
        public DateTimeOffset CapturedAtUtc { get; set; }
    }

    public sealed class NozzleRunoutResult
    {
        public int LogicalZeroAngle { get; set; }
        public Point2D<double> CenterMm { get; set; }
        public double MaxRadialDeviationMm { get; set; }
        public double RangeXmm { get; set; }
        public double RangeYmm { get; set; }
        public IReadOnlyList<NozzleRunoutSample> Samples { get; set; }
    }

    /// <summary>
    /// Angle-indexed nozzle-tip displacement. Camera-to-tool translation is
    /// deliberately excluded. Zero is a logical high-Y reference, not G92.
    /// </summary>
    public static class NozzleRunoutMath
    {
        public const int IncrementDegrees = 30;
        public const int SampleCount = 12;
        public static int[] SweepAngles() =>
            Enumerable.Range(0, SampleCount).Select(i => i * IncrementDegrees).ToArray();

        private static bool Valid(double x) => !Double.IsNaN(x) && !Double.IsInfinity(x);

        public static double NormalizeAngle(double angle)
        {
            if (!Valid(angle)) throw new ArgumentOutOfRangeException(nameof(angle));
            return (angle % 360 + 360) % 360;
        }

        public static NozzleRunoutResult Analyze(IEnumerable<NozzleRunoutSample> input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var samples = input.OrderBy(s => s.CommandedAngle).ToArray();
            if (samples.Length != SampleCount ||
                samples.Where((s, i) => s.CommandedAngle != i * IncrementDegrees).Any() ||
                samples.Any(s => !Valid(s.Xmm) || !Valid(s.Ymm) ||
                                 !Valid(s.PixelsPerMm) || s.PixelsPerMm <= 0))
                throw new ArgumentException("Require twelve valid nozzle samples at 0,30,...,330 degrees.");
            var mx = samples.Average(s => s.Xmm);
            var my = samples.Average(s => s.Ymm);
            var high = samples.OrderByDescending(s => s.Ymm)
                .ThenBy(s => s.CommandedAngle).First();
            return new NozzleRunoutResult
            {
                LogicalZeroAngle = high.CommandedAngle,
                CenterMm = new Point2D<double>(mx, my),
                MaxRadialDeviationMm = samples.Max(s =>
                    Math.Sqrt(Math.Pow(s.Xmm-mx,2) + Math.Pow(s.Ymm-my,2))),
                RangeXmm = samples.Max(s => s.Xmm)-samples.Min(s => s.Xmm),
                RangeYmm = samples.Max(s => s.Ymm)-samples.Min(s => s.Ymm),
                Samples = samples
            };
        }

        // Negative deviation relative to the best-fit mean. Interpolated
        // across 330->0 without a discontinuity. No machine command is sent.
        public static Point2D<double> Correction(NozzleRunoutResult result, double physicalAngle)
        {
            if (result?.Samples == null || result.Samples.Count != SampleCount)
                throw new ArgumentException("A completed twelve-angle sweep is required.");
            var angle = NormalizeAngle(physicalAngle);
            var lower = (int)Math.Floor(angle / IncrementDegrees);
            var upper = (lower + 1) % SampleCount;
            var fraction = (angle - lower * IncrementDegrees) / IncrementDegrees;
            var a = result.Samples[lower];
            var b = result.Samples[upper];
            return new Point2D<double>(
                result.CenterMm.X - (a.Xmm + (b.Xmm - a.Xmm)*fraction),
                result.CenterMm.Y - (a.Ymm + (b.Ymm - a.Ymm)*fraction));
        }

        public static bool IsRepeatable(NozzleRunoutSample expected,
            NozzleRunoutSample observed, double toleranceMm)
        {
            if (expected == null || observed == null ||
                expected.CommandedAngle != observed.CommandedAngle ||
                !Valid(toleranceMm) || toleranceMm <= 0 ||
                !Valid(observed.Xmm) || !Valid(observed.Ymm)) return false;
            return Math.Sqrt(Math.Pow(expected.Xmm-observed.Xmm,2) +
                             Math.Pow(expected.Ymm-observed.Ymm,2)) <= toleranceMm;
        }
    }
}
