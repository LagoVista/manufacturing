using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public sealed class VacuumContactCheckpointTests
    {
        [Test]
        public void AcknowledgedMachineIsNotPermissionToDescend()
        {
            Assert.That(VacuumContactCheckpoint.CanDescend(1, 0, true), Is.False);
            Assert.That(VacuumContactCheckpoint.CanDescend(2, 0, true), Is.False);
        }

        [Test]
        public void EachHeadNeedsItsOwnClearance()
        {
            Assert.That(VacuumContactCheckpoint.CanDescend(1, 1, true), Is.True);
            Assert.That(VacuumContactCheckpoint.CanDescend(2, 1, true), Is.False);
            Assert.That(VacuumContactCheckpoint.CanDescend(1, 2, true), Is.False);
            Assert.That(VacuumContactCheckpoint.CanDescend(2, 2, true), Is.True);
        }

        [Test]
        public void MissingGeneralAcknowledgementBlocksBothHeads()
        {
            Assert.That(VacuumContactCheckpoint.CanDescend(1, 1, false), Is.False);
            Assert.That(VacuumContactCheckpoint.CanDescend(2, 2, false), Is.False);
        }

        [Test]
        public void SealedSensorEvidenceNeedsFreshHeadSpecificContactConfirmation()
        {
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(1, 1), Is.True);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(2, 1), Is.False);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(2, 2), Is.True);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(1, 0), Is.False);
        }

        [Test]
        public void InvalidHeadCannotBypassGate()
        {
            Assert.That(VacuumContactCheckpoint.CanDescend(3, 3, true), Is.False);
            Assert.That(VacuumContactCheckpoint.CanCaptureSealed(0, 0), Is.False);
        }
    }
}
