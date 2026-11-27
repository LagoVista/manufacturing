using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class CalibrationImageTransformTests
    {
        [Test]
        public void SquareImageInLandscapeWindowIsLetterboxedPrecisely()
        {
            Assert.That(CalibrationImageTransform.TryCreate(480, 480, 960, 600,
                out var map), Is.True);
            Assert.That(map.Scale, Is.EqualTo(1.25));
            Assert.That(map.Left, Is.EqualTo(180));
            Assert.That(map.Top, Is.Zero);
            Assert.That(map.MapX(240), Is.EqualTo(480));
            Assert.That(map.MapY(240), Is.EqualTo(300));
            Assert.That(map.Radius(168), Is.EqualTo(210));
        }

        [Test]
        public void MatchingAspectHasNoLetterboxOffsets()
        {
            Assert.That(CalibrationImageTransform.TryCreate(640, 480, 800, 600,
                out var map), Is.True);
            Assert.That(map.Left, Is.Zero);
            Assert.That(map.Top, Is.Zero);
            Assert.That(map.MapX(320), Is.EqualTo(400));
            Assert.That(map.MapY(240), Is.EqualTo(300));
        }

        [Test]
        public void PortraitFrameInLandscapeSurfaceHasCorrectCenter()
        {
            Assert.That(CalibrationImageTransform.TryCreate(480, 640, 800, 600,
                out var map), Is.True);
            Assert.That(map.Scale, Is.EqualTo(0.9375));
            Assert.That(map.Left, Is.EqualTo(175));
            Assert.That(map.MapX(0), Is.EqualTo(175));
            Assert.That(map.MapX(480), Is.EqualTo(625));
            Assert.That(map.MapY(640), Is.EqualTo(600));
        }

        [Test]
        public void ExposureAnnulusUsesSameFramePixelScaleAsVideo()
        {
            Assert.That(CalibrationImageTransform.TryCreate(480, 480, 800, 600,
                out var map), Is.True);
            Assert.That(map.Radius(480 * 0.15), Is.EqualTo(90));
            Assert.That(map.Radius(480 * 0.35), Is.EqualTo(210));
        }

        [Test]
        public void RejectsZeroNegativeOrNonFiniteDimensions()
        {
            Assert.That(CalibrationImageTransform.TryCreate(0, 480, 800, 600,
                out _), Is.False);
            Assert.That(CalibrationImageTransform.TryCreate(480, -1, 800, 600,
                out _), Is.False);
            Assert.That(CalibrationImageTransform.TryCreate(480, 480,
                double.NaN, 600, out _), Is.False);
            Assert.That(CalibrationImageTransform.TryCreate(480, 480,
                800, double.PositiveInfinity, out _), Is.False);
        }
    }
}
