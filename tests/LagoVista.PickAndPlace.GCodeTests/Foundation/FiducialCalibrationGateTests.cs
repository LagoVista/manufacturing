using LagoVista.PickAndPlace.Models;
using LagoVista.Manufacturing.Models;
using NUnit.Framework;
using System;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FiducialCalibrationGateTests
    {
        private static readonly DateTimeOffset Now =
            new DateTimeOffset(2026, 11, 21, 17, 45, 0, TimeSpan.Zero);

        private static FiducialCircleObservation Centered() =>
            new FiducialCircleObservation
            {
                CameraType = CameraTypes.Position,
                ProfileKey = "macfiducial",
                ObservedAtUtc = Now.AddMilliseconds(-50),
                FrameAtUtc = Now.AddMilliseconds(-60),
                FrameWidth = 480,
                FrameHeight = 480,
                PixelsPerMillimeter = 100,
                CenterXpx = 240,
                CenterYpx = 240,
                RadiusPx = 95,
                RadiusMm = 0.95,
                OffsetXpx = 0.5,
                OffsetYpx = -0.5,
                OffsetXmm = 0,
                OffsetYmm = 0,
                Stabilized = true,
                PositionStable = true,
                Centered = true,
                ErrorToleranceMm = 0.1,
                FoundCount = 9
            };

        private static bool Qualify(FiducialCircleObservation o, out string reason) =>
            FiducialCalibrationGate.TryQualify(o, "macfiducial", 100,
                Now.AddSeconds(-1), Now, out reason);

        private static bool Commit(FiducialCircleObservation o, out string reason) =>
            FiducialCalibrationGate.CanCommitOrigin(o, "macfiducial", 100,
                Now.AddSeconds(-1), Now, out reason);

        [Test]
        public void FreshStableCenteredHoughObservationCanResetMachineOrigin()
        {
            Assert.That(Commit(Centered(), out var reason), Is.True, reason);
        }

        [Test]
        public void MinimumCountAloneDoesNotEstablishPositionStability()
        {
            var o = Centered();
            o.PositionStable = false;
            Assert.That(Qualify(o, out var reason), Is.False);
            Assert.That(reason, Does.Contain("not stable"));
        }

        [Test]
        public void CenteredFlagWithoutStabilizedCountCannotResetOrigin()
        {
            var o = Centered();
            o.Stabilized = false;
            Assert.That(Commit(o, out _), Is.False);
        }

        [Test]
        public void StableOffCenterCandidateMayMoveButCannotResetOrigin()
        {
            var o = Centered();
            o.Centered = false;
            o.OffsetXpx = 13.0;
            o.OffsetXmm = 0.1;
            Assert.That(Qualify(o, out _), Is.True);
            Assert.That(Commit(o, out var reason), Is.False);
            Assert.That(reason, Does.Contain("outside centering tolerance"));
        }

        [Test]
        public void FlaggedCenteredCannotBypassNumericTolerance()
        {
            var o = Centered();
            o.OffsetXpx = 15.0; // 0.15 mm at 100 px/mm
            Assert.That(Commit(o, out _), Is.False);
        }

        [Test]
        public void OldFrameCannotQualifyEvenWithNewCallback()
        {
            var o = Centered();
            o.FrameAtUtc = Now.AddSeconds(-4);
            Assert.That(Qualify(o, out var reason), Is.False);
            Assert.That(reason, Does.Contain("predates last completed move"));
        }

        [Test]
        public void TwoSecondFreshnessLimitRejectsOldDetection()
        {
            var o = Centered();
            o.FrameAtUtc = Now.AddMilliseconds(-2200);
            o.ObservedAtUtc = Now.AddMilliseconds(-2200);
            Assert.That(FiducialCalibrationGate.TryQualify(o, "macfiducial",
                100, Now.AddSeconds(-5), Now, out var reason), Is.False);
            Assert.That(reason, Does.Contain("Stale"));
        }

        [Test]
        public void ActiveVisionProfileChangeInvalidatesCandidate()
        {
            var o = Centered();
            o.ProfileKey = "brdfiducial";
            Assert.That(Qualify(o, out var reason), Is.False);
            Assert.That(reason, Does.Contain("profile"));
        }

        [Test]
        public void BottomCameraCircleCannotAuthorizeMachineOriginReset()
        {
            var o = Centered();
            o.CameraType = CameraTypes.PartInspection;
            Assert.That(Commit(o, out var reason), Is.False);
            Assert.That(reason, Does.Contain("different camera"));
        }

        [Test]
        public void PixelScaleChangeInvalidatesCachedCircle()
        {
            var o = Centered();
            o.PixelsPerMillimeter = 90;
            Assert.That(Qualify(o, out var reason), Is.False);
            Assert.That(reason, Does.Contain("scale"));
        }

        [Test]
        public void InvalidCenterAndRadiusAreNeverAccepted()
        {
            var o = Centered();
            o.RadiusPx = double.NaN;
            Assert.That(Commit(o, out _), Is.False);
            o = Centered();
            o.CenterXpx = double.PositiveInfinity;
            Assert.That(Commit(o, out _), Is.False);
        }

        [Test]
        public void FutureCameraFrameIsNotEligibleEvidence()
        {
            var o = Centered();
            o.FrameAtUtc = Now.AddMilliseconds(15);
            Assert.That(Qualify(o, out var reason), Is.False);
            Assert.That(reason, Does.Contain("Stale"));
        }

        [Test]
        public void MissingCircleNeverBecomesAnImplicitSuccess()
        {
            Assert.That(Commit(null, out var reason), Is.False);
            Assert.That(reason, Does.Contain("No circle"));
        }

        [Test]
        public void ZeroOrNegativeCenterToleranceCannotResetOrigin()
        {
            var o = Centered();
            o.ErrorToleranceMm = 0;
            Assert.That(Commit(o, out _), Is.False);
        }
    }
}
