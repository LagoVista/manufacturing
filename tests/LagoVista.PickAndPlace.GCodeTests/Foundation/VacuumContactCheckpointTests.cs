using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public sealed class VacuumContactCheckpointTests
    {
        [Test]
        public void RunBothAutomaticallySamplesEachInstalledHeadWithoutContactCheckbox()
        {
            // This is the original T007 auto-run contract: obtaining sensor
            // evidence is allowed, but calibration is accepted only after
            // VacuumCalibrationCandidate.TryCreate validates the samples.
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(1, 0, true), Is.True);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(2, 0, true), Is.True);
        }

        [Test]
        public void GuidedStepStillRequiresFreshHeadSpecificOperatorProof()
        {
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(1, 0, false), Is.False);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(2, 1, false), Is.False);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(2, 2, false), Is.True);
        }

        [Test]
        public void AutomaticModeDoesNotPermitUnconfiguredHeadNumbers()
        {
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(0, 0, true), Is.False);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(3, 3, true), Is.False);
        }

        [Test]
        public void HeadOneSealProofCannotBeReusedByHeadTwo()
        {
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(1, 1), Is.True);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(2, 1), Is.False);
        }

        [Test]
        public void EachHeadMayProveOnlyItsOwnContact()
        {
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(2, 2), Is.True);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(1, 2), Is.False);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(1, 0), Is.False);
        }

        [Test]
        public void InvalidHeadCannotClaimSealedPressureEvidence()
        {
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(0, 0), Is.False);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(3, 3), Is.False);
        }
    }
}
