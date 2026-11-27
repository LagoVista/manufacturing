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

        private void RequireAutomaticBoundary(bool requireHistory = true)
        {
            if (_stopAutomatic) throw new CalibrationStopped("Operator stopped at a safe phase boundary.");
            var m = _automaticMachine;
            if (m == null || !ReferenceEquals(m, _repo.CurrentMachine) ||
                !m.Connected || !m.WasMachineHomed || !m.WasMachinOriginCalibrated ||
                m.Mode != OperatingMode.Manual ||
                m.Busy || m.IsLocating || _locator.IsLocating ||
                m.ToSendQueueCount != 0 || m.UnacknowledgedBytesSent != 0 ||
                m.CurrentMachineToolHead != null || m.LeftVacuumPump || m.RightVacuumPump)
                throw new CalibrationStopped("Machine must remain homed, connected, idle in Manual camera mode, both vacuums off.");
            if (requireHistory && (_pendingHistorySnapshot != null || HistoryBusy ||
                !HistoryStatus.StartsWith("RECORDED", StringComparison.Ordinal)))
                throw new CalibrationStopped("Previous phase history is not recorded. No further motion.");
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
                RequireAutomaticBoundary(false);
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
                RequireAutomaticBoundary(false);
                SaveVerifiedOffset = false; // retain the independent two-pick candidate gate
                if (_automaticFull)
                {
                    foreach (var index in installed)
                    {
                        HeadIndexText = index.ToString();
                        await StartCoreAsync();
                        RequireAutomaticBoundary();
                        if (!CanSaveAndVerify)
                            throw new CalibrationStopped("Head " + index + " did not produce a recorded two-pick candidate. " + Status);
                        await SaveAndVerifyCoreAsync();
                        RequireAutomaticBoundary();
                        if (!SavedPickupVerified || CalibrationSteps.Any(s => s.Status != "Passed"))
                            throw new CalibrationStopped("Head " + index + " saved pickup did not pass. " + Status);
                    }
                    await ProposeSharedCameraCoreAsync();
                    RequireAutomaticBoundary();
                    if (!SharedCandidateReady || CalibrationSteps.Any(s => s.Status != "Passed"))
                        throw new CalibrationStopped("Shared camera proposal did not pass. " + Status);
                    await ApproveSharedCameraCoreAsync();
                    RequireAutomaticBoundary();
                    if (!SharedCameraVerified || CalibrationSteps.Any(s => s.Status != "Passed"))
                        throw new CalibrationStopped("Shared camera verification failed. " + Status);
                    await RunFullDiagnosticsCoreAsync();
                }
                else
                {
                    // No candidate creation, correction jog, or configuration writes.
                    await ValidateSharedCameraCoreAsync();
                }
                RequireAutomaticBoundary();
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
