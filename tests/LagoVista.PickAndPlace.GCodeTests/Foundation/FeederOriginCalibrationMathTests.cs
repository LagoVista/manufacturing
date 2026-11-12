using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederOriginCalibrationMathTests
    {
        [Test]
        public void CapturedOffsetIsMeasuredMinusCalculatedOriginNotOldOffset()
        {
            Assert.That(FeederOriginCalibrationMath.TryCalculate(
                new Point2D<double>(-164.8, -192.0),
                new Point2D<double>(-165.16, -192.44),
                90.9, true, true, out var result), Is.True);
            Assert.That(result.X, Is.EqualTo(-0.36));
            Assert.That(result.Y, Is.EqualTo(-0.44));
        }

        [Test]
        public void OnlyStableAndCenteredCameraObservationsMayAuthorizeOffset()
        {
            var baseline = new Point2D<double>(50, 70);
            var observed = new Point2D<double>(50.25, 70.25);
            Assert.That(FeederOriginCalibrationMath.TryCalculate(
                baseline, observed, 90.9, false, true, out _), Is.False);
            Assert.That(FeederOriginCalibrationMath.TryCalculate(
                baseline, observed, 90.9, true, false, out _), Is.False);
            Assert.That(FeederOriginCalibrationMath.TryCalculate(
                baseline, observed, 0, true, true, out _), Is.False);
        }

        [Test]
        public void InvalidMeasurementsAreRejected()
        {
            var baseline = new Point2D<double>(0, 0);
            Assert.That(FeederOriginCalibrationMath.TryCalculate(
                baseline, new Point2D<double>(double.NaN, 2),
                90.9, true, true, out _), Is.False);
            Assert.That(FeederOriginCalibrationMath.TryCalculate(
                baseline, new Point2D<double>(0, 2),
                double.PositiveInfinity, true, true, out _), Is.False);
        }

        [Test]
        public void FirstApproachAcceptsFreshlyRecordedCameraBaseline()
        {
            // Regression: the step-1 check must use the captured baseline,
            // not zero/null defaults from before profile selection.
            Assert.That(FeederOriginCalibrationMath.CameraProfileUnchanged(
                "position-camera", 90.9,
                "position-camera", 90.9, "feederorigin", "feederorigin"), Is.True);
            Assert.That(FeederOriginCalibrationMath.CameraProfileUnchanged(
                null, 0, "position-camera", 90.9, "feederorigin", "feederorigin"), Is.False);
        }

        [Test]
        public void CameraGuardRejectsProfileCameraAndScaleChanges()
        {
            Assert.That(FeederOriginCalibrationMath.CameraProfileUnchanged(
                "camera-a", 90.9, "camera-b", 90.9, "feederorigin", "feederorigin"), Is.False);
            Assert.That(FeederOriginCalibrationMath.CameraProfileUnchanged(
                "camera-a", 90.9, "camera-a", 90.9, "default", "feederorigin"), Is.False);
            Assert.That(FeederOriginCalibrationMath.CameraProfileUnchanged(
                "camera-a", 90.9, "camera-a", 91.0, "feederorigin", "feederorigin"), Is.False);
            Assert.That(FeederOriginCalibrationMath.CameraProfileUnchanged(
                "camera-a", 90.9, "camera-a", double.NaN, "feederorigin", "feederorigin"), Is.False);
        }

        [Test]
        public void PostObservationMovementInvalidatesPositionProof()
        {
            var captured = new Point2D<double>(25.50, 40.20);
            Assert.That(FeederOriginCalibrationMath.SamePosition(
                captured, new Point2D<double>(25.53, 40.19), 0.10), Is.True);
            Assert.That(FeederOriginCalibrationMath.SamePosition(
                captured, new Point2D<double>(25.65, 40.19), 0.10), Is.False);
            Assert.That(FeederOriginCalibrationMath.SamePosition(
                captured, new Point2D<double>(25.50, double.NaN), 0.10), Is.False);
        }
    }
}
