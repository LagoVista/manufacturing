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
            var bars = FeederBarcodeSignal.FindBars(signal, pixelsPerMm,
                FeederBarcodeSignal.PrototypeWidthsMm, threshold);

            if (bars.Count < 2)
                return null;

            return new Observation
            {
                Vertical = vertical,
                CrossPixel = (vertical ? image.Width : image.Height) / 2,
                Bars = bars
            };
        }

        public static void Draw(IInputOutputArray output, Observation observation, Size imageSize, bool? expectedVertical = null)
        {
            // A framing target, not the machine origin or a detected feature.
            // The start of a vertical marker should be in the upper part of
            // the image; for horizontal markers it belongs near the left edge.
            if (expectedVertical.HasValue)
            {
                var vertical = expectedVertical.Value;
                var axisPixels = vertical ? imageSize.Height : imageSize.Width;
                var guide = (int)Math.Round(axisPixels * 0.20);
                var color = new MCvScalar(0, 210, 255);
                if (vertical)
                    CvInvoke.Line(output, new Point(4, guide),
                        new Point(Math.Min(48, imageSize.Width - 1), guide), color, 2);
                else
                    CvInvoke.Line(output, new Point(guide, 4),
                        new Point(guide, Math.Min(48, imageSize.Height - 1)), color, 2);
                CvInvoke.PutText(output, "Start-bar target",
                    vertical ? new Point(8, Math.Max(14, guide - 8))
                             : new Point(Math.Min(imageSize.Width - 145, guide + 9), 20),
                    Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.35, color, 1);
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

            foreach (var bar in observation.Bars)
            {
                var center = (bar.StartPixel + bar.EndPixel) / 2;
                var color = bar.Value.HasValue
                    ? new MCvScalar(0, 255, 0)
                    : new MCvScalar(0, 180, 255);
                var label = bar.Value.HasValue
                    ? $"{bar.WidthMm:F2}mm [{bar.Value.Value}]"
                    : $"{bar.WidthMm:F2}mm [?]";

                if (observation.Vertical)
                {
                    CvInvoke.Line(output, new Point(observation.CrossPixel - 10, center),
                        new Point(observation.CrossPixel + 10, center), color, 2);
                    var x = Math.Max(3, Math.Min(imageSize.Width - 115, observation.CrossPixel + 15));
                    CvInvoke.PutText(output, label,
                        new Point(x, Math.Max(15, Math.Min(imageSize.Height - 4, center))),
                        Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.38, color, 1);
                }
                else
                {
                    CvInvoke.Line(output, new Point(center, observation.CrossPixel - 10),
                        new Point(center, observation.CrossPixel + 10), color, 2);
                    CvInvoke.PutText(output, label,
                        new Point(Math.Max(3, Math.Min(imageSize.Width - 115, center - 20)),
                                  Math.Max(15, observation.CrossPixel - 16)),
                        Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.38, color, 1);
                }
            }

            CvInvoke.PutText(output,
                $"Feeder bars: {observation.Bars.Count} ({(observation.Vertical ? "vertical" : "horizontal")})",
                new Point(8, imageSize.Height - 12),
                Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.48,
                new MCvScalar(0, 255, 0), 1);
        }
    }
}
