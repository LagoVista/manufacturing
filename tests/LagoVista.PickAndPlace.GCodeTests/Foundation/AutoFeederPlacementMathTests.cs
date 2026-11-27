using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests
{
    public class AutoFeederPlacementMathTests
    {
        [TestCase(FeederRotations.Zero, 130)]
        [TestCase(FeederRotations.OneEighty, 70)]
        public void UsesRailPitchAndDirection(FeederRotations rotation, double expected)
        {
            Assert.That(AutoFeederPlacementMath.TryOrigin(new Point2D<double>(100, 20), 10, 4, 12, 15, rotation, out var origin), Is.True);
            Assert.That(origin.X, Is.EqualTo(expected));
            Assert.That(origin.Y, Is.EqualTo(20));
        }

        [Test]
        public void RejectsExclusiveEndAndUnsupportedRotation()
        {
            Assert.That(AutoFeederPlacementMath.ContainsSlot(10, 4, 14), Is.False);
            Assert.That(AutoFeederPlacementMath.ContainsSlot(10, 4, 13), Is.True);
            Assert.That(AutoFeederPlacementMath.TryOrigin(new Point2D<double>(100, 20), 10, 4, 12, 15, FeederRotations.Ninety, out _), Is.False);
        }

        [Test]
        public void OpticalResidualUsesCalibratedScaleAndInvertedImageY()
        {
            var p = new Point2D<double>(100, 20);
            var obs = new FiducialCircleObservation { OffsetXpx = 20, OffsetYpx = -10, RadiusMm = 1 };
            Assert.That(AutoFeederPlacementMath.TryMeasure(p, p, obs, 10, 5, .5, 2,
                new Point2D<double>(200, 200), out var target, out var residual), Is.True);
            Assert.That(target.X, Is.EqualTo(102));
            Assert.That(target.Y, Is.EqualTo(21));
            Assert.That(residual, Is.EqualTo(System.Math.Sqrt(5)).Within(.00001));
            Assert.That(AutoFeederPlacementMath.TryMeasure(p, p, obs, 10, 1, .5, 2,
                new Point2D<double>(200, 200), out _, out _), Is.False);
        }
    }
}
