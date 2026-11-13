using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using NUnit.Framework;

namespace Mfg.ServicesTests
{
    [TestFixture]
    public class StripFeederPickupCalculatorTests
    {
        private static Point2D<double> Pick(bool vertical, bool forward, bool holesOnTop,
            int partIndex = 1, double? xOffset = 2, double? yOffset = 3.5)
            => StripFeederPickupCalculator.Calculate(
                new Point2D<double>(100, 200),
                new Point2D<double>(0, 0), new Point2D<double>(40, 0),
                vertical, holesOnTop, forward, partIndex,
                4, 8, xOffset, yOffset, false);

        [TestCase(false, true, true, 102, 196.5)]
        [TestCase(false, true, false, 102, 203.5)]
        [TestCase(true, true, true, 96.5, 198)]
        [TestCase(true, true, false, 103.5, 198)]
        [TestCase(false, false, true, 138, 196.5)]
        [TestCase(false, false, false, 138, 203.5)]
        [TestCase(true, false, true, 96.5, 162)]
        [TestCase(true, false, false, 103.5, 162)]
        public void Pickup_UsesFeederLocalAxesAndDirection(bool vertical, bool forward,
            bool holesOnTop, double x, double y)
        {
            var result = Pick(vertical, forward, holesOnTop);
            Assert.That(result.X, Is.EqualTo(x).Within(0.000001));
            Assert.That(result.Y, Is.EqualTo(y).Within(0.000001));
        }

        [TestCase(false, true, 8)]
        [TestCase(false, false, -8)]
        [TestCase(true, true, -8)]
        [TestCase(true, false, 8)]
        public void ComponentIndex_AdvancesByPartPitchWithoutCrossAxisDrift(
            bool vertical, bool forward, double longitudinalDelta)
        {
            var first = Pick(vertical, forward, false, partIndex: 1);
            var third = Pick(vertical, forward, false, partIndex: 3);
            Assert.That(vertical ? third.Y - first.Y : third.X - first.X,
                Is.EqualTo(longitudinalDelta).Within(0.000001));
            Assert.That(vertical ? third.X - first.X : third.Y - first.Y,
                Is.EqualTo(0).Within(0.000001));
        }

        [Test]
        public void TwoMillimeterPartPitchDoesNotChangeFourMillimeterHoleSpacing()
        {
            var first = StripFeederPickupCalculator.Calculate(
                new Point2D<double>(0, 0), new Point2D<double>(0, 0),
                new Point2D<double>(40, 0), false, false, true,
                1, 2, 8, 2, 3.5, false);
            var third = StripFeederPickupCalculator.Calculate(
                new Point2D<double>(0, 0), new Point2D<double>(0, 0),
                new Point2D<double>(40, 0), false, false, true,
                3, 2, 8, 2, 3.5, false);
            Assert.That(third.X - first.X, Is.EqualTo(4).Within(0.000001));
        }

        [Test]
        public void BothSidedTapeUsesTheExistingAcrossOffsetDefault()
        {
            var pickup = StripFeederPickupCalculator.Calculate(
                new Point2D<double>(0, 0), new Point2D<double>(0, 0),
                new Point2D<double>(40, 0), false, false, true,
                1, 4, 8, null, null, true);
            Assert.That(pickup.X, Is.EqualTo(2).Within(0.000001));
            Assert.That(pickup.Y, Is.EqualTo(2.2).Within(0.000001));
        }
    }
}
