using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederCalibrationAutoAdvancePolicyTests
    {
        [TestCase(true, false, 0, 0, true)]
        [TestCase(true, true, 0, 0, false)]
        [TestCase(true, false, 1, 0, false)]
        [TestCase(true, false, 0, 1, false)]
        [TestCase(false, false, 0, 0, false)]
        public void SerialChannelMustSettleAfterM400(
            bool connected, bool busy, int pending, int unacknowledged, bool expected)
        {
            Assert.That(FeederCalibrationAutoAdvancePolicy.SerialChannelQuiet(
                connected, busy, pending, unacknowledged), Is.EqualTo(expected));
        }

        [TestCase(MachineOperationRunStatus.Pending, null, false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "approach-origin", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "center-fiducial", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "first-tape-hole", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "first-tape-hole", true, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "last-tape-hole", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "last-tape-hole", true, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "save-origin", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "verify-current-part", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "verify-current-part", false, true, true)]
        [TestCase(MachineOperationRunStatus.Running, "verify-previous-empty", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "verify-previous-empty", false, true, true)]
        [TestCase(MachineOperationRunStatus.Blocked, "first-tape-hole", false, false, false)]
        [TestCase(MachineOperationRunStatus.Completed, null, false, false, false)]
        [TestCase(MachineOperationRunStatus.Cancelled, "verify-current-part", false, false, false)]
        public void AutoAdvanceCommitsVerifiedOffsetsButNotPartObservations(
            MachineOperationRunStatus status, string activeStepKey,
            bool holeApproached, bool pocketApproached, bool expected)
        {
            Assert.That(FeederCalibrationAutoAdvancePolicy.CanAdvance(
                status, activeStepKey, holeApproached, pocketApproached),
                Is.EqualTo(expected));
        }
    }
}
