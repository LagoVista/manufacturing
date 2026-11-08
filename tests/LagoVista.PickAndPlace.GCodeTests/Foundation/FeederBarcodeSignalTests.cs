using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederBarcodeSignalTests
    {
        private static byte[] MakeLine(int[] widths, int gap = 30, int margin = 25)
        {
            var length = widths.Sum() + (widths.Length - 1) * gap + margin * 2;
            var line = Enumerable.Repeat((byte)255, length).ToArray();
            var x = margin;
            foreach (var width in widths)
            {
                for (var i = x; i < x + width; i++)
                    line[i] = 0;
                x += width + gap;
            }
            return line;
        }

        [Test]
        public void DetectFourBarWidthsFromPrintedPrototype()
        {
            var input = MakeLine(new[] { 40, 80, 120, 160 });
            var bars = FeederBarcodeSignal.FindBars(input, 100);

            Assert.That(bars.Count, Is.EqualTo(4));
            Assert.That(bars.Select(bar => bar.Value).ToArray(),
                Is.EqualTo(new int?[] { 0, 1, 2, 3 }));
            Assert.That(bars.Select(bar => bar.WidthMm).ToArray(),
                Is.EqualTo(new[] { 0.4, 0.8, 1.2, 1.6 }).Within(0.0001));
        }

        [Test]
        public void IsolatedThresholdNoiseDoesNotSplitBars()
        {
            var input = MakeLine(new[] { 40, 80, 120 });
            input[29] = 255; // one white pixel inside the first black bar
            input[65] = 0;   // one black pixel in a white gap

            var bars = FeederBarcodeSignal.FindBars(input, 100);

            Assert.That(bars.Count, Is.EqualTo(3));
            Assert.That(bars.Select(bar => bar.Value).ToArray(),
                Is.EqualTo(new int?[] { 0, 1, 2 }));
        }

        [Test]
        public void BarsClippedByImageEdgesAreNotMeasured()
        {
            var input = MakeLine(new[] { 40, 80, 120 });
            for (var i = 0; i < 10; i++) input[i] = 0;
            for (var i = input.Length - 10; i < input.Length; i++) input[i] = 0;

            var bars = FeederBarcodeSignal.FindBars(input, 100);

            // The dark exterior is not a barcode bar; three bounded runs remain.
            Assert.That(bars.Count, Is.EqualTo(3));
        }

        [Test]
        public void FramingBarIsMeasuredButNotMisclassifiedAsData()
        {
            var input = MakeLine(new[] { 250, 25, 80 });
            var bars = FeederBarcodeSignal.FindBars(input, 100);

            Assert.That(bars.Count, Is.EqualTo(3));
            Assert.That(bars[0].WidthMm, Is.EqualTo(2.5).Within(0.0001));
            Assert.That(bars[0].Value, Is.Null);
            Assert.That(bars[1].Value, Is.Null);
            Assert.That(bars[2].Value, Is.EqualTo(1));
        }

        [Test]
        public void FourWidthsStayDistinctWithScreenshotSizedGaps()
        {
            // Representative screen-space widths measured from the real machine
            // image. The frame's display scale is only a test fixture; use the
            // calibrated camera pixels/mm for live physical measurements.
            var input = MakeLine(new[] { 38, 55, 24, 74, 55, 38 }, gap: 16);
            var bars = FeederBarcodeSignal.FindBars(input, 50);

            Assert.That(bars.Count, Is.EqualTo(6));
            Assert.That(bars.Select(bar => bar.Value).ToArray(),
                Is.EqualTo(new int?[] { 1, 2, 0, 3, 2, 1 }));
        }

        [Test]
        public void VerticalCenterBandUsesOnlyTenPixelsAndXorTransitions()
        {
            const int width = 80, height = 400;
            var blackRanges = new[] { (25, 65), (100, 180), (215, 335) };
            Func<int, int, byte> pixel = (x, y) =>
            {
                // A different, distracting pattern occupies most of the frame.
                // It must never influence the deterministic center-band read.
                if (x < width / 2 || x >= width / 2 + 10)
                    return y % 7 == 0 ? (byte)255 : (byte)0;

                foreach (var range in blackRanges)
                    if (y >= range.Item1 && y < range.Item2)
                        return (byte)0;

                return (byte)255;
            };

            var binary = FeederBarcodeSignal.SampleCenterBand(width, height, true, pixel);
            var bars = FeederBarcodeSignal.FindBars(binary, 100);
            Assert.That(binary.All(value => value == 0 || value == 255), Is.True);
            Assert.That(bars.Count, Is.EqualTo(3));
            Assert.That(bars.Select(bar => bar.StartPixel), Is.EqualTo(new[] { 25, 100, 215 }));
            Assert.That(bars.Select(bar => bar.EndPixel), Is.EqualTo(new[] { 65, 180, 335 }));
            Assert.That(bars.Select(bar => bar.Value).ToArray(),
                Is.EqualTo(new int?[] { 0, 1, 2 }));
        }

        [Test]
        public void HorizontalCenterBandReadsLeftToRight()
        {
            const int width = 400, height = 80;
            Func<int, int, byte> pixel = (x, y) =>
            {
                if (y < height / 2 || y >= height / 2 + 10)
                    return (byte)0;
                return (x >= 25 && x < 65) || (x >= 100 && x < 180)
                    ? (byte)0 : (byte)255;
            };
            var binary = FeederBarcodeSignal.SampleCenterBand(width, height, false, pixel);
            var bars = FeederBarcodeSignal.FindBars(binary, 100);
            Assert.That(bars.Count, Is.EqualTo(2));
            Assert.That(bars.Select(bar => bar.Value).ToArray(),
                Is.EqualTo(new int?[] { 0, 1 }));
        }

        [Test]
        public void InvalidPixelsPerMillimeterIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                FeederBarcodeSignal.FindBars(new byte[] { 255, 0, 255 }, 0));
        }
    }
}
