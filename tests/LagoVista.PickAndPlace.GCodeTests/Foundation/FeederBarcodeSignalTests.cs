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
        public void InvalidPixelsPerMillimeterIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                FeederBarcodeSignal.FindBars(new byte[] { 255, 0, 255 }, 0));
        }
    }
}
