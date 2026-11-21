using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public sealed class VacuumContactCheckpointTests
    {
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
