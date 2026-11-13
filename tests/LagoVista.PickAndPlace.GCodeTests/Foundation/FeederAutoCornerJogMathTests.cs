using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederAutoCornerJogMathTests
    {
        [TestCase(25.93, 347.39, -2.42, 0.29, true, 23.51, 347.68)]
        [TestCase(25.93, 347.39, 10, 0, true, 35.93, 347.39)]
        [TestCase(25.93, 347.39, -10.01, 0, false, 0, 0)]
        [TestCase(25.93, 347.39, 0, 10.01, false, 0, 0)]
        [TestCase(1, 20, -2.42, 0, false, 0, 0)]
        [TestCase(25.93, 399, 0, 2, false, 0, 0)]
        public void RestrictsJogBeforeCallingSecondPlay(
            double x, double y, double dx, double dy,
            bool allowed, double expectedX, double expectedY)
        {
            var accepted = FeederAutoCornerJogMath.TryTarget(
                new Point2D<double>(x, y), dx, dy,
                new Point2D<double>(400, 400), out var target);
            Assert.That(accepted, Is.EqualTo(allowed));
            if (allowed)
            {
                Assert.That(target.X, Is.EqualTo(expectedX).Within(0.0001));
                Assert.That(target.Y, Is.EqualTo(expectedY).Within(0.0001));
            }
            else
                Assert.That(target, Is.Null);
        }

        [Test]
        public void RejectsNonFiniteMeasurements()
        {
            Assert.That(FeederAutoCornerJogMath.TryTarget(
                new Point2D<double>(double.NaN, 1), 1, 1,
                new Point2D<double>(400, 400), out _), Is.False);
            Assert.That(FeederAutoCornerJogMath.TryTarget(
                new Point2D<double>(10, 10), double.PositiveInfinity, 0,
                new Point2D<double>(400, 400), out _), Is.False);
        }
    }
}
