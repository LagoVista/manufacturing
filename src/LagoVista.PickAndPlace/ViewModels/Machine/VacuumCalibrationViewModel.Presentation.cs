using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Read-only projection of the authoritative vacuum operation and actual
    /// accepted sensor windows. There is NO additional vacuum sample stream,
    /// actuator command or machine move in the presentation layer.
    /// </summary>
    public sealed partial class VacuumCalibrationViewModel
    {
        private string _leftObserved = "No fresh sensor sample";
        private string _rightObserved = "No fresh sensor sample";
        private DateTimeOffset? _lastLeftSampleUtc;
        private DateTimeOffset? _lastRightSampleUtc;

        public ObservableCollection<FeederCalibrationFlowStep> VacuumSteps { get; private set; }

        public string LeftObserved
        {
            get => _leftObserved;
            private set => Set(ref _leftObserved, value);
        }

        public string RightObserved
        {
            get => _rightObserved;
            private set => Set(ref _rightObserved, value);
        }

        public DateTimeOffset? LastLeftSampleUtc
        {
            get => _lastLeftSampleUtc;
            private set => Set(ref _lastLeftSampleUtc, value);
        }

        public DateTimeOffset? LastRightSampleUtc
        {
            get => _lastRightSampleUtc;
            private set => Set(ref _lastRightSampleUtc, value);
        }

        public string PumpCommands =>
            "Left pump: " + (_machine.LeftVacuumPump ? "ON (commanded)" : "OFF (commanded)") +
            "  |  Right pump: " + (_machine.RightVacuumPump ? "ON (commanded)" : "OFF (commanded)") +
            "  |  Actual vacuum is determined by the sensor samples below.";

        public string StageStatus =>
            "HEAD " + _head + "  ·  " + _phase +
            (_runAllPaused ? "  ·  WAITING FOR OPERATOR" :
                _runAllActive ? "  ·  AUTOMATIC TO NEXT CHECKPOINT" : "");

        public string CalibrationSaveStatus =>
            _calibrationSaved
                ? "SAVED · Both measured vacuum profiles persisted to the machine."
                : _phase == Phase.Finished && _leftCandidate != null && _rightCandidate != null
                    ? "READY FOR REVIEW · Both candidates measured; nothing saved yet."
                    : "UNSAVED · No vacuum thresholds are changed until Apply Calibration.";

        private void InitializeVacuumDisplay()
        {
            VacuumSteps = new ObservableCollection<FeederCalibrationFlowStep>(
                VacuumValidationProcedureDefinition.Create().Steps.Select(
                    (step, index) => new FeederCalibrationFlowStep
                    {
                        Number = index + 1,
                        StepKey = step.Key,
                        Title = step.Title,
                        Detail = step.Instructions,
                        Status = "Queued"
                    }));
            RefreshVacuumDisplay();
        }

        private void RecordAcceptedVacuumBand(int head, VacuumPressureBand band)
        {
            if (band == null) return;
            var summary = band.MeanRawCount.ToString("F1") +
                " raw counts (stable accepted window)";
            if (head == 1)
            {
                LeftObserved = summary;
                LastLeftSampleUtc = DateTimeOffset.UtcNow;
            }
            else if (head == 2)
            {
                RightObserved = summary;
                LastRightSampleUtc = DateTimeOffset.UtcNow;
            }
        }

        private void RefreshVacuumDisplay()
        {
            RaisePropertyChanged(nameof(PumpCommands));
            RaisePropertyChanged(nameof(StageStatus));
            RaisePropertyChanged(nameof(CalibrationSaveStatus));
            if (VacuumSteps == null) return;

            var snapshots = _session?.Snapshot?.Steps;
            foreach (var step in VacuumSteps)
            {
                var state = snapshots?.FirstOrDefault(s => s.Key == step.StepKey);
                if (state == null)
                {
                    step.Status = "Queued";
                    step.StartedAt = null;
                    step.FinishedAt = null;
                    continue;
                }
                step.StartedAt = state.StartedAtUtc;
                step.FinishedAt = state.CompletedAtUtc;
                step.Status = VacuumStepRailProjection.FromOperationStatus(state.Status);
                switch (step.Status)
                {
                    case "Running":
                        step.Detail = "Head " + _head + ": " + _status;
                        break;
                    case "Passed":
                        step.Detail = "Machine/operator evidence accepted for " + step.Title;
                        break;
                    case "Failed":
                        step.Detail = state.Error ?? _session?.Snapshot?.LastError ??
                            "Hardware step blocked without a specific reason. Inspect the red failure detail and machine status.";
                        break;
                    case "Cancelled":
                        step.Detail = "Operator-aborted. Pump-off and safe-Z were requested; " +
                            "verify physical Z and outputs before another run.";
                        break;
                    case "Queued":
                        step.Detail = "Awaiting fresh machine and sensor evidence.";
                        break;
                }
            }
        }
    }
}
