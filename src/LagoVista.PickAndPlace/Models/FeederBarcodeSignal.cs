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
        /// Read exactly ten pixels just beyond the frame center, across the
        /// narrow axis of a known-orientation marker. Return a binary 1D signal
        /// along the bar-code direction. No image-wide search or OpenCV.
        /// </summary>
        public static byte[] SampleCenterBand(int width, int height, bool vertical,
            Func<int, int, byte> getPixel, byte threshold = 128)
        {
            if (getPixel == null)
                throw new ArgumentNullException(nameof(getPixel));
            if (width < 10 || height < 10)
                throw new ArgumentOutOfRangeException(nameof(width), "Frame must fit the ten-pixel band.");

            const int bandWidth = 10;
            var length = vertical ? height : width;
            var bandStart = (vertical ? width : height) / 2;
            var signal = new byte[length];

            for (var along = 0; along < length; along++)
            {
                var brightCount = 0;
                for (var cross = bandStart; cross < bandStart + bandWidth; cross++)
                {
                    var x = vertical ? cross : along;
                    var y = vertical ? along : cross;
                    if (getPixel(x, y) >= threshold)
                        brightCount++;
                }

                signal[along] = brightCount >= 6 ? (byte)255 : (byte)0;
            }

            return signal;
        }

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

            // XOR adjacent binary samples to find transitions, then measure
            // only fully observed black runs between two white regions.
            // The first or last run is ignored if clipped by the image edge.
            var wasBlack = !bright[0];
            var start = wasBlack ? 0 : -1;
            for (var i = 1; i < bright.Length; i++)
            {
                var black = !bright[i];
                if (black ^ wasBlack)
                {
                    if (black)
                    {
                        start = i;
                    }
                    else if (start > 0)
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

                            // Normalized physical widths are classified against
                            // the four prototype widths; framing bars can remain
                            // unclassified while retaining their raw measurement.
                            if (closest > 0.14)
                                value = null;

                            result.Add(new FeederBarcodeBar(start, i, pixelsPerMm, value));
                        }

                        start = -1;
                    }
                }

                wasBlack = black;
            }

            return result;
        }
    }
}
