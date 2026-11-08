using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederBarcodeScanTests
    {
        // Exact 11-bar proportions on the printable 20 x 2 mm test sheet:
        // START, 8 data symbols 0/1/2/3/0/1/2/3, check 1, END.
        private static readonly int[] PrintedWidths =
            { 250, 50, 75, 100, 125, 50, 75, 100, 125, 75, 25 };

        private static byte[] MakePrintedMarker(int[] widths)
        {
            const int gap = 50;
            const int margin = 200;
            var pixels = Enumerable.Repeat((byte)255,
                widths.Sum() + (widths.Length - 1) * gap + 2 * margin).ToArray();
            var offset = margin;
            foreach (var width in widths)
            {
                for (var i = offset; i < offset + width; i++)
                    pixels[i] = 0;
                offset += width + gap;
            }
            return pixels;
        }

        private static byte[] Frame(byte[] entireMarker, int offset)
        {
            return entireMarker.Skip(offset).Take(850).ToArray();
        }

        [TestCase(40)]
        [TestCase(250)]
        public void ThreeOverlappingMachineCapturesReadCompletePrintedMarker(double nominalPixelsPerMm)
        {
            var marker = MakePrintedMarker(PrintedWidths);
            var scan = new FeederBarcodeScan();
            scan.AddCapture(Frame(marker, 0), 0, nominalPixelsPerMm);
            Assert.That(scan.HasVerifiedStart, Is.True, scan.Status);
            Assert.That(scan.PixelsPerMmFromStart, Is.EqualTo(100).Within(0.0001));
            Assert.That(scan.ObservedBars, Is.EqualTo(4), "The first frame must anchor real machine coordinates.");
            Assert.That(scan.IsComplete, Is.False, "The first camera view should not contain the whole marker.");

            // Machine advances 4.8 mm per capture; camera signal shifts 480 px.
            // The second and third frames have no complete 5x start bar.
            scan.AddCapture(Frame(marker, 480), 4.8, nominalPixelsPerMm);
            scan.AddCapture(Frame(marker, 960), 9.6, nominalPixelsPerMm);

            Assert.That(scan.IsComplete, Is.True, scan.Status);
            Assert.That(scan.ObservedBars, Is.EqualTo(11));
            Assert.That(scan.FeederId, Is.EqualTo(0x1B1B));
            Assert.That(scan.PixelsPerUnit, Is.EqualTo(50));
            Assert.That(scan.CaptureCount, Is.EqualTo(3));
        }

        [Test]
        public void IncompleteMarkerMustNeverProduceIdentity()
        {
            var marker = MakePrintedMarker(PrintedWidths);
            var scan = new FeederBarcodeScan();
            scan.AddCapture(Frame(marker, 0), 0, 100);
            scan.AddCapture(Frame(marker, 480), 4.8, 100);

            Assert.That(scan.IsComplete, Is.False);
            Assert.That(scan.FeederId, Is.Null);
            Assert.That(scan.Status, Does.Contain("READING"));
        }

        [Test]
        public void FullMarkerWithWrongCheckBarMustNotVerify()
        {
            var bad = PrintedWidths.ToArray();
            bad[9] = 100; // check symbol 2 instead of printed symbol 1
            var marker = MakePrintedMarker(bad);
            var scan = new FeederBarcodeScan();

            scan.AddCapture(Frame(marker, 0), 0, 100);
            scan.AddCapture(Frame(marker, 480), 4.8, 100);
            scan.AddCapture(Frame(marker, 960), 9.6, 100);

            Assert.That(scan.IsComplete, Is.False);
            Assert.That(scan.FeederId, Is.Null);
            Assert.That(scan.Status, Does.Contain("CHECK FAILED"));
        }

        [Test]
        public void FullMarkerWithoutEndFramingMustNotVerify()
        {
            var bad = PrintedWidths.ToArray();
            bad[10] = 50; // 1x instead of the required 0.5x END
            var marker = MakePrintedMarker(bad);
            var scan = new FeederBarcodeScan();

            scan.AddCapture(Frame(marker, 0), 0, 100);
            scan.AddCapture(Frame(marker, 480), 4.8, 100);
            scan.AddCapture(Frame(marker, 960), 9.6, 100);

            Assert.That(scan.IsComplete, Is.False);
            Assert.That(scan.FeederId, Is.Null);
            Assert.That(scan.Status, Does.Contain("END BAR"));
        }

        [Test]
        public void CheckSymbolIsDeterministicAndRejectsOtherIdentities()
        {
            Assert.That(FeederBarcodeScan.CalculateCheckSymbol(0x1B1B), Is.EqualTo(1));
            Assert.That(FeederBarcodeScan.CalculateCheckSymbol(0x1B1A), Is.Not.EqualTo(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => FeederBarcodeScan.CalculateCheckSymbol(0x10000));
        }
    }
}
