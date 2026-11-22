using LagoVista.Core;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Uses the existing authenticated Manufacturing calibration history API.
    /// Never modifies a machine setting and never controls movement.
    /// </summary>
    public sealed partial class CameraToolPickupCalibrationViewModel
    {
        private readonly IRestClient _restClient;
        private MachineOperationRunSnapshot _pendingHistorySnapshot;
        private MachineOperationDefinition _pendingHistoryDefinition;
        private MachineOperationRunSnapshot _lastHistorySnapshot;
        private bool _historyBusy;
        private string _historyStatus =
            "Run evidence will be saved after the machine settles.";
        private string _historyRunId = "No run yet";
        private string _lastBottomOpticalEvidence;

        public string HistoryStatus
        {
            get => _historyStatus;
            private set => Set(ref _historyStatus, value);
        }

        public string HistoryRunId
        {
            get => _historyRunId;
            private set => Set(ref _historyRunId, value);
        }

        public bool HistoryBusy
        {
            get => _historyBusy;
            private set
            {
                Set(ref _historyBusy, value);
                RaisePropertyChanged(nameof(CanOperate));
                RaisePropertyChanged(nameof(CanRetryHistory));
            }
        }

        public bool CanRetryHistory =>
            !CalibrationRunning && !HistoryBusy && _pendingHistorySnapshot != null;

        // Offline diagnostic escape hatch when Manufacturing REST is unavailable.
        public string DiagnosticReport
        {
            get
            {
                var b = new StringBuilder();
                b.AppendLine("T014 - Camera-to-Tool XY / two-pick verification");
                b.AppendLine("Run ID: " + HistoryRunId);
                b.AppendLine("History receipt: " + HistoryStatus);
                b.AppendLine("Procedure: " + Status);
                b.AppendLine("Candidate: " + CandidateSummary);
                b.AppendLine("Bottom vision: " + BottomVisionState);
                b.AppendLine("Bottom measurements: " + BottomVisionMeasurements);
                var snapshot = _lastHistorySnapshot;
                if (snapshot != null)
                {
                    b.AppendLine("Run status: " + snapshot.Status +
                        "; active step: " + snapshot.ActiveStepKey);
                    b.AppendLine("Machine ID: " + snapshot.Target?.MachineId);
                    b.AppendLine("Last error/recovery: " + snapshot.LastError);
                    foreach (var step in snapshot.Steps)
                    {
                        b.AppendLine(step.Key + " [" + step.Status + "] " +
                            (step.Error ?? String.Empty));
                        foreach (var evidence in step.Evidence)
                            b.AppendLine("  " + evidence.CapturedAtUtc.ToString("O") +
                                " [" + evidence.Verification + "] " +
                                evidence.Value + " Saved=" + evidence.Saved);
                    }
                }
                return b.ToString();
            }
        }

        private void NewHistorySession(MachineOperationSession session)
        {
            HistoryRunId = session.Snapshot.RunId;
            HistoryStatus = "Collecting run evidence; not saved remotely yet.";
            _lastBottomOpticalEvidence = null;
            RaisePropertyChanged(nameof(DiagnosticReport));
        }

        private void CaptureBottomOpticalEvidence(string profile, double diameter,
            double x, double y, string diagnostics)
        {
            _lastBottomOpticalEvidence =
                "profile=" + profile +
                "; diameterMm=" + diameter.ToString("F3") +
                "; offsetXmm=" + x.ToString("F4") +
                "; offsetYmm=" + y.ToString("F4") +
                "; detector=" + diagnostics;
        }

        // A failed acquisition is Blocked, not Completed. Likewise, two
        // successful pickups with the final offset intentionally UNSAVED need
        // an explicit approval hold, not a fictitious saved receipt.
        private void BlockHistoryStep(string reason)
        {
            if (_session?.Snapshot.Status == MachineOperationRunStatus.Running)
                _session.BlockActiveStep(reason, DateTimeOffset.UtcNow);
        }

        private async Task SaveSettledHistoryAsync()
        {
            if (_session == null)
                return;

            try
            {
                // Immutable, idempotent retry payload. Retain it after failures.
                var snapshot = JsonConvert.DeserializeObject<MachineOperationRunSnapshot>(
                    JsonConvert.SerializeObject(_session.Snapshot));
                _lastHistorySnapshot = snapshot;
                _pendingHistorySnapshot = snapshot;
                _pendingHistoryDefinition = _session.Definition;
                RaisePropertyChanged(nameof(CanRetryHistory));
                RaisePropertyChanged(nameof(DiagnosticReport));
                await RetryHistoryAsync();
            }
            catch (Exception ex)
            {
                HistoryStatus = "Unable to prepare history: " + ex.Message +
                    ". Copy Run Report before closing.";
                _repo.CurrentMachine?.AddStatusMessage(
                    StatusMessageTypes.Warning, HistoryStatus);
            }
        }

        public async Task RetryHistoryAsync()
        {
            if (HistoryBusy || CalibrationRunning ||
                _pendingHistorySnapshot == null || _pendingHistoryDefinition == null)
                return;

            var snapshot = _pendingHistorySnapshot;
            var definition = _pendingHistoryDefinition;
            HistoryBusy = true;
            HistoryStatus = "Saving run " + snapshot.RunId + " to Manufacturing...";
            try
            {
                var result = await _restClient.PutAsync("/api/mfg/calibration/run",
                    new { Snapshot = snapshot, Definition = definition }, waitCursor: false);
                if (result.Successful)
                {
                    if (ReferenceEquals(_pendingHistorySnapshot, snapshot))
                    {
                        _pendingHistorySnapshot = null;
                        _pendingHistoryDefinition = null;
                    }
                    HistoryStatus = "RECORDED " + snapshot.RunId + " (" + snapshot.Status +
                        "). " + (snapshot.ActiveStepKey == "commit-offset"
                            ? "Both pickups measured; offset intentionally UNSAVED."
                            : "Available through Manufacturing run history.");
                }
                else
                {
                    HistoryStatus = "NOT SAVED: " + result.ErrorMessage +
                        ". Retry History Save or Copy Run Report.";
                    _repo.CurrentMachine?.AddStatusMessage(
                        StatusMessageTypes.Warning, HistoryStatus);
                }
            }
            catch (Exception ex)
            {
                HistoryStatus = "NOT SAVED: " + ex.Message +
                    ". Retry History Save or Copy Run Report.";
                _repo.CurrentMachine?.AddStatusMessage(
                    StatusMessageTypes.Warning, HistoryStatus);
            }
            finally
            {
                HistoryBusy = false;
                RaisePropertyChanged(nameof(CanRetryHistory));
                RaisePropertyChanged(nameof(DiagnosticReport));
            }
        }
    }
}
