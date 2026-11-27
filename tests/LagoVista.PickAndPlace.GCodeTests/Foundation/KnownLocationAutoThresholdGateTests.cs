using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class KnownLocationAutoThresholdGateTests
    {
        [TestCase(30, 100, 65)]
        [TestCase(60, 160, 110)]
        [TestCase(100, 220, 160)]
        public void AcceptsSeparatedDiscAcrossLighting(int background, int foreground, int threshold)
        {
            var frame = Disc(background, foreground);
            Assert.That(KnownLocationAutoThresholdGate.Assess(frame, frame, threshold,
                out var separation, out _), Is.True);
            Assert.That(separation, Is.EqualTo(foreground - background));
        }

        [TestCase(100, 100, 100)]
        [TestCase(100, 110, 105)]
        [TestCase(0, 150, 75)]
        [TestCase(40, 255, 150)]
        public void RejectsMissingLowContrastOrClippedDisc(int background, int foreground, int threshold)
        {
            var frame = Disc(background, foreground);
            Assert.That(KnownLocationAutoThresholdGate.Assess(frame, frame, threshold,
                out _, out _), Is.False);
        }

        [Test]
        public void SlowThresholdDriftRequiresFreshHistory()
        {
            var gate = new KnownLocationAutoThresholdGate();
            Assert.That(gate.ObserveThreshold(80), Is.False);
            Assert.That(gate.ObserveThreshold(84), Is.True);
            Assert.That(gate.ObserveThreshold(88), Is.True);
            Assert.That(gate.ObserveThreshold(92), Is.False);
            Assert.That(gate.ObserveThreshold(93), Is.True);
            gate.Reset();
            Assert.That(gate.ObserveThreshold(93), Is.False);
        }

        [Test]
        public void CenterAndDiameterDriftRequireFreshHistory()
        {
            var gate = new KnownLocationAutoThresholdGate();
            Assert.That(gate.ObserveCircle(100, 100, 70, 10), Is.False);
            Assert.That(gate.ObserveCircle(100.5, 100, 70.5, 10), Is.True);
            Assert.That(gate.ObserveCircle(101.2, 100, 70, 10), Is.False);
            Assert.That(gate.ObserveCircle(101.2, 100, 72, 10), Is.False);
            Assert.That(gate.ObserveCircle(101.2, 100, 72.5, 10), Is.True);
        }

        [Test]
        public void AutoModeIsOptInAndDoesNotChangeManualValue()
        {
            var profile = new VisionProfile { PrimaryThreshold = 42 };
            Assert.That(profile.AutoThreshold, Is.False);
            profile.AutoThreshold = true;
            profile.AutoThreshold = false;
            Assert.That(profile.PrimaryThreshold, Is.EqualTo(42));
        }

        private static byte[,,] Disc(int background, int foreground)
        {
            var frame = new byte[64, 64, 1];
            for (var y = 0; y < 64; y++)
            for (var x = 0; x < 64; x++)
                frame[y, x, 0] = (byte)((x - 32) * (x - 32) + (y - 32) * (y - 32) < 16 * 16
                    ? foreground : background);
            return frame;
        }
    }
}
