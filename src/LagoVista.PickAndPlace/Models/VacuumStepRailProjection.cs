using LagoVista.Manufacturing.Models.Operations;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Presentation-only mapping for the authoritative machine operation
    /// states; never manufactures evidence or marks a command as success.
    /// </summary>
    public static class VacuumStepRailProjection
    {
        public static string FromOperationStatus(MachineOperationStepStatus status)
        {
            switch (status)
            {
                case MachineOperationStepStatus.Pending: return "Queued";
                case MachineOperationStepStatus.Active: return "Running";
                case MachineOperationStepStatus.Passed: return "Passed";
                case MachineOperationStepStatus.Blocked: return "Failed";
                case MachineOperationStepStatus.Cancelled: return "Cancelled";
                default: return "Queued";
            }
        }
    }
}
