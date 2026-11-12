using System;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models.Operations
{
    // Data contracts only. A concrete C# operation runner owns the physical behavior.
    public enum MachineOperationRunStatus
    {
        Pending,
        Running,
        Blocked,
        Completed,
        Cancelled
    }

    public enum MachineOperationStepStatus
    {
        Pending,
        Active,
        Passed,
        Blocked,
        Cancelled
    }

    public enum MachineOperationVerificationKind
    {
        None,
        MachineObserved,
        VisionVerified,
        OperatorConfirmed
    }

    public enum MachineOperationAction
    {
        Start,
        Continue,
        Confirm,
        Retry,
        Cancel
    }

    public class MachineOperationStepDefinition
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Instructions { get; set; }
        public MachineOperationVerificationKind RequiredVerification { get; set; }
        public bool RequiresSavedEvidence { get; set; }
    }

    public class MachineOperationDefinition
    {
        public int SchemaVersion { get; set; } = 1;
        public string Key { get; set; }
        public int Version { get; set; } = 1;
        public string Title { get; set; }
        public List<MachineOperationStepDefinition> Steps { get; set; } =
            new List<MachineOperationStepDefinition>();
    }

    public class MachineOperationTarget
    {
        public string MachineId { get; set; }
        public string FeederId { get; set; }
        public int? RowIndex { get; set; }
        public int? StartingPartIndex { get; set; }
        public string CameraId { get; set; }
    }

    /// <summary>
    /// A measurement/evidence receipt, not an instruction to move a machine.
    /// Saved means the authoritative setting write was confirmed, not just measured.
    /// </summary>
    public class MachineOperationEvidence
    {
        public string RunId { get; set; }
        public string StepKey { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public string Source { get; set; }
        public MachineOperationVerificationKind Verification { get; set; }
        public bool Saved { get; set; }
        public DateTimeOffset CapturedAtUtc { get; set; }
        public string VisionProfileKey { get; set; }
        public double? PixelsPerMm { get; set; }
    }

    public class MachineOperationStepSnapshot
    {
        public string Key { get; set; }
        public MachineOperationStepStatus Status { get; set; }
        public DateTimeOffset? StartedAtUtc { get; set; }
        public DateTimeOffset? CompletedAtUtc { get; set; }
        public string Error { get; set; }
        public List<MachineOperationEvidence> Evidence { get; set; } =
            new List<MachineOperationEvidence>();
    }

    public class MachineOperationRunSnapshot
    {
        public int SchemaVersion { get; set; } = 1;
        public string RunId { get; set; }
        public string DefinitionKey { get; set; }
        public int DefinitionVersion { get; set; }
        public MachineOperationTarget Target { get; set; }
        public MachineOperationRunStatus Status { get; set; }
        public string ActiveStepKey { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset? CompletedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public string LastError { get; set; }
        public List<MachineOperationStepSnapshot> Steps { get; set; } =
            new List<MachineOperationStepSnapshot>();
    }

    public class MachineOperationActionResult
    {
        public bool Successful { get; set; }
        public string ErrorMessage { get; set; }
        public MachineOperationRunSnapshot Snapshot { get; set; }
    }
}
