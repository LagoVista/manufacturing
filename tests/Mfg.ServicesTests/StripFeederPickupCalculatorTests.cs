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
                new Point2D<double>(0, 0), new Point2D<double>(38, 0),
                vertical, holesOnTop, forward, partIndex,
                4, 8, xOffset, yOffset, false);

        [TestCase(false, true, true, 102, 196.5)]
        [TestCase(false, true, false, 102, 203.5)]
        [TestCase(true, true, true, 96.5, 198)]
        [TestCase(true, true, false, 103.5, 198)]
        public void FirstPickup_RespectsOrientationAndHoleSide(bool vertical, bool forward, bool holesOnTop,
            double x, double y)
        {
            // Deliberately records present behavior, including the alignment correction.
            var result = Pick(vertical, forward, holesOnTop);
            Assert.That(result.X, Is.EqualTo(x).Within(0.000001));
            Assert.That(result.Y, Is.EqualTo(y).Within(0.000001));
        }

        [Test]
        public void ComponentIndex_AdvancesByTapePitch_WhenReferencesAreNominal()
        {
            // An exact nominal reference span has no interpolation correction.
            var first = StripFeederPickupCalculator.Calculate(
                new Point2D<double>(100, 200), new Point2D<double>(0, 0),
                new Point2D<double>(38, 0), false, false, true, 1, 4, 8, 2, 3.5, false);
            var third = StripFeederPickupCalculator.Calculate(
                new Point2D<double>(100, 200), new Point2D<double>(0, 0),
                new Point2D<double>(38, 0), false, false, true, 3, 4, 8, 2, 3.5, false);
            Assert.That(third.X - first.X, Is.EqualTo(8).Within(0.000001));
            Assert.That(third.Y, Is.EqualTo(first.Y).Within(0.000001));
        }

        [Test]
        public void ReverseFeed_AnchorsAtLastReferenceHole()
        {
            var result = Pick(false, false, false);
            Assert.That(result.X, Is.EqualTo(136).Within(0.000001));
            Assert.That(result.Y, Is.EqualTo(203.5).Within(0.000001));
        }
    }
}
