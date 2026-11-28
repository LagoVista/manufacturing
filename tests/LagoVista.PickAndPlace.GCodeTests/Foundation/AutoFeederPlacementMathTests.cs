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
        public void BackfillsFirstSlotUsingNominalOffsetWithoutDoubleCounting()
        {
            Assert.That(AutoFeederPlacementMath.TryBackfillFirstSlot(new Point2D<double>(66,4),
                new Point2D<double>(9,7),new Point2D<double>(7,4),26,26,
                new Point2D<double>(500,500),out var origin),Is.True);
            Assert.That(origin.X,Is.EqualTo(68));
            Assert.That(origin.Y,Is.EqualTo(7));
            Assert.That(AutoFeederPlacementMath.TryBackfillFirstSlot(new Point2D<double>(66,4),
                new Point2D<double>(9,7),new Point2D<double>(7,4),35,26,
                new Point2D<double>(500,500),out _),Is.False);
        }

        [Test]
        public void RailBackfillUsesTemplateRatherThanPreviouslyCalibratedOffset()
        {
            var old = new Point2D<double>(66,4);
            var nominal = new Point2D<double>(5,65.5);
            var measured = new Point2D<double>(7,69);
            Assert.That(AutoFeederPlacementMath.TryBackfillFirstSlot(old, measured, nominal, 26,26,
                new Point2D<double>(500,500),out var rail),Is.True);
            Assert.That(rail.X,Is.EqualTo(68));
            Assert.That(rail.Y,Is.EqualTo(7.5));
            Assert.That(AutoFeederPlacementMath.TryOrigin(rail,26,12,35,15,FeederRotations.Zero,out var slot),Is.True);
            Assert.That(slot.X+nominal.X,Is.EqualTo(208));
            Assert.That(slot.Y+nominal.Y,Is.EqualTo(73));
            Assert.That((old+measured-rail).X,Is.EqualTo(nominal.X));
            Assert.That((old+measured-rail).Y,Is.EqualTo(nominal.Y));
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
