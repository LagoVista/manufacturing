using System;
using System.Collections.Generic;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// One fully visible black bar, measured along the long axis of a feeder marker.
    /// Pixel endpoints are half-open: [StartPixel, EndPixel).
    /// </summary>
    public sealed class FeederBarcodeBar
    {
        public int StartPixel { get; }
        public int EndPixel { get; }
        public double WidthMm { get; }
        public int? Value { get; }

        public FeederBarcodeBar(int startPixel, int endPixel, double pixelsPerMm, int? value)
        {
            StartPixel = startPixel;
            EndPixel = endPixel;
            WidthMm = (endPixel - startPixel) / pixelsPerMm;
            Value = value;
        }
    }

    /// <summary>
    /// First-pass one-dimensional measurement of black bars on a white strip.
    /// This does not assign a feeder identity or alter feeder geometry.
    /// A caller can combine measurements from overlapping camera positions later.
    /// </summary>
    public static class FeederBarcodeSignal
    {
        // Four bar widths on the original 200 x 20 prototype print.
        // Keep these separate from the proposed start/check/end encoding:
        // we must validate the printed marker before defining the final format.
        public static readonly double[] PrototypeWidthsMm = { 0.4, 0.8, 1.2, 1.6 };

        /// <summary>
        /// Extract bounded black runs. A run touching either image edge is
        /// deliberately excluded because its width cannot be measured reliably.
        /// </summary>
        public static IReadOnlyList<FeederBarcodeBar> FindBars(
            byte[] intensity,
            double pixelsPerMm,
            IReadOnlyList<double> widthsMm = null,
            byte whiteThreshold = 128)
        {
            if (intensity == null)
                throw new ArgumentNullException(nameof(intensity));
            if (pixelsPerMm <= 0 || double.IsNaN(pixelsPerMm) || double.IsInfinity(pixelsPerMm))
                throw new ArgumentOutOfRangeException(nameof(pixelsPerMm));

            var result = new List<FeederBarcodeBar>();
            if (intensity.Length < 3)
                return result;

            var nominal = widthsMm ?? PrototypeWidthsMm;
            var bright = new bool[intensity.Length];
            for (var i = 0; i < intensity.Length; i++)
            {
                // A 3-pixel median suppresses isolated threshold speckles without
                // erasing the much wider printed gaps or the narrowest bar.
                var left = intensity[Math.Max(0, i - 1)] >= whiteThreshold;
                var middle = intensity[i] >= whiteThreshold;
                var right = intensity[Math.Min(intensity.Length - 1, i + 1)] >= whiteThreshold;
                bright[i] = (left && middle) || (left && right) || (middle && right);
            }

            var start = -1;
            for (var i = 0; i <= bright.Length; i++)
            {
                var dark = i < bright.Length && !bright[i];
                if (dark)
                {
                    if (start < 0)
                        start = i;
                }
                else if (start >= 0)
                {
                    // Only measure black bars bounded by white on both sides.
                    if (start > 0 && i < bright.Length && bright[start - 1] && bright[i])
                    {
                        var width = (i - start) / pixelsPerMm;
                        if (width >= 0.18 && width <= 3.2)
                        {
                            int? value = null;
                            var closest = double.MaxValue;
                            for (var n = 0; n < nominal.Count; n++)
                            {
                                var error = Math.Abs(width - nominal[n]);
                                if (error < closest)
                                {
                                    closest = error;
                                    value = n;
                                }
                            }

                            // Record the raw width even when it does not fit one of
                            // the four known classes (e.g., a future framing bar).
                            if (closest > 0.14)
                                value = null;

                            result.Add(new FeederBarcodeBar(start, i, pixelsPerMm, value));
                        }
                    }
                    start = -1;
                }
            }

            return result;
        }
    }
}
