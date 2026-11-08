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
        public void BarcodeHasIndependentVisionProfileFromFeederOrigin()
        {
            var profiles = VisionProfile.DefaultVisionProfiles;
            var origin = profiles.Single(p => p.Key == VisionProfile.VisionProfile_FeederOrigin);
            var barcode = profiles.Single(p => p.Key == VisionProfile.VisionProfile_FeederBarcode);
            Assert.That(origin.Key, Is.Not.EqualTo(barcode.Key));
            Assert.That(barcode.Id, Is.Not.EqualTo(origin.Id));
            Assert.That(barcode.Text, Is.EqualTo("Feeder Barcode"));
        }

        [Test]
        public void FiveXStartCalibratesFourWidthsDespiteImperfectCameraScale()
        {
            // Printed widths at 50 px/mm: 5-unit start = 125 px;
            // data = 1, 1.5, 2, 2.5 units; 0.5 unit = 25 px.
            var image = MakeLine(new[] { 125, 25, 38, 50, 63 }, gap: 15);
            // The camera profile is deliberately 16% off the actual pixel scale.
            var result = FeederBarcodeSignal.ReadFromStart(image, pixelsPerMm: 42);
            Assert.That(result.HasVerifiedStart, Is.True, result.Status);
            Assert.That(result.PixelsPerUnit, Is.EqualTo(25).Within(0.0001));
            Assert.That(result.PixelsPerMmFromStart, Is.EqualTo(50).Within(0.0001));
            Assert.That(result.Bars.Count, Is.EqualTo(5));
            Assert.That(result.Bars.Skip(1).Select(bar => bar.Value).ToArray(),
                Is.EqualTo(new int?[] { 0, 1, 2, 3 }));
        }

        [TestCase(20)]
        [TestCase(100)]
        [TestCase(500)]
        public void FiveXStartIgnoresIncorrectCameraPixelsPerMm(double reportedPixelsPerMm)
        {
            // Same physical marker under wildly different nominal camera
            // calibrations: the start bar must never be removed by mm gates.
            var image = MakeLine(new[] { 125, 25, 38, 50, 63 }, gap: 15);
            var result = FeederBarcodeSignal.ReadFromStart(image, reportedPixelsPerMm);

            Assert.That(result.HasVerifiedStart, Is.True, result.Status);
            Assert.That(result.Bars.Count, Is.EqualTo(5));
            Assert.That(result.PixelsPerUnit, Is.EqualTo(25).Within(0.0001));
            Assert.That(result.Bars.Skip(1).Select(bar => bar.Value).ToArray(),
                Is.EqualTo(new int?[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void FiveXStartWorksAtDifferentPrintedMarkerScale()
        {
            // Pixel widths for the same 5:1/1.5/2/2.5 proportions at
            // another print or camera scale, without changing the decoder.
            var image = MakeLine(new[] { 90, 18, 27, 36, 45 }, gap: 12);
            var result = FeederBarcodeSignal.ReadFromStart(image, pixelsPerMm: 200);

            Assert.That(result.HasVerifiedStart, Is.True, result.Status);
            Assert.That(result.PixelsPerUnit, Is.EqualTo(18).Within(0.0001));
            Assert.That(result.Bars.Skip(1).Select(bar => bar.Value).ToArray(),
                Is.EqualTo(new int?[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void IncompleteStartBarMustNotCalibrateUsingFirstVisibleDataBar()
        {
            var complete = MakeLine(new[] { 125, 25, 38, 50 }, gap: 15);
            // Clip the 125-pixel start bar at the image boundary.
            var clipped = complete.Skip(50).ToArray();
            var result = FeederBarcodeSignal.ReadFromStart(clipped, pixelsPerMm: 50);
            Assert.That(result.HasVerifiedStart, Is.False);
        }

        [Test]
        public void LargerFirstDataBarIsNotMistakenForFiveXStart()
        {
            var middleOfMarker = MakeLine(new[] { 63, 25, 50, 38 }, gap: 15);
            var result = FeederBarcodeSignal.ReadFromStart(middleOfMarker, pixelsPerMm: 50);
            Assert.That(result.HasVerifiedStart, Is.False);
        }

        [Test]
        public void IncorrectFollowingWidthRejectsStartCalibration()
        {
            var image = MakeLine(new[] { 125, 93, 50, 38 }, gap: 15);
            var result = FeederBarcodeSignal.ReadFromStart(image, pixelsPerMm: 50);
            Assert.That(result.HasVerifiedStart, Is.False);
            Assert.That(result.Status, Does.Contain("NOT VERIFIED"));
        }

        [Test]
        public void StartBarMustBeEarlyEnoughInFrameToCalibrate()
        {
            var image = Enumerable.Repeat((byte)255, 600)
                .Concat(MakeLine(new[] { 125, 25, 38 }, gap: 15, margin: 20)).ToArray();
            var result = FeederBarcodeSignal.ReadFromStart(image, pixelsPerMm: 50);
            // First bar begins past the midpoint of this long frame.
            Assert.That(result.HasVerifiedStart, Is.False);
        }

        [Test]
        public void InvalidPixelsPerMillimeterIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                FeederBarcodeSignal.FindBars(new byte[] { 255, 0, 255 }, 0));
        }
    }
}
