using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FiducialManualJogPolicyTests
    {
        private static bool Check(double x, double y, double dx, double dy,
            out string reason) =>
            FiducialManualJogPolicy.TryValidate(x, y, dx, dy,
                300, 200, out reason);

        [TestCase(0.1, 0.0)]
        [TestCase(-0.1, 0.0)]
        [TestCase(0.0, 0.5)]
        [TestCase(0.0, -0.5)]
        public void AcceptsOnlySingleAxisOperatorSteps(double dx, double dy)
        {
            Assert.That(Check(100, 100, dx, dy, out var reason), Is.True, reason);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(0.1, 0.1)]
        [TestCase(1.0, 0)]
        [TestCase(0, -5.0)]
        public void RejectsNoopDiagonalAndUnboundedJogs(double dx, double dy)
        {
            Assert.That(Check(100, 100, dx, dy, out _), Is.False);
        }

        [Test]
        public void RejectsPhysicalLimitsWithoutClamping()
        {
            Assert.That(Check(299.95, 100, 0.1, 0, out _), Is.False);
            Assert.That(Check(0.05, 100, -0.1, 0, out _), Is.False);
            Assert.That(Check(100, 199.9, 0, 0.5, out _), Is.False);
        }

        [Test]
        public void InvalidWorkAreaOrPositionNeverAuthorizesJog()
        {
            Assert.That(FiducialManualJogPolicy.TryValidate(
                10, 10, 0.1, 0, double.NaN, 200, out _), Is.False);
            Assert.That(FiducialManualJogPolicy.TryValidate(
                double.PositiveInfinity, 10, 0.1, 0, 300, 200, out _), Is.False);
        }
    }
}
