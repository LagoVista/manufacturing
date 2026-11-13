using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederCalibrationAutoAdvancePolicyTests
    {
        [TestCase(MachineOperationRunStatus.Pending, null, false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "approach-origin", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "center-fiducial", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "first-tape-hole", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "first-tape-hole", true, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "last-tape-hole", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "last-tape-hole", true, false, false)]
        [TestCase(MachineOperationRunStatus.Running, "save-origin", false, false, false)]
        [TestCase(MachineOperationRunStatus.Running, "verify-current-part", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "verify-current-part", false, true, false)]
        [TestCase(MachineOperationRunStatus.Running, "verify-previous-empty", false, false, true)]
        [TestCase(MachineOperationRunStatus.Running, "verify-previous-empty", false, true, false)]
        [TestCase(MachineOperationRunStatus.Blocked, "first-tape-hole", false, false, false)]
        [TestCase(MachineOperationRunStatus.Completed, null, false, false, false)]
        [TestCase(MachineOperationRunStatus.Cancelled, "verify-current-part", false, false, false)]
        public void AutoAdvanceNeverCrossesOperatorConfirmation(
            MachineOperationRunStatus status, string activeStepKey,
            bool holeApproached, bool pocketApproached, bool expected)
        {
            Assert.That(FeederCalibrationAutoAdvancePolicy.CanAdvance(
                status, activeStepKey, holeApproached, pocketApproached),
                Is.EqualTo(expected));
        }
    }
}
