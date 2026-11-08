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
            // The physical camera keeps the same frame size even after
            // the end of the printed label has moved into view.
            var frame = entireMarker.Skip(offset).Take(850).ToArray();
            return frame.Concat(Enumerable.Repeat((byte)255, 850 - frame.Length)).ToArray();
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
        public void OverlappingScanAcceptsRealCameraFirstSymbolWidthBias()
        {
            var widths = PrintedWidths.ToArray();
            // The narrowest symbol in the real camera image is approximately
            // 1.21x the start-derived unit instead of its nominal 1.0x.
            widths[1] = 61;
            widths[5] = 61;

            var marker = MakePrintedMarker(widths);
            var scan = new FeederBarcodeScan();
            scan.AddCapture(Frame(marker, 0), 0, 50);
            scan.AddCapture(Frame(marker, 480), 4.8, 50);
            scan.AddCapture(Frame(marker, 960), 9.6, 50);

            Assert.That(scan.IsComplete, Is.True, scan.Status);
            Assert.That(scan.FeederId, Is.EqualTo(0x1B1B));
            Assert.That(scan.ObservedBars, Is.EqualTo(11));
        }

        [Test]
        public void ScannerKeepsStartScaleAfterStartLeavesLiveView()
        {
            var marker = MakePrintedMarker(PrintedWidths);
            var scan = new FeederBarcodeScan();
            scan.AddCapture(Frame(marker, 0), 0, 45);
            Assert.That(scan.HasVerifiedStart, Is.True, scan.Status);

            scan.AddCapture(Frame(marker, 480), 4.8, 250);
            Assert.That(scan.HasVerifiedStart, Is.True, scan.Status);
            Assert.That(scan.PixelsPerUnit, Is.EqualTo(50).Within(0.0001));
            Assert.That(scan.ObservedBars, Is.EqualTo(7));
            Assert.That(scan.LastOverlapCount, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void OverlappingFramesCorrectReportedMachinePositionDrift()
        {
            var marker = MakePrintedMarker(PrintedWidths);
            var scan = new FeederBarcodeScan();
            scan.AddCapture(Frame(marker, 0), 0, 100);

            // The pixels are those from true 4.8/9.6 mm advances, but the
            // reported machine positions have 0.65/-0.45 mm error. The
            // old 0.28 mm exact-coordinate deduplication produced 15+ bars.
            scan.AddCapture(Frame(marker, 480), 5.45, 100);
            Assert.That(scan.LastOverlapCount, Is.GreaterThanOrEqualTo(2), scan.Status);
            Assert.That(scan.LastRegistrationCorrectionMm, Is.EqualTo(-0.65).Within(0.02));
            scan.AddCapture(Frame(marker, 960), 9.15, 100);

            Assert.That(scan.IsComplete, Is.True, scan.Status);
            Assert.That(scan.ObservedBars, Is.EqualTo(11));
            Assert.That(scan.FeederId, Is.EqualTo(0x1B1B));
        }

        [Test]
        public void SmallerThreeMillimeterStepsPreserveOverlapToFinalEndMarker()
        {
            var marker = MakePrintedMarker(PrintedWidths);
            var scan = new FeederBarcodeScan();
            for (var step = 0; step <= 4; step++)
            {
                scan.AddCapture(Frame(marker, step * 300), step * 3.0, 45);
                Assert.That(scan.Status, Does.Not.Contain("OVERLAP NOT VERIFIED"));
            }

            Assert.That(scan.IsComplete, Is.True, scan.Status);
            Assert.That(scan.ObservedBars, Is.EqualTo(11));
            Assert.That(scan.FeederId, Is.EqualTo(0x1B1B));
        }

        [Test]
        public void MissingOverlapIsRejectedRatherThanDoubleCounting()
        {
            var marker = MakePrintedMarker(PrintedWidths);
            var scan = new FeederBarcodeScan();
            scan.AddCapture(Frame(marker, 0), 0, 100);
            var beforeCount = scan.ObservedBars;

            // Jumping close to the end does not provide a valid two-bar overlap.
            scan.AddCapture(Frame(marker, 1300), 13, 100);

            Assert.That(scan.ObservedBars, Is.EqualTo(beforeCount));
            Assert.That(scan.IsComplete, Is.False);
            Assert.That(scan.Status, Does.Contain("OVERLAP NOT VERIFIED"));
            Assert.That(scan.HasVerifiedStart, Is.True);
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
