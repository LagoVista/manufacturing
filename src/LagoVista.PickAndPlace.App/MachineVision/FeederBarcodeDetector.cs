using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    /// <summary>
    /// Finds a readable one-dimensional cross-section of the printed feeder marker.
    /// The marker may be vertical or horizontal. The first pass intentionally
    /// measures visible bars only; feeder identity and multi-frame stitching follow.
    /// </summary>
    internal static class FeederBarcodeDetector
    {
        internal sealed class Observation
        {
            public bool Vertical { get; set; }
            public int CrossPixel { get; set; }
            public IReadOnlyList<FeederBarcodeBar> Bars { get; set; }
            public FeederBarcodeCalibratedRead Calibration { get; set; }
            public byte[] Signal { get; set; }
        }

        public static Observation FindBars(Image<Gray, byte> image, double pixelsPerMm,
            byte threshold, bool? expectedVertical = null)
        {
            // The selected feeder tells us the orientation. Never hunt elsewhere
            // in the frame or infer another orientation from unrelated noise.
            if (image == null || pixelsPerMm <= 0 || !expectedVertical.HasValue)
                return null;

            var vertical = expectedVertical.Value;
            var data = image.Data;
            var signal = FeederBarcodeSignal.SampleCenterBand(
                image.Width, image.Height, vertical, (x, y) => data[y, x, 0], threshold);
            var calibration = FeederBarcodeSignal.ReadFromStart(signal, pixelsPerMm, threshold);
            // Preserve the raw center-band signal even in frames without a
            // complete bounded bar; overlapping scans still need a fresh sample.
            return new Observation
            {
                Vertical = vertical,
                CrossPixel = (vertical ? image.Width : image.Height) / 2,
                Bars = calibration.Bars,
                Calibration = calibration,
                Signal = signal
            };
        }

        public static void Draw(IInputOutputArray output, Observation observation, Size imageSize, bool? expectedVertical = null)
        {
            // A framing target, not the machine origin or a detected feature.
            // The start of a vertical marker should be fully visible in the upper part of
            // the image; for horizontal markers it belongs near the left edge.
            if (expectedVertical.HasValue)
            {
                var vertical = expectedVertical.Value;
                var axisPixels = vertical ? imageSize.Height : imageSize.Width;
                var guide = (int)Math.Round(axisPixels * 0.35);
                var color = new MCvScalar(0, 210, 255);
                // Keep the target next to the fixed center sampling band,
                // rather than at the image's outer border.
                var center = (vertical ? imageSize.Width : imageSize.Height) / 2;
                if (vertical)
                    CvInvoke.Line(output, new Point(center - 15, guide),
                        new Point(center + 35, guide), color, 2);
                else
                    CvInvoke.Line(output, new Point(guide, center - 15),
                        new Point(guide, center + 35), color, 2);
                // This is a framing guide, NOT a detected START bar.
                // Keep the label distinct from the verified "START 5x" annotation.
                DrawReadableText(output, "FRAME",
                    vertical ? new Point(Math.Min(imageSize.Width - 120, center + 40), Math.Max(24, guide - 8))
                             : new Point(Math.Min(imageSize.Width - 120, guide + 8), Math.Max(24, center - 25)),
                    color, 0.85);
            }

            if (observation == null)
                return;

            // This is the ONLY sampled region: ten pixels to the right
            // of center for vertical feeders, or below center for horizontal.
            var scanColor = new MCvScalar(0, 180, 0);
            if (observation.Vertical)
                CvInvoke.Rectangle(output,
                    new Rectangle(observation.CrossPixel, 0,
                        10, imageSize.Height - 1), scanColor, 1);
            else
                CvInvoke.Rectangle(output,
                    new Rectangle(0, observation.CrossPixel,
                        imageSize.Width - 1, 10), scanColor, 1);

            var number = 0;
            foreach (var bar in observation.Bars)
            {
                number++;
                var center = (bar.StartPixel + bar.EndPixel) / 2;
                var verified = observation.Calibration?.HasVerifiedStart == true;
                var startBar = verified && number == 1;
                var color = startBar || (verified && bar.Value.HasValue)
                    ? new MCvScalar(0, 255, 0)
                    : new MCvScalar(0, 180, 255);
                // Use the *relative*, self-calibrated bar width when available.
                // Before START is verified, show raw pixels rather than
                // millimeters calculated from an untrusted camera scale.
                var units = verified
                    ? (bar.EndPixel - bar.StartPixel) / observation.Calibration.PixelsPerUnit
                    : 0;
                var label = startBar
                    ? "START 5x"
                    : verified && bar.Value.HasValue
                        ? $"{number - 1}: {units:F1}x [{bar.Value.Value}]"
                        : $"{number}: {bar.EndPixel - bar.StartPixel}px [?]";

                if (observation.Vertical)
                {
                    CvInvoke.Line(output, new Point(observation.CrossPixel - 10, center),
                        new Point(observation.CrossPixel + 10, center), color, 2);
                    var x = Math.Max(3, Math.Min(imageSize.Width - 176, observation.CrossPixel + 23));
                    DrawReadableText(output, label,
                        new Point(x, Math.Max(22, Math.Min(imageSize.Height - 7, center))),
                        color, 0.82);
                }
                else
                {
                    CvInvoke.Line(output, new Point(center, observation.CrossPixel - 10),
                        new Point(center, observation.CrossPixel + 10), color, 2);
                    // Horizontal bars can be close together: use stacked labels
                    // below the center band rather than overlapping them along X.
                    var y = observation.CrossPixel + 42 + (number - 1) * 28;
                    if (y + 6 < imageSize.Height)
                        DrawReadableText(output, label, new Point(12, y), color, 0.82);
                }
            }

            var calibrated = observation.Calibration?.HasVerifiedStart == true;
            var status = calibrated
                ? $"START OK {observation.Calibration.PixelsPerUnit:F1}px/unit"
                : "START NOT VERIFIED";
            DrawReadableText(output, status,
                new Point(8, imageSize.Height - 43),
                calibrated ? new MCvScalar(0, 255, 0) : new MCvScalar(0, 180, 255), 0.78);
            DrawReadableText(output, $"BARS: {observation.Bars.Count}",
                new Point(8, imageSize.Height - 13),
                new MCvScalar(255, 255, 255), 0.68);
        }

        // Labels are overlays only; the binary sampling band and decoded widths
        // are unchanged. The dark outline keeps large characters legible over
        // both the bright printed strip and the black camera background.
        private static void DrawReadableText(IInputOutputArray output, string text,
            Point position, MCvScalar color, double scale)
        {
            CvInvoke.PutText(output, text, position,
                Emgu.CV.CvEnum.FontFace.HersheySimplex, scale,
                new MCvScalar(0, 0, 0), 5);
            CvInvoke.PutText(output, text, position,
                Emgu.CV.CvEnum.FontFace.HersheySimplex, scale, color, 2);
        }
    }
}
