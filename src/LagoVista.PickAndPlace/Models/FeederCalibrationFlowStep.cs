using LagoVista.Core.Models;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// UI-only procedure step. Source-of-truth evidence remains in the
    /// MachineOperationSession / Manufacturing history records.
    /// </summary>
    public sealed class FeederCalibrationFlowStep : ModelBase
    {
        public int Number { get; set; }
        public int? RowIndex { get; set; }
        public string StepKey { get; set; }
        public string Title { get; set; }

        private string _status = "Queued";
        private string _detail = "Awaiting procedure start";
        private int _retryCount;
        private DateTimeOffset? _startedAt;
        private DateTimeOffset? _finishedAt;

        public string Status { get => _status; set => Set(ref _status, value); }
        public string Detail { get => _detail; set => Set(ref _detail, value); }
        public int RetryCount { get => _retryCount; set => Set(ref _retryCount, value); }
        public DateTimeOffset? StartedAt { get => _startedAt; set { Set(ref _startedAt, value); RaisePropertyChanged(nameof(Duration)); } }
        public DateTimeOffset? FinishedAt { get => _finishedAt; set { Set(ref _finishedAt, value); RaisePropertyChanged(nameof(Duration)); } }
        public string Duration => !StartedAt.HasValue ? "—"
            : ((FinishedAt ?? DateTimeOffset.UtcNow) - StartedAt.Value).TotalSeconds.ToString("F1") + "s";

        public void Begin(string detail)
        {
            if (!StartedAt.HasValue) StartedAt = DateTimeOffset.UtcNow;
            Status = "Running";
            Detail = detail;
        }

        public void Pass(string detail)
        {
            Detail = detail;
            FinishedAt = DateTimeOffset.UtcNow;
            Status = "Passed";
        }

        public void Fail(string detail)
        {
            Detail = detail;
            FinishedAt = DateTimeOffset.UtcNow;
            Status = "Failed";
        }
    }
}
