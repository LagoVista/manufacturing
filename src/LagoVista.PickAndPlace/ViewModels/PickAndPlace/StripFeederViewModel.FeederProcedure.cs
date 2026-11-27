using LagoVista.Core.Validation;
using LagoVista.Core.Models.Drawing;
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

                // Count actual camera correction commands, not just a
                // modal Run Again. The runner caps each target at five.
                if (item.Key == operation.AutomaticJogStepKey)
                    step.RetryCount = Math.Max(step.RetryCount, operation.AutomaticJogCount);

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
            StripFeederCalibrationScope scope, string expectedId, CancellationToken token)
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
                        // Verify the physical barcode BEFORE committing the
                        // shared feeder origin offset. A wrong installed feeder
                        // must not alter persistent calibration settings.
                        if (scope == StripFeederCalibrationScope.FeederOrigin &&
                            active == "save-origin" &&
                            !await VerifyBarcodeAndReturnToOriginAsync(
                                feeder, operation, expectedId, ct))
                        {
                            FindFlowStep(active, rowKey)?.Fail(FeederFlowStatus);
                            return new MachineOperationActionResult
                            {
                                Successful = false,
                                ErrorMessage = FeederFlowStatus
                            };
                        }
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
                {
                    // Keep partial evidence for a stopped operation and do not
                    // overwrite a specific barcode mismatch diagnostic.
                    // Never replace an unsaved checkpoint snapshot with
                    // later cancellation evidence. Preserve and retry the
                    // original failed write on the next modal attempt.
                    if (operation.CanCancel && _pendingHistorySnapshot == null)
                    {
                        await operation.CancelAsync(token);
                        await SaveFeederCalibrationRunHistoryAsync(operation);
                    }
                    return FeederFlowStatus.StartsWith("Stopped:", StringComparison.Ordinal)
                        ? false
                        : FailFlow(outcome.Message ??
                            $"Calibration stopped at {operation.Current.ActiveStepKey}.");
                }
                return true;
            }
        }

        private async Task<bool> WaitForFeederFlowSerialIdleAsync(CancellationToken token)
        {
            var machine = Machine;
            var consecutive = 0;
            for (var poll = 0; poll < 40; poll++)
            {
                token.ThrowIfCancellationRequested();
                if (machine == null || !machine.Connected)
                    return false;
                if (!machine.Busy &&
                    machine.ToSendQueueCount == 0 &&
                    machine.UnacknowledgedBytesSent == 0)
                {
                    if (++consecutive >= 2)
                        return true;
                }
                else consecutive = 0;
                await Task.Delay(75, token);
            }
            return false;
        }

        private async Task<bool> VerifyBarcodeAndReturnToOriginAsync(
            StripFeeder feeder, StripFeederCalibrationRunner operation,
            string expectedId, CancellationToken token)
        {
            var barcodeStep = FindFlowStep("barcode", null);
            barcodeStep?.Begin("Scanning all 11 barcode bars; verifying CRC-2 and feeder identity.");
            var machine = Machine;
            if (machine == null || !machine.Connected ||
                machine.Mode != OperatingMode.Manual)
                return FailBarcode(barcodeStep, "Machine unavailable or not in Manual mode.");

            token.ThrowIfCancellationRequested();
            var centered = await machine.GetCurrentLocationAsync(5000);
            if (!centered.Successful || centered.Result == null)
                return FailBarcode(barcodeStep,
                    "Could not capture the visually centered feeder origin position.");

            // Derive a provisional origin offset solely to aim the EXISTING
            // barcode framing logic. It remains in-memory until identity is
            // confirmed; saving is exclusively owned by the runner afterward.
            var calculated = FindLocation(StripFeederLocationTypes.FeederOrigin);
            var capture = machine.PositionImageCaptureService;
            if (!calculated.Successful || calculated.Result == null ||
                capture?.Camera == null ||
                !FeederOriginCalibrationMath.TryCalculate(
                    calculated.Result, centered.Result,
                    capture.Camera.EffectivePixelsPerMM,
                    positionStable: true, centered: true, out var provisional))
                return FailBarcode(barcodeStep, "Unable to derive a safe provisional feeder origin.");

            var previous = new Point2D<double>(
                feeder.OriginOffset.X, feeder.OriginOffset.Y);
            try
            {
                feeder.OriginOffset = provisional;
                FrameFeederBarcode();
                var barcode = await ScanFeederBarcodeAsync();
                if (!barcode.Successful)
                    return FailBarcode(barcodeStep,
                        "Full feeder barcode read failed: " + barcode.ErrorMessage);
                if (!String.Equals(barcode.Result, expectedId,
                    StringComparison.OrdinalIgnoreCase))
                    return FailBarcode(barcodeStep,
                        $"Feeder ID mismatch: expected {expectedId}, read {barcode.Result}. No calibration offset saved.");

                barcodeStep?.Pass(
                    $"Matched feeder {barcode.Result}; full 11-bar scan, CRC-2 verified.");
                operation.RecordBarcodeIdentityEvidence(expectedId, barcode.Result);

                // The scan may move as far as 18 mm along the barcode.
                // Return to the measured center and reacquire a fresh corner
                // before the existing runner commits any origin offset.
                token.ThrowIfCancellationRequested();
                machine.SetVisionProfile(CameraTypes.Position,
                    VisionProfile.VisionProfile_FeederOrigin);
                machine.SendSafeMoveHeight();
                var safe = await machine.SpinUntilIdleAsync(10000);
                if (!safe.Successful ||
                    !await WaitForFeederFlowSerialIdleAsync(token))
                    return FailFlow("Safe Z/serial idle failed returning from barcode scan.");

                machine.PositionImageCaptureService?.ResetDetections();
                machine.GotoPoint(centered.Result);
                var moved = await machine.SpinUntilIdleAsync(30000);
                if (!moved.Successful ||
                    !await WaitForFeederFlowSerialIdleAsync(token))
                    return FailFlow("Unable to return to measured feeder origin after barcode scan: " + moved.ErrorMessage);
                await Task.Delay(200, token);
                var actual = await machine.GetCurrentLocationAsync(5000);
                if (!actual.Successful || !machine.Connected ||
                    !FeederOriginCalibrationMath.SamePosition(
                        actual.Result, centered.Result, 0.5))
                    return FailFlow("Machine did not settle at the measured origin after barcode scan.");

                return true;
            }
            finally
            {
                // Barcode mismatch, camera failure and successful verification
                // all leave the previously saved offset unchanged in memory.
                // The runner performs the actual save after fresh corner proof.
                feeder.OriginOffset = previous;
            }
        }

        private bool FailBarcode(FeederCalibrationFlowStep step, string error)
        {
            step?.Fail(error);
            return FailFlow(error);
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
            var prior = FeederFlowSteps?.Where(x => x.StartedAt.HasValue).ToDictionary(
                x => x.StepKey + "/" + x.RowIndex, x => x.RetryCount);
            var planned = feeder == null ? new List<FeederCalibrationFlowStep>()
                : FeederCalibrationFlowPlan.Steps(feeder);
            foreach (var step in planned)
            {
                if (prior != null && prior.TryGetValue(
                    step.StepKey + "/" + step.RowIndex, out var count))
                    step.RetryCount = count + 1;
            }
            FeederFlowSteps = new ObservableCollection<FeederCalibrationFlowStep>(planned);
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
                    var message = "Set and save the feeder's four-digit hexadecimal barcode ID first.";
                    FindFlowStep("barcode", null)?.Fail(message);
                    FailFlow(message);
                    return;
                }
                if (installedRows.Count == 0)
                {
                    FailFlow("No installed component tapes were found on this feeder.");
                    return;
                }
                if (installedRows.Any(row => row.CurrentPartIndex <= 1 ||
                    (row.PartCapacity > 0 && row.PartCapacity < row.CurrentPartIndex)))
                {
                    FailFlow("Every installed tape needs a valid current index above 1 and within its capacity to verify its preceding empty pocket.");
                    return;
                }

                // A failed durable checkpoint must not be overwritten by a
                // new run in the same modal. Attempt the existing idempotent
                // history retry before any physical movement.
                if (_pendingHistorySnapshot != null)
                {
                    await RetryFeederCalibrationHistoryAsync();
                    if (_pendingHistorySnapshot != null)
                    {
                        FailFlow("Previous calibration history remains unsaved. Check the API before starting again.");
                        return;
                    }
                }

                FeederFlowStatus =
                    $"Calibrating feeder {feeder.Name}: {installedRows.Count} installed tape(s). Stay at the machine.";
                var firstRow = installedRows[0];
                CurrentRow = firstRow;

                if (!await RunFeederPhaseAsync(feeder, firstRow,
                    StripFeederCalibrationScope.FeederOrigin, expectedId, CancellationToken.None))
                    return;

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
                        StripFeederCalibrationScope.InstalledTape, expectedId, CancellationToken.None))
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
