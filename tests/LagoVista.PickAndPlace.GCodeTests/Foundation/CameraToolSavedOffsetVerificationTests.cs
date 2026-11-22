using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    [TestFixture]
    public class CameraToolSavedOffsetVerificationTests
    {
        private static Point2D<double> XY(double x, double y) =>
            new Point2D<double>(x, y);

        private static CameraToolPickupObservation Observation(double x, double y,
            DateTimeOffset finished, DateTimeOffset seen) =>
            new CameraToolPickupObservation
            {
                HeadIndex = 1,
                NozzleId = "N14",
                FixtureId = "known-location-7mm-cylinder",
                PartRelativeToNozzleMm = XY(x, y),
                VacuumConfirmed = true,
                StableVision = true,
                NozzleCalibrated = true,
                MotionFinishedUtc = finished,
                CapturedUtc = seen
            };

        private static CameraToolPickupCandidate Candidate(DateTimeOffset now)
        {
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(
                XY(22.7, 65), Observation(0.810, 0.344,
                    now.AddSeconds(-6), now.AddSeconds(-5)),
                now.AddSeconds(-4), out var candidate, out var reason),
                Is.True, reason);
            return candidate;
        }

        [Test]
        public void PersistedThirdPickupAcceptsNozzleRelativeErrorWithinPointTwoMm()
        {
            var now = DateTimeOffset.UtcNow;
            var candidate = Candidate(now);
            var third = Observation(0.114, 0.017,
                now.AddSeconds(-2), now.AddSeconds(-1));
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out var residual, out var error),
                Is.True, error);
            Assert.That(residual, Is.EqualTo(Math.Sqrt(0.114 * 0.114 + 0.017 * 0.017))
                .Within(0.0000001));
            Assert.That(residual, Is.LessThanOrEqualTo(0.2));
        }

        [Test]
        public void SavedOffsetProofRejectsAbovePointTwoEvenIfVisionIsStable()
        {
            var now = DateTimeOffset.UtcNow;
            var candidate = Candidate(now);
            var third = Observation(0.201, 0.0,
                now.AddSeconds(-2), now.AddSeconds(-1));
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out var residual, out var reason),
                Is.False);
            Assert.That(residual, Is.GreaterThan(0.2));
            Assert.That(reason, Does.Contain("0.200 mm"));
        }

        [Test]
        public void SavedOffsetProofRejectsWrongHeadNozzleAndStaleThirdPickup()
        {
            var now = DateTimeOffset.UtcNow;
            var candidate = Candidate(now);
            var third = Observation(0.01, 0.01,
                now.AddSeconds(-2), now.AddSeconds(-1));
            third.NozzleId = "N24";
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out _, out _), Is.False);
            third.NozzleId = "N14";
            third.HeadIndex = 2;
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out _, out _), Is.False);
            third.HeadIndex = 1;
            third.CapturedUtc = now.AddSeconds(-10);
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out _, out _), Is.False);
        }

    }
}
