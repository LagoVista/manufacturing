using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Supervised, per-head vacuum validation. Only the operator can advance a
    /// hardware stage. The fiducial approach moves at configured safe Z; the
    /// operator jogs into plate contact manually rather than this pilot guessing
    /// a ZL/ZR contact height from feeder PickHeight.
    ///
    /// This first pass does not persist calibration or claim independent
    /// solenoid proof: the legacy solenoid interface is not per-head on Marlin.
    /// </summary>
    public sealed class VacuumCalibrationViewModel : ViewModelBase
    {
        private enum Phase { Ready, Idle, Open, AtFiducial, Sealed, Released, Finished, Faulted, Aborted }
        private readonly IMachineRepo _repo;
        private readonly IMachine _machine;
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private Phase _phase = Phase.Ready;
        private MachineOperationSession _session;
        private VacuumPressureBand _idle;
        private VacuumPressureBand _oppositeIdle;
        private VacuumPressureBand _open;
        private VacuumPressureBand _oppositeOpen;
        private VacuumPressureBand _sealed;
        private VacuumCalibrationCandidate _leftCandidate;
        private VacuumCalibrationCandidate _rightCandidate;
        private bool _busy;
        private bool _pumpActive;
        private bool _operatorAcknowledged;
        private bool _operatorConfirmedContact;
        private string _status = ManufacturingResources.VacuumCalibration_Ready;
        private string _rawReading = "-";
        private string _leftSummary = "-";
        private string _rightSummary = "-";
        private int _head = 1;

        public VacuumCalibrationViewModel(IMachineRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _machine = repo.CurrentMachine ?? throw new InvalidOperationException("Select a machine before starting.");
        }

        public int HeadIndex => _head;
        public string HeadDescription => "Head " + _head;
        public bool Busy { get => _busy; private set { Set(ref _busy, value); NotifyButtons(); } }
        public bool PumpActive { get => _pumpActive; private set { Set(ref _pumpActive, value); NotifyButtons(); } }
        public bool OperatorAcknowledged
        {
            get => _operatorAcknowledged;
            set { Set(ref _operatorAcknowledged, value); NotifyButtons(); }
        }
        public bool OperatorConfirmedContact
        {
            get => _operatorConfirmedContact;
            set { Set(ref _operatorConfirmedContact, value); NotifyButtons(); }
        }
        public string Status { get => _status; private set => Set(ref _status, value); }
        public string RawReading { get => _rawReading; private set => Set(ref _rawReading, value); }
        public string LeftSummary { get => _leftSummary; private set => Set(ref _leftSummary, value); }
        public string RightSummary { get => _rightSummary; private set => Set(ref _rightSummary, value); }
        public bool CanContinue => !Busy && _phase != Phase.Finished &&
            _phase != Phase.Faulted && _phase != Phase.Aborted &&
            (_phase != Phase.Ready || OperatorAcknowledged) &&
            (_phase != Phase.AtFiducial || OperatorConfirmedContact);
        public bool CanAbort => _phase != Phase.Aborted && _phase != Phase.Finished;
        public bool CanClose => !Busy && !PumpActive && (_phase == Phase.Ready || _phase == Phase.Aborted || _phase == Phase.Finished);
        public bool NeedsPlateContact => _phase == Phase.AtFiducial;
        public string NextAction => _phase == Phase.Ready ? ManufacturingResources.VacuumCalibration_CaptureIdle :
            _phase == Phase.Idle ? ManufacturingResources.VacuumCalibration_PumpOpen :
            _phase == Phase.Open ? ManufacturingResources.VacuumCalibration_MoveFiducial :
            _phase == Phase.AtFiducial ? ManufacturingResources.VacuumCalibration_CaptureSealed :
            _phase == Phase.Sealed ? ManufacturingResources.VacuumCalibration_Release :
            _phase == Phase.Released ? ManufacturingResources.VacuumCalibration_NextHead :
            ManufacturingResources.VacuumCalibration_Complete;
        public string ProcedureStatus => _session?.Snapshot?.Status.ToString() ?? "Pending";

        private void NotifyButtons()
        {
            RaisePropertyChanged(nameof(CanContinue));
            RaisePropertyChanged(nameof(CanAbort));
            RaisePropertyChanged(nameof(CanClose));
            RaisePropertyChanged(nameof(NeedsPlateContact));
            RaisePropertyChanged(nameof(NextAction));
            RaisePropertyChanged(nameof(ProcedureStatus));
            RaisePropertyChanged(nameof(HeadIndex));
            RaisePropertyChanged(nameof(HeadDescription));
        }

        private void SetPhase(Phase phase)
        {
            _phase = phase;
            NotifyButtons();
        }

        private bool ContextSafe(bool requireSelectedHead, out string reason)
        {
            reason = null;
            var settings = _machine.Settings;
            if (!ReferenceEquals(_repo.CurrentMachine, _machine) ||
                !_machine.Connected || !_machine.WasMachineHomed ||
                !_machine.WasMachinOriginCalibrated ||
                _machine.Mode != OperatingMode.Manual || settings == null)
            {
                reason = "Machine changed, is disconnected, not homed/origin-calibrated or not in Manual mode.";
                return false;
            }
            if (settings.ToolHeads?.FirstOrDefault(h => h.HeadIndex == _head && h.CurrentNozzle != null) == null)
            {
                reason = "No installed nozzle for the selected head.";
                return false;
            }
            var fiducial = settings.MachineFiducial;
            var area = settings.WorkAreaSize;
            if (fiducial == null || area == null || fiducial.IsOrigin() ||
                !Finite(fiducial.X) || !Finite(fiducial.Y) ||
                !Finite(area.X) || !Finite(area.Y) ||
                fiducial.X <= 0 || fiducial.Y <= 0 ||
                fiducial.X >= area.X || fiducial.Y >= area.Y ||
                !Finite(settings.SafMoveHeight) || settings.SafMoveHeight == 0)
            {
                reason = "Configure a non-origin MachineFiducial inside work area and a nonzero safe Z before calibration.";
                return false;
            }
            if (requireSelectedHead && _machine.CurrentMachineToolHead?.HeadIndex != _head)
            {
                reason = "The selected physical tool head changed during the procedure.";
                return false;
            }
            if (_machine.IsLocating)
            {
                reason = "Machine is running another location operation.";
                return false;
            }
            return true;
        }

        private static bool Finite(double value) => !Double.IsNaN(value) && !Double.IsInfinity(value);

        private async Task<VacuumPressureBand> CaptureAsync(int head, CancellationToken token)
        {
            var samples = new List<VacuumPressureSample>();
            for (var i = 0; i < 6; i++)
            {
                token.ThrowIfCancellationRequested();
                if (!ContextSafe(_phase == Phase.AtFiducial || _phase == Phase.Sealed,
                    out var reason))
                    throw new InvalidOperationException(reason);

                var result = head == 1
                    ? await _machine.ReadLeftVacuumAsync()
                    : await _machine.ReadRightVacuumAsync();
                if (!result.Successful)
                    throw new InvalidOperationException("Head " + head + ": " + result.ErrorMessage);
                samples.Add(new VacuumPressureSample
                {
                    HeadIndex = head,
                    RawCount = result.Result,
                    CapturedAtUtc = DateTimeOffset.UtcNow
                });
                if (i < 5)
                    await Task.Delay(90, token);
            }
            // Raw counts are hardware-dependent. Allow 4% span as a starting
            // stability gate; show failure rather than accepting a noisy band.
            var spreadLimit = Math.Max(40.0,
                Math.Abs(samples.Average(s => (double)s.RawCount)) * 0.04);
            if (!VacuumPressureSampling.TryCaptureBand(
                samples, head, DateTimeOffset.UtcNow,
                TimeSpan.FromSeconds(8), 6, spreadLimit,
                out var band, out var failure))
                throw new InvalidOperationException("Head " + head + ": " + failure);
            return band;
        }

        private void StopPumps()
        {
            if (_machine?.Connected != true) return;
            _machine.LeftVacuumPump = false;
            _machine.RightVacuumPump = false;
            PumpActive = false;
        }

        private void RecordStep(string step, VacuumPressureBand band, MachineOperationVerificationKind kind)
        {
            var now = DateTimeOffset.UtcNow;
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = _session.Snapshot.RunId,
                StepKey = step,
                CapturedAtUtc = now,
                Name = "Vacuum " + step,
                Value = band == null ? "operator-confirmed" : band.MeanRawCount.ToString("F1"),
                Unit = band == null ? "confirmation" : "raw counts",
                Source = "vacuum-sensor-" + _machine.Settings.VacuumSensorTransport?.Value.ToString(),
                Verification = kind,
                Saved = false
            });
            _session.CompleteActiveStep(step, DateTimeOffset.UtcNow);
            NotifyButtons();
        }

        private void BeginSession()
        {
            var now = DateTimeOffset.UtcNow;
            _session = new MachineOperationSession(VacuumValidationProcedureDefinition.Create(),
                new MachineOperationTarget { MachineId = Convert.ToString(_machine.Settings.Id) }, now);
            _session.Start(now);
            RecordStep("readiness", null, MachineOperationVerificationKind.OperatorConfirmed);
        }

        public async Task ContinueAsync()
        {
            if (!CanContinue || Busy) return;
            Busy = true;
            try
            {
                _cancel.Token.ThrowIfCancellationRequested();
                if (!ContextSafe(_phase == Phase.AtFiducial || _phase == Phase.Sealed,
                    out var contextError))
                    throw new InvalidOperationException(contextError);

                switch (_phase)
                {
                    case Phase.Ready:
                        if (!OperatorAcknowledged)
                            throw new InvalidOperationException("Operator supervision is required.");
                        BeginSession();
                        await CaptureIdleAsync(_cancel.Token);
                        SetPhase(Phase.Idle);
                        break;
                    case Phase.Idle:
                        await CaptureOpenAsync(_cancel.Token);
                        SetPhase(Phase.Open);
                        break;
                    case Phase.Open:
                        await MoveToFiducialAsync(_cancel.Token);
                        SetPhase(Phase.AtFiducial);
                        Status = ManufacturingResources.VacuumCalibration_JogInstruction;
                        break;
                    case Phase.AtFiducial:
                        if (!OperatorConfirmedContact)
                            throw new InvalidOperationException("Operator has not confirmed plate contact.");
                        await CaptureSealedAsync(_cancel.Token);
                        SetPhase(Phase.Sealed);
                        break;
                    case Phase.Sealed:
                        await ReleaseAsync(_cancel.Token);
                        SetPhase(Phase.Released);
                        break;
                    case Phase.Released:
                        if (_head == 1)
                        {
                            _head = 2;
                            _idle = null; _oppositeIdle = null;
                            _open = null; _oppositeOpen = null; _sealed = null;
                            OperatorConfirmedContact = false;
                            Status = ManufacturingResources.VacuumCalibration_ReadyRight;
                            await CaptureIdleAsync(_cancel.Token);
                            SetPhase(Phase.Idle);
                        }
                        else
                        {
                            if (PumpActive || _machine.LeftVacuumPump || _machine.RightVacuumPump ||
                                _leftCandidate == null || _rightCandidate == null)
                                throw new InvalidOperationException("Both heads must finish safely before completion.");
                            var settled = await _machine.SpinUntilIdleAsync(15000);
                            if (!settled.Successful || _machine.Busy ||
                                _machine.ToSendQueueCount > 0 ||
                                _machine.LeftVacuumPump || _machine.RightVacuumPump)
                                throw new InvalidOperationException("Machine queue or pump output did not settle at final safe shutdown.");
                            RecordStep("both-safe", null, MachineOperationVerificationKind.MachineObserved);
                            SetPhase(Phase.Finished);
                            Status = ManufacturingResources.VacuumCalibration_Review;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                SetPhase(Phase.Faulted);
                Status = "Procedure failed: " + ex.Message + " | Verify pump and Z state before restarting.";
                try
                {
                    StopPumps();
                    if (_machine.Connected && _machine.WasMachineHomed)
                    {
                        _machine.SendSafeMoveHeight();
                        var recovered = await _machine.SpinUntilIdleAsync(15000);
                        if (!recovered.Successful || _machine.Busy)
                            Status += " | Retraction was NOT confirmed: " + recovered.ErrorMessage;
                    }
                    else
                        Status += " | Retraction unavailable: machine disconnected or not homed.";
                }
                catch (Exception cleanupError)
                {
                    Status += " | Retraction was NOT confirmed: " + cleanupError.Message;
                }
                if (_session?.Snapshot?.Status == MachineOperationRunStatus.Running)
                    _session.BlockActiveStep(ex.Message, DateTimeOffset.UtcNow);
            }
            finally
            {
                Busy = false;
            }
        }

        private async Task CaptureIdleAsync(CancellationToken token)
        {
            StopPumps();
            await Task.Delay(350, token);
            _idle = await CaptureAsync(_head, token);
            _oppositeIdle = await CaptureAsync(_head == 1 ? 2 : 1, token);
            RawReading = "Idle: " + _idle.MeanRawCount.ToString("F1") +
                " | Other head: " + _oppositeIdle.MeanRawCount.ToString("F1");
            RecordStep(_head == 1 ? "left-idle" : "right-idle",
                _idle, MachineOperationVerificationKind.MachineObserved);
            Status = ManufacturingResources.VacuumCalibration_IdleCaptured;
        }

        private async Task CaptureOpenAsync(CancellationToken token)
        {
            var mapping = _machine.Settings.GcodeMapping?.Value;
            if (mapping == null)
                throw new InvalidOperationException("Vacuum pump G-code mapping is missing.");
            if (_head == 1 &&
                (String.IsNullOrWhiteSpace(mapping.LeftVacuumOn) ||
                 String.IsNullOrWhiteSpace(mapping.LeftVacuumOff)))
                throw new InvalidOperationException("Left pump on/off commands must both be configured.");
            if (_head == 2 &&
                (String.IsNullOrWhiteSpace(mapping.RightVacuumOn) ||
                 String.IsNullOrWhiteSpace(mapping.RightVacuumOff)))
                throw new InvalidOperationException("Right pump on/off commands must both be configured.");

            if (_head == 1) _machine.LeftVacuumPump = true;
            else _machine.RightVacuumPump = true;
            PumpActive = true;
            await Task.Delay(450, token);
            _open = await CaptureAsync(_head, token);
            _oppositeOpen = await CaptureAsync(_head == 1 ? 2 : 1, token);
            var minResponse = Math.Max(10,
                3.0 * (_idle.MaximumRawCount - _idle.MinimumRawCount));
            if (Math.Abs(_open.MeanRawCount - _idle.MeanRawCount) < minResponse)
                throw new InvalidOperationException("Pump on did not produce distinguishable sensor response.");
            var maxOtherDrift = Math.Max(40.0, Math.Abs(_oppositeIdle.MeanRawCount) * 0.04);
            if (Math.Abs(_oppositeOpen.MeanRawCount - _oppositeIdle.MeanRawCount) > maxOtherDrift)
                throw new InvalidOperationException("Inactive head pressure changed unexpectedly.");

            RawReading = "Idle: " + _idle.MeanRawCount.ToString("F1") +
                " | Open: " + _open.MeanRawCount.ToString("F1");
            RecordStep(_head == 1 ? "left-open" : "right-open",
                _open, MachineOperationVerificationKind.MachineObserved);
            Status = ManufacturingResources.VacuumCalibration_OpenCaptured;
        }

        private async Task MoveToFiducialAsync(CancellationToken token)
        {
            var head = _machine.Settings.ToolHeads.First(x => x.HeadIndex == _head);
            _machine.SendSafeMoveHeight();
            var safe = await _machine.SpinUntilIdleAsync(15000);
            if (!safe.Successful)
                throw new InvalidOperationException("Safe-Z move failed: " + safe.ErrorMessage);

            token.ThrowIfCancellationRequested();
            await _machine.MoveToToolHeadAsync(head);
            if (_machine.CurrentMachineToolHead?.HeadIndex != _head)
                throw new InvalidOperationException("Tool-head selection failed.");

            // Tool coordinates are established by MoveToToolHeadAsync.
            _machine.GotoPoint(_machine.Settings.MachineFiducial);
            var result = await _machine.SpinUntilIdleAsync(20000);
            if (!result.Successful)
                throw new InvalidOperationException("Fiducial approach failed: " + result.ErrorMessage);
            await VerifyFiducialAsync(token);
        }

        private async Task VerifyFiducialAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var position = await _machine.GetCurrentLocationAsync(5000);
            var target = _machine.Settings.MachineFiducial;
            if (!position.Successful || position.Result == null ||
                Math.Abs(position.Result.X - target.X) > 0.5 ||
                Math.Abs(position.Result.Y - target.Y) > 0.5)
                throw new InvalidOperationException("Controller XY does not confirm the machine fiducial.");
        }

        private async Task CaptureSealedAsync(CancellationToken token)
        {
            await VerifyFiducialAsync(token);
            var idle = await _machine.SpinUntilIdleAsync(10000);
            if (!idle.Successful)
                throw new InvalidOperationException("Wait for manual Z jog to settle before capturing sealed pressure.");

            _sealed = await CaptureAsync(_head, token);
            var minResponse = Math.Max(10.0, 3.0 * (_idle.MaximumRawCount - _idle.MinimumRawCount));
            if (!VacuumCalibrationCandidate.TryCreate(_idle, _open, _sealed,
                minResponse, 12.0, 3.0, out var candidate, out var reason))
                throw new InvalidOperationException(reason);
            if (_head == 1) _leftCandidate = candidate;
            else _rightCandidate = candidate;

            RawReading = "Idle: " + _idle.MeanRawCount.ToString("F1") +
                " | Open: " + _open.MeanRawCount.ToString("F1") +
                " | Sealed: " + _sealed.MeanRawCount.ToString("F1");
            RecordStep(_head == 1 ? "left-seal" : "right-seal",
                _sealed, MachineOperationVerificationKind.MachineObserved);
            Status = ManufacturingResources.VacuumCalibration_SealedCaptured;
        }

        private async Task ReleaseAsync(CancellationToken token)
        {
            _machine.SendSafeMoveHeight();
            var raised = await _machine.SpinUntilIdleAsync(15000);
            if (!raised.Successful)
                throw new InvalidOperationException("Could not retract nozzle from plate.");
            StopPumps();
            await Task.Delay(450, token);
            var release = await CaptureAsync(_head, token);
            var inactive = await CaptureAsync(_head == 1 ? 2 : 1, token);
            var candidate = _head == 1 ? _leftCandidate : _rightCandidate;
            if (!candidate.IsBackAtIdle(release, 8))
                throw new InvalidOperationException("Vacuum did not return to the measured idle band.");
            var maxOtherDrift = Math.Max(40.0, Math.Abs(_oppositeIdle.MeanRawCount) * 0.04);
            if (Math.Abs(inactive.MeanRawCount - _oppositeIdle.MeanRawCount) > maxOtherDrift)
                throw new InvalidOperationException("Other head did not remain in its idle band.");
            RecordStep(_head == 1 ? "left-release" : "right-release",
                release, MachineOperationVerificationKind.MachineObserved);
            var summary = "0% = " + candidate.Idle.MeanRawCount.ToString("F0") +
                " | Open " + candidate.NoPartPercent.ToString("F1") +
                "% | 100% = " + candidate.Sealed.MeanRawCount.ToString("F0") +
                " | Part threshold " + candidate.PartPresentThresholdPercent.ToString("F1") + "%";
            if (_head == 1) LeftSummary = summary;
            else RightSummary = summary;
            Status = ManufacturingResources.VacuumCalibration_ReleaseCaptured;
        }

        public async Task AbortAsync()
        {
            if (Busy)
            {
                _cancel.Cancel();
                StopPumps();
                Status = ManufacturingResources.VacuumCalibration_AbortRequested;
                return;
            }
            Busy = true;
            try
            {
                StopPumps();
                if (!_machine.Connected || !_machine.WasMachineHomed)
                    throw new InvalidOperationException("Machine disconnected or not homed. Manually verify safe Z.");
                _machine.SendSafeMoveHeight();
                var settled = await _machine.SpinUntilIdleAsync(15000);
                if (!settled.Successful)
                    throw new InvalidOperationException("Safe-Z retraction was not confirmed: " + settled.ErrorMessage);

                if (_session?.Snapshot?.Status == MachineOperationRunStatus.Running ||
                    _session?.Snapshot?.Status == MachineOperationRunStatus.Blocked)
                    _session.Cancel(DateTimeOffset.UtcNow);
                SetPhase(Phase.Aborted);
                Status = ManufacturingResources.VacuumCalibration_Aborted;
            }
            catch (Exception ex)
            {
                SetPhase(Phase.Faulted);
                Status = "Cannot verify safe shutdown: " + ex.Message +
                    " | Use machine emergency stop and confirm Z/outputs before closing.";
            }
            finally
            {
                Busy = false;
            }
        }
    }
}
