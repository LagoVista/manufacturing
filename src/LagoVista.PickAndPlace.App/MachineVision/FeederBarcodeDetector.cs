using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        public static Observation FindBars(Image<Gray, byte> image, double pixelsPerMm, byte threshold)
        {
            if (image == null || pixelsPerMm <= 0)
                return null;

            var data = image.Data;
            Observation best = null;
            double bestScore = 0;
            foreach (var vertical in new[] { true, false })
            {
                var axisLength = vertical ? image.Height : image.Width;
                var crossLength = vertical ? image.Width : image.Height;
                // We know the camera is already near the expected feeder corner.
                // Prefer scan lines near the image center without requiring exact
                // transverse alignment of the narrow printed strip.
                var crossStart = Math.Max(3, crossLength / 2 - 210);
                var crossEnd = Math.Min(crossLength - 3, crossLength / 2 + 210);
                for (var cross = crossStart; cross < crossEnd; cross += 6)
                {
                    var intensity = new byte[axisLength];
                    for (var i = 0; i < axisLength; i++)
                    {
                        // Average a three-pixel-wide line to suppress isolated noise.
                        var sum = vertical
                            ? data[i, cross - 1, 0] + data[i, cross, 0] + data[i, cross + 1, 0]
                            : data[cross - 1, i, 0] + data[cross, i, 0] + data[cross + 1, i, 0];
                        intensity[i] = (byte)(sum / 3);
                    }

                    var bars = FeederBarcodeSignal.FindBars(intensity, pixelsPerMm,
                        FeederBarcodeSignal.PrototypeWidthsMm, threshold);
                    if (bars.Count < 2)
                        continue;

                    var classified = bars.Count(bar => bar.Value.HasValue);
                    var score = classified * 12 + bars.Count * 3 -
                                Math.Abs(cross - crossLength / 2.0) / 60.0;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = new Observation
                        {
                            Vertical = vertical,
                            CrossPixel = cross,
                            Bars = bars
                        };
                    }
                }
            }

            return best;
        }

        public static void Draw(IInputOutputArray output, Observation observation, Size imageSize)
        {
            if (observation == null)
                return;

            var scanColor = new MCvScalar(0, 180, 0);
            if (observation.Vertical)
                CvInvoke.Line(output, new Point(observation.CrossPixel, 0),
                    new Point(observation.CrossPixel, imageSize.Height - 1), scanColor, 1);
            else
                CvInvoke.Line(output, new Point(0, observation.CrossPixel),
                    new Point(imageSize.Width - 1, observation.CrossPixel), scanColor, 1);

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
