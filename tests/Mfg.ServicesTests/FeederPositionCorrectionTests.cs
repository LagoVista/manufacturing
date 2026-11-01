using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using NUnit.Framework;

namespace Mfg.ServicesTests
{
    [TestFixture]
    public class FeederPositionCorrectionTests
    {
        [Test]
        public void NoMountingError_DoesNotMovePickPosition()
        {
            var result = FeederPositionCorrection.Apply(
                new Point2D<double>(40, 0), new Point2D<double>(40, 0),
                0.5, new Point2D<double>(120, 70));
            Assert.That(result.X, Is.EqualTo(120).Within(0.000001));
            Assert.That(result.Y, Is.EqualTo(70).Within(0.000001));
        }

        [TestCase(0, 120, 70)]
        [TestCase(0.5, 121, 69)]
        [TestCase(1, 122, 68)]
        public void CorrectsLinearlyAcrossFeeder(double ratio, double expectedX, double expectedY)
        {
            var result = FeederPositionCorrection.Apply(
                new Point2D<double>(38, 2), new Point2D<double>(40, 0),
                ratio, new Point2D<double>(120, 70));
            Assert.That(result.X, Is.EqualTo(expectedX).Within(0.000001));
            Assert.That(result.Y, Is.EqualTo(expectedY).Within(0.000001));
        }

        [Test]
        public void ReversedFeed_UsesCallerAdjustedRatio()
        {
            var result = FeederPositionCorrection.Apply(
                new Point2D<double>(38, 2), new Point2D<double>(40, 0),
                1 - 0.25, new Point2D<double>(120, 70));
            Assert.That(result.X, Is.EqualTo(121.5).Within(0.000001));
            Assert.That(result.Y, Is.EqualTo(68.5).Within(0.000001));
        }
    }
}
