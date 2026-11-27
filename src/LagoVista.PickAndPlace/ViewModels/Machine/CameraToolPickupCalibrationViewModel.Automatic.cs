using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.Core;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public sealed partial class CameraToolPickupCalibrationViewModel
    {
        private bool _automaticRunActive;
        private bool _automaticRunVerified;
        private bool _stopAutomatic;
        private bool _automaticFull;
        private bool _firstKnownLocationHandled;
        private IMachine _automaticMachine;

        public bool AutomaticRunActive
        {
            get => _automaticRunActive;
            private set
            {
                Set(ref _automaticRunActive, value);
                RaisePropertyChanged(nameof(CanOperate));
                RaisePropertyChanged(nameof(CanStartAutomatic));
                RaisePropertyChanged(nameof(CanRetryHistory));
            }
        }
        public bool AutomaticRunVerified { get => _automaticRunVerified; private set => Set(ref _automaticRunVerified, value); }

        // Match Known Location's bounded settling policy: require three
        // consecutive idle samples. Never wait out a lost safety invariant.
        private const int AutomaticBoundarySettleMs = 5000;
        private const int AutomaticBoundaryIdlePolls = 3;

        private async Task RequireAutomaticBoundaryAsync(bool requireHistory = true, bool allowSelectedHead = false)
        {
            var elapsed = System.Diagnostics.Stopwatch.StartNew();
            var stableIdle = 0;
            string settling = null;
            while (true)
            {
                if (_stopAutomatic)
                    throw new CalibrationStopped("Operator stopped at a safe phase boundary.");
                var m = _automaticMachine;
                if (m == null || !ReferenceEquals(m, _repo.CurrentMachine))
                    throw new CalibrationStopped("Selected machine changed or is unavailable.");
                if (!m.Connected)
                    throw new CalibrationStopped("Machine disconnected.");
                if (!m.WasMachineHomed)
                    throw new CalibrationStopped("Machine is not homed.");
                if (!m.WasMachinOriginCalibrated)
                    throw new CalibrationStopped("Machine origin is not calibrated.");
                if (m.Mode != OperatingMode.Manual)
                    throw new CalibrationStopped("Machine is not in Manual mode.");
                if (!allowSelectedHead && m.CurrentMachineToolHead != null)
                    throw new CalibrationStopped("Camera navigation was not restored at the phase boundary.");
                if (m.LeftVacuumPump || m.RightVacuumPump)
                    throw new CalibrationStopped($"Vacuum still enabled: left={m.LeftVacuumPump}, right={m.RightVacuumPump}.");
                if (requireHistory && (_pendingHistorySnapshot != null || HistoryBusy ||
                    !HistoryStatus.StartsWith("RECORDED", StringComparison.Ordinal)))
                    throw new CalibrationStopped("Previous phase history is not recorded. No further motion.");

                if (m.Busy || m.IsLocating || _locator.IsLocating ||
                    m.ToSendQueueCount != 0 || m.UnacknowledgedBytesSent != 0)
                {
                    stableIdle = 0;
                    settling = $"Busy={m.Busy}, machine locating={m.IsLocating}, locator active={_locator.IsLocating}, " +
                        $"queued={m.ToSendQueueCount}, unacknowledged bytes={m.UnacknowledgedBytesSent}.";
                    Status = "Waiting for controller to settle: " + settling;
                }
                else if (++stableIdle >= AutomaticBoundaryIdlePolls)
                    return;

                if (elapsed.ElapsedMilliseconds >= AutomaticBoundarySettleMs)
                    throw new CalibrationStopped("Controller did not reach stable idle within 5 seconds. " +
                        (settling ?? "Insufficient consecutive idle samples."));
                await Task.Delay(100);
            }
        }

        private async Task EnterAutomaticCameraNavigationAsync()
        {
            await RequireAutomaticBoundaryAsync(false, allowSelectedHead: true);
            var m = _automaticMachine;
            var head = m.CurrentMachineToolHead;
            if (head == null) return;
            Status = "Restoring camera navigation at safe move height...";
            var safeZ = m.Settings.SafMoveHeight;
            if (!Finite(safeZ))
                throw new CalibrationStopped("Invalid configured safe move height.");
            var before = await m.GetCurrentLocationAsync(5000);
            if (!before.Successful || before.Result == null)
                throw new CalibrationStopped("Cannot read controller XY for camera navigation return.");
            if (!NozzleRunoutMotionPreflight.TryValidateCameraReturn(before.Result,
                head.Offset, m.Settings.WorkAreaSize, out var reason))
                throw new CalibrationStopped(reason);
            await RequireAutomaticBoundaryAsync(false, allowSelectedHead: true);
            if (!ReferenceEquals(head, m.CurrentMachineToolHead))
                throw new CalibrationStopped("Selected head changed before camera navigation return.");
            m.SendSafeMoveHeight();
            var idle = await m.SpinUntilIdleAsync(10000);
            if (!idle.Successful)
                throw new CalibrationStopped("Safe height move did not settle.");
            await RequireAutomaticBoundaryAsync(false, allowSelectedHead: true);
            var raised = await m.GetCurrentLocationAsync(5000);
            if (!raised.Successful || raised.Result == null || !Finite(m.ToolCommonZ) ||
                Math.Abs(m.ToolCommonZ - safeZ) > 0.20 ||
                !FeederOriginCalibrationMath.SamePosition(raised.Result, before.Result, 0.5))
                throw new CalibrationStopped("Safe height or XY could not be verified before camera navigation return.");
            await RequireAutomaticBoundaryAsync(false, allowSelectedHead: true);
            if (!ReferenceEquals(head, m.CurrentMachineToolHead) ||
                !NozzleRunoutMotionPreflight.TryValidateCameraReturn(raised.Result,
                    head.Offset, m.Settings.WorkAreaSize, out _))
                throw new CalibrationStopped("Head or return geometry changed before camera navigation.");
            await m.MoveToCameraAsync();
            await RequireAutomaticBoundaryAsync(false);
            var after = await m.GetCurrentLocationAsync(5000);
            if (!after.Successful || after.Result == null ||
                !FeederOriginCalibrationMath.SamePosition(after.Result, before.Result, 0.5) ||
                !Finite(m.ToolCommonZ) || Math.Abs(m.ToolCommonZ - safeZ) > 0.20)
                throw new CalibrationStopped("Camera navigation return did not preserve controller XY and safe Z.");
            await RequireAutomaticBoundaryAsync(false);
        }

        public async Task RunAutomaticAsync()
        {
            if (!CanStartAutomatic) return;
            AutomaticRunActive = true;
            AutomaticRunVerified = false;
            _stopAutomatic = false;
            _automaticFull = FullDiagnostics;
            _firstKnownLocationHandled = false;
            _automaticMachine = _repo.CurrentMachine;
            ClearSharedCandidate();
            ResetNozzleResults();
            _firstFiducialVector = null;
            RepeatabilityProgress = "";
            FiducialCheckpointStatus = "";
            if (_pendingHistorySnapshot != null)
            {
                Status = "Previous run history is pending. Retry History Save first.";
                AutomaticRunActive = false;
                _automaticFull = false;
                _automaticMachine = null;
                return;
            }
            InvalidateSavedPickupCandidate();
            foreach (var index in new[] { 1, 2 })
            {
                SetPickupResult(index, "Pending", _automaticFull ? "Not measured" : "Saved · validation only");
                SetDiscResult(index, "Pending", "Not tested");
            }
            try
            {
                await EnterAutomaticCameraNavigationAsync();
                var installed = _automaticMachine.Settings.ToolHeads.Where(h => h?.CurrentNozzle != null)
                    .Select(h => h.HeadIndex).OrderBy(i => i).ToArray();
                if (!installed.SequenceEqual(new[] { 1, 2 }))
                    throw new CalibrationStopped("Automatic shared calibration requires both installed heads.");
                if (_automaticFull && (!Int32.TryParse(DiagnosticIterations, out var iterations) || iterations < 1 || iterations > 100))
                    throw new CalibrationStopped("Full Diagnostics requires 1–100 iterations per head.");
                // Check both heads and inspection geometry before the first physical stage.
                _machine = _automaticMachine;
                try { CheckSharedReadiness(); await ReadSharedGeometryAsync(); }
                finally { _machine = null; }
                await RequireAutomaticBoundaryAsync(false);
                SaveVerifiedOffset = false; // retain the independent two-pick candidate gate
                if (_automaticFull)
                {
                    await RunFiducialCheckpointAsync("Before pickup calibration");
                    await RequireAutomaticBoundaryAsync();
                    if (CalibrationSteps.Any(s => s.Status != "Passed"))
                        throw new CalibrationStopped("Initial machine fiducial check did not pass. " + Status);
                    foreach (var index in installed)
                    {
                        HeadIndexText = index.ToString();
                        await StartCoreAsync();
                        await RequireAutomaticBoundaryAsync();
                        if (!CanSaveAndVerify)
                            throw new CalibrationStopped("Head " + index + " did not produce a recorded two-pick candidate. " + Status);
                        await SaveAndVerifyCoreAsync();
                        await RequireAutomaticBoundaryAsync();
                        if (!SavedPickupVerified || CalibrationSteps.Any(s => s.Status != "Passed"))
                            throw new CalibrationStopped("Head " + index + " saved pickup did not pass. " + Status);
                        await RunFiducialCheckpointAsync("After Head " + index + " pickup acceptance");
                        await RequireAutomaticBoundaryAsync();
                        if (CalibrationSteps.Any(s => s.Status != "Passed"))
                            throw new CalibrationStopped("Machine fiducial check did not pass. " + Status);
                    }
                    await ProposeSharedCameraCoreAsync();
                    await RequireAutomaticBoundaryAsync();
                    if (!SharedCandidateReady || CalibrationSteps.Any(s => s.Status != "Passed"))
                        throw new CalibrationStopped("Shared camera proposal did not pass. " + Status);
                    await ApproveSharedCameraCoreAsync();
                    await RequireAutomaticBoundaryAsync();
                    if (!SharedCameraVerified || CalibrationSteps.Any(s => s.Status != "Passed"))
                        throw new CalibrationStopped("Shared camera verification failed. " + Status);
                    await RunFiducialCheckpointAsync("After shared camera acceptance");
                    await RequireAutomaticBoundaryAsync();
                    if (CalibrationSteps.Any(s => s.Status != "Passed"))
                        throw new CalibrationStopped("Machine fiducial check did not pass. " + Status);
                    await RunFullDiagnosticsCoreAsync();
                }
                else
                {
                    // No candidate creation, correction jog, or configuration writes.
                    await ValidateSharedCameraCoreAsync();
                }
                await RequireAutomaticBoundaryAsync();
                if (!SharedCameraVerified || CalibrationSteps.Any(s => s.Status != "Passed") ||
                    HeadOneDiscState != "Passed" || HeadTwoDiscState != "Passed")
                    throw new CalibrationStopped("Final validation did not pass. " + Status);
                AutomaticRunVerified = true;
                Status = _automaticFull
                    ? "PASS: both pickup offsets, shared camera and full repeatability verified."
                    : "PASS: both saved pickups and uncorrected nozzle approaches verified. Nothing saved.";
            }
            catch (Exception ex)
            {
                AutomaticRunVerified = false;
                Status = "STOPPED: " + ex.Message;
                _automaticMachine?.AddStatusMessage(StatusMessageTypes.Warning, Status);
            }
            finally
            {
                _automaticFull = false;
                _stopAutomatic = false;
                _automaticMachine = null;
                AutomaticRunActive = false;
            }
        }

        // Only in the write-authorized full run, and only for the first fresh,
        // bounded, stable target observation. Keep subsequent optical checks.
        private async Task RememberFirstKnownLocationAsync(Point2D<double> measured)
        {
            RequireValid();
            _firstKnownLocationHandled = true;
            var old = _machine.Settings.KnownCalibrationPoint;
            if (!KnownLocationCylinderCenteringMath.TryPlanKnownLocationUpdate(old, measured,
                _machine.Settings.WorkAreaSize, out var shouldWrite))
                throw new CalibrationStopped("Measured disc center is beyond the allowed Known Location update; inspect fixture.");
            if (!shouldWrite) return;
            if (_stopAutomatic) throw new CalibrationStopped("Stopped before Known Location save.");
            await NeedIdleAsync("Known Location save preflight");
            if (_machine.CurrentMachineToolHead != null || _machine.LeftVacuumPump || _machine.RightVacuumPump)
                throw new CalibrationStopped("Known Location save requires empty heads and camera mode.");
            var persisted = await ReadSharedGeometryAsync();
            if (!SameXY(persisted.KnownCalibrationPoint, old))
                throw new CalibrationStopped("Stored Known Location changed; reload before calibration.");
            RequireValid();
            if (_stopAutomatic || _machine.Busy || _machine.ToSendQueueCount != 0 ||
                _machine.UnacknowledgedBytesSent != 0 || _machine.CurrentMachineToolHead != null ||
                _machine.LeftVacuumPump || _machine.RightVacuumPump)
                throw new CalibrationStopped("Machine readiness changed before Known Location save.");
            persisted.KnownCalibrationPoint = new Point2D<double>(measured.X, measured.Y);
            var saved = await _restClient.PutAsync("/api/mfg/machine", persisted);
            if (!saved.Successful)
                throw new CalibrationStopped("Known Location save not acknowledged; inspect authoritative settings.");
            var readback = await ReadSharedGeometryAsync();
            if (!SameXY(readback.KnownCalibrationPoint, measured))
                throw new CalibrationStopped("Known Location read-back failed; reload before further motion.");
            _machine.Settings.KnownCalibrationPoint = new Point2D<double>(measured.X, measured.Y);
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = _session.Snapshot.RunId,
                StepKey = _session.Snapshot.ActiveStepKey,
                Name = "Known Location updated from first stable disc center",
                Value = $"Old {Describe(old)}; saved {Describe(measured)}",
                Source = "T001.Automatic",
                Saved = true,
                Verification = MachineOperationVerificationKind.MachineObserved,
                CapturedAtUtc = DateTimeOffset.UtcNow
            });
        }
    }
}
