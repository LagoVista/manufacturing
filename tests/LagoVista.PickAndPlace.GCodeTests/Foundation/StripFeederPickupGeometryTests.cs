using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    [TestFixture]
    public class StripFeederPickupGeometryTests
    {
        private static Point2D<double> At(
            int index, bool vertical, bool forward, bool holesOnTop,
            double pitch = 4, double packageAlong = 2, double packageAcross = 3.5)
            => StripFeederPickupCalculator.Calculate(
                new Point2D<double>(100, 200),
                new Point2D<double>(0, 0), new Point2D<double>(40, 0),
                vertical, holesOnTop, forward, index,
                pitch, 8, packageAlong, packageAcross, false);

        [TestCase(false, true, true, 102, 196.5)]
        [TestCase(false, true, false, 102, 203.5)]
        [TestCase(true, true, true, 96.5, 198)]
        [TestCase(true, true, false, 103.5, 198)]
        [TestCase(false, false, true, 138, 196.5)]
        [TestCase(false, false, false, 138, 203.5)]
        [TestCase(true, false, true, 96.5, 162)]
        [TestCase(true, false, false, 103.5, 162)]
        public void DirectionAndOrientation_SelectCorrectReferenceEnd(
            bool vertical, bool forward, bool holesOnTop, double x, double y)
        {
            var point = At(1, vertical, forward, holesOnTop);
            Assert.That(point.X, Is.EqualTo(x).Within(0.000001));
            Assert.That(point.Y, Is.EqualTo(y).Within(0.000001));
        }

        [TestCase(false, true, 8)]
        [TestCase(false, false, -8)]
        [TestCase(true, true, -8)]
        [TestCase(true, false, 8)]
        public void WithStraightTape_ThreePartsDoNotChangeCrossAxis(
            bool vertical, bool forward, double longitudinalDelta)
        {
            var first = At(1, vertical, forward, false);
            var third = At(3, vertical, forward, false);
            Assert.That(vertical ? third.Y - first.Y : third.X - first.X,
                Is.EqualTo(longitudinalDelta).Within(0.000001));
            Assert.That(vertical ? third.X - first.X : third.Y - first.Y,
                Is.EqualTo(0).Within(0.000001));
        }

        [TestCase(false, true)]
        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(true, false)]
        public void MeasuredLateralTapeDrift_AffectsOnlyAcrossAxis(
            bool vertical, bool forward)
        {
            var origin = new Point2D<double>(100, 200);
            var firstHole = new Point2D<double>(0, 3.5);
            var lastHole = new Point2D<double>(116, 2.8);
            var point = StripFeederPickupCalculator.Calculate(
                origin, firstHole, lastHole, vertical, false, forward, 16,
                4, 8, 2, 3.5, false);
            // Part 16 is 62 mm from its feed end, i.e. 62/116 of the
            // measured span. X is along tape; Y is across.
            var fraction = forward ? 62d / 116d : 1d - 62d / 116d;
            var localX = 116 * fraction;
            var localY = 3.5 + (2.8 - 3.5) * fraction + 3.5;
            Assert.That(point.X, Is.EqualTo(vertical ? 100 + localY : 100 + localX)
                .Within(0.000001));
            Assert.That(point.Y, Is.EqualTo(vertical ? 200 - localX : 200 + localY)
                .Within(0.000001));
        }

        [Test]
        public void RealReverseFeedPartTwo_IsCloseToObservedCenteredPosition()
        {
            // Physical feeder run: origin X23.57 Y347.82, measured tape holes
            // (1.6,3.5)->(117.5,2.8), reverse feed, current part index TWO.
            var calculated = StripFeederPickupCalculator.Calculate(
                new Point2D<double>(23.57, 347.82),
                new Point2D<double>(1.6, 3.5),
                new Point2D<double>(117.5, 2.8),
                true, false, false, 2, 4, 8, 2, 3.5, false);

            // Operator centered the 0805 at X29.86 Y236.26. These offsets
            // are assumptions to verify against the actual package settings.
            Assert.That(calculated.X, Is.EqualTo(29.906207).Within(0.000001));
            Assert.That(calculated.Y, Is.EqualTo(236.314828).Within(0.000001));
            Assert.That(calculated.X, Is.EqualTo(29.86).Within(0.10));
            Assert.That(calculated.Y, Is.EqualTo(236.26).Within(0.10));
        }

        [Test]
        public void TwoMillimeterPartPitch_DoesNotChangeSprocketPitch()
        {
            var first = At(1, false, true, false, pitch: 2);
            var third = At(3, false, true, false, pitch: 2);
            Assert.That(third.X - first.X, Is.EqualTo(4).Within(0.000001));
            Assert.That(third.Y - first.Y, Is.EqualTo(0).Within(0.000001));
        }
    }
}
