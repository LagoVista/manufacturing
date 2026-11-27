using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class CameraToolBottomCameraFramingTests
    {
        private static Point2D<double> P(double x, double y) =>
            new Point2D<double>(x, y);

        [Test]
        public void MeasuredCameraResponseCentersOffsetNozzleWithoutWritingHeadOffset()
        {
            var initial = P(-0.555, -1.689);
            Assert.That(CameraToolBottomCameraFraming.TrySolve(
                initial, P(0.245, -1.689), P(-0.555, -0.889), 0.8,
                out var framing, out var error), Is.True, error);
            Assert.That(framing.X, Is.EqualTo(0.555).Within(0.000001));
            Assert.That(framing.Y, Is.EqualTo(1.689).Within(0.000001));

            var fixedHeadOffset = P(23.447, 65.325);
            Assert.That(CameraToolBottomCameraFraming.TryValidateInspectionXY(
                P(145, 120), fixedHeadOffset, framing, P(400, 400),
                out var cameraTarget, out error), Is.True, error);
            Assert.That(cameraTarget.X, Is.EqualTo(145.555).Within(0.000001));
            Assert.That(cameraTarget.Y, Is.EqualTo(121.689).Within(0.000001));
            Assert.That(fixedHeadOffset.X, Is.EqualTo(23.447),
                "The temporary camera framing move must not mutate saved tool-head calibration.");
        }

        [Test]
        public void SharedCameraUsesOnlyHeadOneCorrectionAndChecksBothTravelEnvelopes()
        {
            var headOne = P(23, 65);
            var headTwo = P(-23, 65);
            var previous = P(145, 120);
            Assert.That(CameraToolBottomCameraFraming.TryProposeSharedCamera(previous,
                headOne, headTwo, P(0.555, 1.689), P(400, 400), out var candidate, out var reason), Is.True, reason);
            Assert.That(candidate.X, Is.EqualTo(145.555).Within(0.000001));
            Assert.That(candidate.Y, Is.EqualTo(121.689).Within(0.000001));
            Assert.That(previous.X, Is.EqualTo(145));
            Assert.That(headOne.X, Is.EqualTo(23));
            Assert.That(headTwo.X, Is.EqualTo(-23));
            Assert.That(CameraToolBottomCameraFraming.TryProposeSharedCamera(P(25, 120),
                headOne, headTwo, P(0, 0), P(400, 400), out candidate, out _), Is.False);
            Assert.That(candidate, Is.Null);
        }

        [TestCase(0.2, 0.2, true)]
        [TestCase(0.01, 0.201, false)]
        [TestCase(0.201, 0.01, false)]
        [TestCase(double.NaN, 0.01, false)]
        [TestCase(0.01, double.PositiveInfinity, false)]
        public void IndependentUncorrectedProofCannotAverageOrHideFailures(double one, double two, bool expected)
        {
            Assert.That(CameraToolBottomCameraFraming.BothHeadsPassUncorrected(one, two), Is.EqualTo(expected));
        }

        [Test]
        public void CameraOrientationCanBeMirroredAndRotatedWithoutGuessingAxisSigns()
        {
            var initial = P(-0.555, -1.689);
            // Observed J = [ 0 -1 ; +1 0 ], a 90 degree rotation.
            Assert.That(CameraToolBottomCameraFraming.TrySolve(
                initial, P(-0.555, -0.889), P(-1.355, -1.689), 0.8,
                out var framing, out var error), Is.True, error);
            Assert.That(framing.X, Is.EqualTo(1.689).Within(0.000001));
            Assert.That(framing.Y, Is.EqualTo(-0.555).Within(0.000001));
        }

        [Test]
        public void UnreliableProbeAndTravelPastPhysicalMarginFailClosed()
        {
            Assert.That(CameraToolBottomCameraFraming.TrySolve(
                P(-0.555, -1.689), P(-0.55, -1.68), P(-0.56, -1.67), 0.8,
                out _, out var reason), Is.False);
            Assert.That(reason, Does.Contain("trustworthy"));

            Assert.That(CameraToolBottomCameraFraming.TryValidateInspectionXY(
                P(2, 120), P(22, 65), P(0, 0), P(400, 400),
                out _, out reason), Is.True, reason); // Physical X = 24.
            Assert.That(CameraToolBottomCameraFraming.TryValidateInspectionXY(
                P(2, 120), P(0, 65), P(0, 0), P(400, 400),
                out _, out reason), Is.False);
            Assert.That(reason, Does.Contain("physical XY"));
        }

        [Test]
        public void GreenRequiresCameraAndNozzleCenteringNotMerelyPartMinusNozzle()
        {
            // This reproduces the already recorded 0.017mm HEAD1 measurement:
            // pickup perfect but both disc and nozzle 1.78mm off camera center.
            Assert.That(CameraToolBottomCameraFraming.TryVerifyCenteredPickup(
                P(-0.555, -1.689), P(-0.544, -1.701),
                out var nozzle, out var disc, out var error), Is.False);
            Assert.That(nozzle, Is.GreaterThan(1.7));
            Assert.That(disc, Is.GreaterThan(1.7));
            Assert.That(error, Does.Contain("Nozzle"));

            // Small nozzle offset, very good pickup (0.17mm relative error),
            // but disc outside 0.2mm of camera image center: still not green.
            Assert.That(CameraToolBottomCameraFraming.TryVerifyCenteredPickup(
                P(0.05, 0.01), P(0.22, 0.03),
                out nozzle, out disc, out error), Is.False);
            Assert.That(error, Does.Contain("Picked disc"));

            Assert.That(CameraToolBottomCameraFraming.TryVerifyCenteredPickup(
                P(0.04, -0.07), P(0.09, -0.10),
                out nozzle, out disc, out error), Is.True, error);
            Assert.That(nozzle, Is.LessThanOrEqualTo(0.20));
            Assert.That(disc, Is.LessThanOrEqualTo(0.20));
        }
    }
}
