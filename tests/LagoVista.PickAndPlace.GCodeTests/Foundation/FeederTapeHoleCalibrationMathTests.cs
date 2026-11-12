using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederTapeHoleCalibrationMathTests
    {
        [Test]
        public void HorizontalFirstHoleMustBeEntirelyRightOfLeftFeederEdge()
        {
            var origin = new Point2D<double>(25, 347);
            Assert.That(FeederTapeHoleCalibrationMath.CompletelyBeyondLeadingEdge(
                origin, new Point2D<double>(27, 349), 0.8, false), Is.True);
            Assert.That(FeederTapeHoleCalibrationMath.CompletelyBeyondLeadingEdge(
                origin, new Point2D<double>(25.5, 349), 0.8, false), Is.False);
            Assert.That(FeederTapeHoleCalibrationMath.CompletelyBeyondLeadingEdge(
                origin, new Point2D<double>(24, 349), 0.8, false), Is.False);
        }

        [Test]
        public void VerticalFirstHoleMustBeEntirelyBelowTopFeederEdge()
        {
            var origin = new Point2D<double>(25, 347);
            Assert.That(FeederTapeHoleCalibrationMath.CompletelyBeyondLeadingEdge(
                origin, new Point2D<double>(27, 345), 0.8, true), Is.True);
            Assert.That(FeederTapeHoleCalibrationMath.CompletelyBeyondLeadingEdge(
                origin, new Point2D<double>(27, 346.5), 0.8, true), Is.False);
            Assert.That(FeederTapeHoleCalibrationMath.CompletelyBeyondLeadingEdge(
                origin, new Point2D<double>(27, 349), 0.8, true), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OrientationMappingRoundTripsMeasuredMachineCoordinates(bool vertical)
        {
            var origin = new Point2D<double>(25.90, 347.35);
            var offset = new Point2D<double>(4.0, 2.5);
            var target = FeederTapeHoleCalibrationMath.ToMachinePosition(origin, offset, vertical);
            var restored = FeederTapeHoleCalibrationMath.ToRowOffset(origin, target, vertical);
            Assert.That(restored.X, Is.EqualTo(offset.X).Within(0.001));
            Assert.That(restored.Y, Is.EqualTo(offset.Y).Within(0.001));
            if (vertical)
            {
                Assert.That(target.X, Is.EqualTo(28.40));
                Assert.That(target.Y, Is.EqualTo(343.35));
            }
            else
            {
                Assert.That(target.X, Is.EqualTo(29.90));
                Assert.That(target.Y, Is.EqualTo(349.85));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LastHoleApproachStartsAtMeasuredFirstHole(bool vertical)
        {
            var first = new Point2D<double>(29.90, 343.35);
            Assert.That(FeederTapeHoleCalibrationMath.TryApproximateLastHole(
                first, vertical, 120, 4, out var last), Is.True);
            Assert.That(last.X, Is.EqualTo(vertical ? 29.90 : 145.90).Within(0.001));
            Assert.That(last.Y, Is.EqualTo(vertical ? 227.35 : 343.35).Within(0.001));
        }

        [Test]
        public void InvalidPitchAndPartialCircleCannotBeUsedForCalibration()
        {
            Assert.That(FeederTapeHoleCalibrationMath.TryApproximateLastHole(
                new Point2D<double>(0, 0), false, 120, 0, out _), Is.False);
            Assert.That(FeederTapeHoleCalibrationMath.CompletelyBeyondLeadingEdge(
                new Point2D<double>(0, 0),
                new Point2D<double>(4, 0), double.NaN, false), Is.False);
        }
    }
}
