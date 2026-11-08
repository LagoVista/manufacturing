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
    /// Read of the 5x-start-bar format. Physical widths are retained for display,
    /// while decoding uses the start bar's measured pixels, not camera pixels/mm.
    /// </summary>
    public sealed class FeederBarcodeCalibratedRead
    {
        public bool HasVerifiedStart { get; }
        public double PixelsPerUnit { get; }
        public double PixelsPerMmFromStart => HasVerifiedStart ? PixelsPerUnit / 0.5 : 0;
        public string Status { get; }
        public IReadOnlyList<FeederBarcodeBar> Bars { get; }

        public FeederBarcodeCalibratedRead(bool hasVerifiedStart, double pixelsPerUnit,
            string status, IReadOnlyList<FeederBarcodeBar> bars)
        {
            HasVerifiedStart = hasVerifiedStart;
            PixelsPerUnit = pixelsPerUnit;
            Status = status;
            Bars = bars;
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

        // Proposed eleven-bar marker:
        // [5x START] [8 data bars] [2-bit check bar] [0.5x END].
        // A data bar encodes 00/01/10/11 using widths 1/1.5/2/2.5 units.
        // Its decoding scale is entirely derived from the known 5-unit start.
        public const double StartBarUnits = 5.0;
        public const double EndBarUnits = 0.5;
        public const double BaseWidthMm = 0.5;
        // Each class is separated by 0.5 units. 0.24 keeps a small
        // rejection region around class midpoints while tolerating the
        // ~0.21-unit edge bias seen on the printed machine marker.
        public const double DataWidthToleranceUnits = 0.24;
        public static readonly double[] DataBarUnits = { 1.0, 1.5, 2.0, 2.5 };

        /// <summary>
        /// Classify a partially observed 11-bar marker only if the beginning
        /// is actually in the image. The first bounded bar must be approximately
        /// the expected 5x start, near the beginning of the frame, and followed
        /// by at least two plausible data bars. The confidence checks prevent a
        /// mid-marker capture from silently calibrating against a data bar.
        /// This is only bar recognition: ID/checksum and stitching come later.
        /// </summary>
        public static FeederBarcodeCalibratedRead ReadFromStart(
            byte[] intensity, double pixelsPerMm, byte whiteThreshold = 128)
        {
            // Read all complete black runs in pixels. Camera pixels/mm is only
            // provisional display metadata, never a pre-calibration size gate.
            var raw = FindBars(intensity, pixelsPerMm, new double[0], whiteThreshold,
                usePhysicalWidthFilter: false);

            FeederBarcodeCalibratedRead Uncalibrated(string reason) =>
                new FeederBarcodeCalibratedRead(false, 0, reason, raw);

            if (raw.Count < 3)
                return Uncalibrated("START NOT VERIFIED: fewer than 3 complete bars");

            var start = raw[0];
            var startPixels = start.EndPixel - start.StartPixel;
            // A nominal 2.5 mm start may appear much larger/smaller when the
            // camera profile is uncalibrated or the entire marker is printed at
            // a different scale. Verify in pixel units against following bars.
            if (start.StartPixel > intensity.Length / 2)
                return Uncalibrated("START NOT VERIFIED: first bar begins too far into frame");

            var pixelsPerUnit = startPixels / StartBarUnits;
            if (pixelsPerUnit < 2)
                return Uncalibrated("START NOT VERIFIED: insufficient resolution");

            var decoded = new List<FeederBarcodeBar>
            {
                new FeederBarcodeBar(start.StartPixel, start.EndPixel, pixelsPerMm, null)
            };

            var confirmedDataBars = 0;
            for (var i = 1; i < raw.Count; i++)
            {
                var bar = raw[i];
                var units = (bar.EndPixel - bar.StartPixel) / pixelsPerUnit;
                int? value = null;
                var closest = double.MaxValue;
                for (var n = 0; n < DataBarUnits.Length; n++)
                {
                    var error = Math.Abs(units - DataBarUnits[n]);
                    if (error < closest)
                    {
                        closest = error;
                        value = n;
                    }
                }

                // Classify against the nearest class, with a narrow
                // guard band around each 0.25-unit midpoint.
                if (closest >= DataWidthToleranceUnits)
                    value = null;
                if (value.HasValue)
                    confirmedDataBars++;

                decoded.Add(new FeederBarcodeBar(bar.StartPixel, bar.EndPixel, pixelsPerMm, value));
            }

            // The two bars immediately after START must both be valid; accepting
            // any two bars elsewhere could lock onto a false start in a partial scan.
            if (!decoded[1].Value.HasValue || !decoded[2].Value.HasValue)
                return Uncalibrated("START NOT VERIFIED: following bar widths disagree");

            return new FeederBarcodeCalibratedRead(true, pixelsPerUnit,
                $"START VERIFIED: {pixelsPerUnit:F1} px/unit, {confirmedDataBars} data bars", decoded);
        }

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
            if (width < 20 || height < 20)
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
            byte whiteThreshold = 128,
            bool usePhysicalWidthFilter = true)
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
                        // Legacy prototype measurement may retain the original
                        // physical plausibility gate. Calibrated decoding must
                        // keep this run regardless of untrusted camera scale.
                        if (!usePhysicalWidthFilter || (width >= 0.18 && width <= 3.2))
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
