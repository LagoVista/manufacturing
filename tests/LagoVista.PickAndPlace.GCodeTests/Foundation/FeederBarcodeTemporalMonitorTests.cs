using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederBarcodeTemporalMonitorTests
    {
        private static byte[] Marker(int startWidthPixels, int margin = 25)
        {
            // Measured machine widths: nominal 255px START, 63px first
            // data bar. When START shrinks to 253px the first symbol's
            // normalized width crosses the strict 0.24-unit tolerance.
            var widths = new[] { startWidthPixels, 63, 85, 108, 132 };
            const int gap = 21;
            var pixels = Enumerable.Repeat((byte)255,
                margin * 2 + widths.Sum() + gap * (widths.Length - 1)).ToArray();
            var position = margin;
            foreach (var width in widths)
            {
                for (var i = position; i < position + width; i++)
                    pixels[i] = 0;
                position += width + gap;
            }
            // Keep the camera frame the SAME size when the measured edge
            // shifts, as happens in the actual video.
            return pixels.Take(750).Concat(Enumerable.Repeat((byte)255,
                Math.Max(0, 750 - pixels.Length))).ToArray();
        }

        private static FeederBarcodeTemporalFrame Add(FeederBarcodeTemporalMonitor monitor,
            byte[] signal)
        {
            var raw = FeederBarcodeSignal.ReadFromStart(signal, pixelsPerMm: 40);
            return monitor.Observe(signal, pixelsPerMm: 40, whiteThreshold: 128, raw: raw);
        }

        [Test]
        public void TwoPixelStartChangeReallyFlipsRawVerification()
        {
            var verified = FeederBarcodeSignal.ReadFromStart(Marker(255), 40);
            var unverified = FeederBarcodeSignal.ReadFromStart(Marker(253), 40);

            Assert.That(verified.HasVerifiedStart, Is.True, verified.Status);
            Assert.That(unverified.HasVerifiedStart, Is.False, unverified.Status);
            Assert.That(verified.Bars[0].EndPixel - verified.Bars[0].StartPixel,
                Is.EqualTo(255));
            Assert.That(unverified.Bars[1].EndPixel - unverified.Bars[1].StartPixel,
                Is.EqualTo(63));
        }

        [Test]
        public void OneRawUnverifiedFrameDoesNotFlipTheStabilizedDisplay()
        {
            var monitor = new FeederBarcodeTemporalMonitor();
            for (var i = 0; i < 3; i++)
                Add(monitor, Marker(255));

            var jitter = Add(monitor, Marker(253));
            Assert.That(jitter.RawRead.HasVerifiedStart, Is.False);
            Assert.That(jitter.DisplayRead.HasVerifiedStart, Is.True, jitter.Diagnostics);
            Assert.That(jitter.MedianWindowFrames, Is.EqualTo(3));
            Assert.That(jitter.UsingHold, Is.False);
            Assert.That(jitter.DisplayRead.PixelsPerUnit, Is.EqualTo(51));
        }

        [Test]
        public void ALongUnverifiedSequenceEventuallyStopsShowingOldCalibration()
        {
            var monitor = new FeederBarcodeTemporalMonitor();
            for (var i = 0; i < 3; i++)
                Add(monitor, Marker(255));

            FeederBarcodeTemporalFrame later = null;
            var sawExplicitHold = false;
            for (var i = 0; i < 7; i++)
            {
                later = Add(monitor, Marker(253));
                sawExplicitHold |= later.UsingHold;
            }

            Assert.That(sawExplicitHold, Is.True, "Short holds must be labeled.");
            Assert.That(later.UsingHold, Is.False);
            Assert.That(later.DisplayRead.HasVerifiedStart, Is.False,
                "Do not paint permanent green from an old good frame.");
        }

        [Test]
        public void ThirtyRawEdgeFramesExposeThresholdJitterAndNeverAlterInput()
        {
            var monitor = new FeederBarcodeTemporalMonitor();
            FeederBarcodeTemporalFrame final = null;
            for (var i = 0; i < 30; i++)
            {
                var signal = Marker(i % 2 == 0 ? 255 : 253, margin: 25 + (i % 2));
                var copy = (byte[])signal.Clone();
                final = Add(monitor, signal);
                Assert.That(signal, Is.EqualTo(copy),
                    "The diagnostic must not modify the frame used by the scanner.");
            }

            Assert.That(monitor.DiagnosticFrames, Is.EqualTo(30));
            Assert.That(final.Diagnostics, Does.Contain("RAW30 30/30"));
            Assert.That(final.Diagnostics, Does.Contain("OK 15/30"));
            Assert.That(final.Diagnostics, Does.Contain("width 253-255px"));
            Assert.That(monitor.MedianFrames, Is.EqualTo(5));
        }

        [Test]
        public void CameraMotionClearsPreviousTemporalHistory()
        {
            var monitor = new FeederBarcodeTemporalMonitor();
            for (var i = 0; i < 8; i++)
                Add(monitor, Marker(255, margin: 25));

            var moved = Add(monitor, Marker(255, margin: 55));
            Assert.That(monitor.DiagnosticFrames, Is.EqualTo(1));
            Assert.That(moved.MedianWindowFrames, Is.Zero);
            Assert.That(moved.UsingHold, Is.False);
            Assert.That(moved.RawRead, Is.SameAs(moved.DisplayRead));
        }

        [Test]
        public void ClippedOrMissingStartClearsOldDisplay()
        {
            var monitor = new FeederBarcodeTemporalMonitor();
            for (var i = 0; i < 5; i++)
                Add(monitor, Marker(255));

            var blank = Enumerable.Repeat((byte)255, 750).ToArray();
            var noStart = Add(monitor, blank);
            Assert.That(noStart.DisplayRead.HasVerifiedStart, Is.False);
            Assert.That(monitor.DiagnosticFrames, Is.Zero);
            Assert.That(noStart.UsingHold, Is.False);
        }
    }
}
