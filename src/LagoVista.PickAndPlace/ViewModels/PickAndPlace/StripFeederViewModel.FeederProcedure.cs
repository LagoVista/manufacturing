using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Operations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public partial class StripFeederViewModel
    {
        private bool _feederFlowRunning;
        private bool _feederFlowFinished;
        private string _feederFlowStatus = "Ready to calibrate the selected feeder.";
        private int _feederFlowRunAttempt;

        public bool FeederFlowRunning
        {
            get => _feederFlowRunning;
            private set { Set(ref _feederFlowRunning, value); RaisePropertyChanged(nameof(FeederFlowFinished)); }
        }

        public bool FeederFlowFinished
        {
            get => _feederFlowFinished;
            private set => Set(ref _feederFlowFinished, value);
        }

        public string FeederFlowStatus
        {
            get => _feederFlowStatus;
            private set => Set(ref _feederFlowStatus, value);
        }

        public ObservableCollection<FeederCalibrationFlowStep> FeederFlowSteps { get; private set; } =
            new ObservableCollection<FeederCalibrationFlowStep>();

        private FeederCalibrationFlowStep FindFlowStep(string key, int? rowIndex)
        {
            // The origin's optical discovery AND persisted correction are
            // one operator-visible stop, though they are two evidence stages.
            if (key == "save-origin")
                key = "center-fiducial";
            return FeederFlowSteps.FirstOrDefault(step =>
                step.StepKey == key && step.RowIndex == rowIndex);
        }

        private void RefreshFlowStepEvidence(
            StripFeederCalibrationRunner operation, int? rowIndex)
        {
            foreach (var item in operation.Current.Steps)
            {
                var step = FindFlowStep(item.Key, rowIndex);
                if (step == null)
                    continue;

                if (item.Status == MachineOperationStepStatus.Active)
                    step.Begin(operation.StatusText);
                if (item.Status == MachineOperationStepStatus.Passed &&
                    item.Key != "center-fiducial")
                    step.Pass(item.Evidence.LastOrDefault()?.Value ?? operation.MeasurementText);
                if (item.Status == MachineOperationStepStatus.Blocked)
                    step.Fail(item.Error ?? operation.StatusText);
            }
        }

        private async Task<bool> RunFeederPhaseAsync(
            StripFeeder feeder, StripFeederRow row,
            StripFeederCalibrationScope scope, CancellationToken token)
        {
            if (!ReferenceEquals(CurrentRow, row) ||
                !ReferenceEquals(Current, feeder) || Machine == null)
                return FailFlow("Feeder, row or machine changed before calibration.");

            var machine = Machine;
            using (var operation = new StripFeederCalibrationRunner(
                machine, _locatorVM, feeder, row,
                () => FindLocation(StripFeederLocationTypes.FeederOrigin),
                () => FindLocation(StripFeederLocationTypes.FirstFeederRowReferenceHole, true),
                () => TapePitch?.ToDouble() ?? 0,
                index => ResolvePocketForCalibration(index),
                item => _restClient.PutAsync("/api/mfg/stripfeeder", item),
                () => ReferenceEquals(Current, feeder) &&
                      ReferenceEquals(CurrentRow, row) &&
                      ReferenceEquals(Machine, machine),
                scope))
            {
                // Each operation has the authoritative evidence and durable
                // history. The modal is just a live projection of that session.
                var rowKey = scope == StripFeederCalibrationScope.FeederOrigin
                    ? (int?)null : row.RowIndex;
                var outcome = await MachineOperationProcedureExecutor.RunAsync(
                    () => operation.Current,
                    async ct =>
                    {
                        token.ThrowIfCancellationRequested();
                        var active = operation.Current.ActiveStepKey;
                        FindFlowStep(active, rowKey)?.Begin(operation.StatusText);
                        var result = await operation.ExecuteAutoNextAsync(ct);
                        RefreshFlowStepEvidence(operation, rowKey);
                        if (!result.Successful)
                            FindFlowStep(active, rowKey)?.Fail(
                                result.ErrorMessage ?? operation.StatusText);
                        return result;
                    },
                    async ct =>
                    {
                        await SaveFeederCalibrationRunHistoryAsync(operation);
                        return _pendingHistorySnapshot == null;
                    }, token);

                RefreshFlowStepEvidence(operation, rowKey);
                if (!outcome.Completed)
                    return FailFlow(outcome.Message ??
                        $"Calibration stopped at {operation.Current.ActiveStepKey}.");
                return true;
            }
        }

        private bool FailFlow(string error)
        {
            FeederFlowStatus = "Stopped: " + error;
            Machine?.AddStatusMessage(StatusMessageTypes.Warning, FeederFlowStatus);
            return false;
        }

        /// <summary>
        /// Invoked once from the feeder-level modal. No step buttons are
        /// involved; physical machine movement is still operator-supervised.
        /// The barcode must match the persisted 4-hex-digit feeder identity.
        /// </summary>
        public async Task StartFeederCalibrationFlowAsync()
        {
            if (FeederFlowRunning)
                return;

            var feeder = Current;
            var machine = Machine;
            var installedRows = feeder == null ? new List<StripFeederRow>()
                : FeederCalibrationFlowPlan.InstalledRows(feeder);
            FeederFlowSteps = new ObservableCollection<FeederCalibrationFlowStep>(
                feeder == null ? new List<FeederCalibrationFlowStep>()
                    : FeederCalibrationFlowPlan.Steps(feeder));
            RaisePropertyChanged(nameof(FeederFlowSteps));
            _feederFlowRunAttempt++;
            FeederFlowFinished = false;
            FeederFlowRunning = true;

            try
            {
                if (feeder == null || !_isEditing || machine == null || !machine.Connected ||
                    _locatorVM == null)
                {
                    FailFlow("Select a saved feeder on a connected machine before running calibration.");
                    return;
                }
                if (!FeederBarcodeIdFormat.TryNormalize(feeder.FeederId, out var expectedId) ||
                    String.IsNullOrEmpty(expectedId))
                {
                    FailFlow("Set and save the feeder's four-digit hexadecimal barcode ID first.");
                    return;
                }
                if (installedRows.Count == 0)
                {
                    FailFlow("No installed component tapes were found on this feeder.");
                    return;
                }
                if (installedRows.Any(row => row.CurrentPartIndex <= 1 ||
                    row.PartCapacity < row.CurrentPartIndex))
                {
                    FailFlow("Every installed tape needs a valid current index above 1 and within its capacity to verify its preceding empty pocket.");
                    return;
                }

                FeederFlowStatus =
                    $"Calibrating feeder {feeder.Name}: {installedRows.Count} installed tape(s). Stay at the machine.";
                var firstRow = installedRows[0];
                CurrentRow = firstRow;

                if (!await RunFeederPhaseAsync(feeder, firstRow,
                    StripFeederCalibrationScope.FeederOrigin, CancellationToken.None))
                    return;

                var barcodeStep = FindFlowStep("barcode", null);
                barcodeStep?.Begin("Framing and reading full barcode with CRC-2 validation.");
                // Reuse the existing framing and scan implementations; no
                // replacement barcode decoder or speculative search motion.
                FrameFeederBarcode();
                var barcode = await ScanFeederBarcodeAsync();
                if (!barcode.Successful)
                {
                    barcodeStep?.Fail(barcode.ErrorMessage ?? FeederBarcodeScanStatus);
                    FailFlow("Barcode scan failed: " + barcode.ErrorMessage);
                    return;
                }
                if (!String.Equals(barcode.Result, expectedId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    var error = $"Feeder identity mismatch: expected {expectedId}, read {barcode.Result}. No tape calibration started.";
                    barcodeStep?.Fail(error);
                    FailFlow(error);
                    return;
                }
                barcodeStep?.Pass($"Barcode {barcode.Result} matches saved Feeder ID; CRC-2 OK.");

                foreach (var row in installedRows)
                {
                    if (!ReferenceEquals(Current, feeder) ||
                        !ReferenceEquals(Machine, machine))
                    {
                        FailFlow("Feeder or machine selection changed. Calibration stopped.");
                        return;
                    }

                    CurrentRow = row;
                    FeederFlowStatus = $"Calibrating installed tape {row.RowIndex}: {row.Component?.Text}";
                    if (!await RunFeederPhaseAsync(feeder, row,
                        StripFeederCalibrationScope.InstalledTape, CancellationToken.None))
                        return;
                }

                FeederFlowStatus =
                    $"Completed: feeder ID {expectedId} verified and all {installedRows.Count} installed tape(s) calibrated.";
            }
            catch (Exception ex)
            {
                FailFlow("Unexpected feeder procedure error: " + ex.Message);
            }
            finally
            {
                FeederFlowRunning = false;
                FeederFlowFinished = true;
            }
        }
    }
}
