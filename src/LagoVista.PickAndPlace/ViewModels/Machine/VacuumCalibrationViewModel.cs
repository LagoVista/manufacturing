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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Supervised, per-head vacuum validation. Only the operator can advance a
    /// hardware stage. Uses the already-configured machine fiducial as an
    /// XY test spot (no fiducial calibration). Camera -> selected tool offsets
    /// are provided by the existing machine operations; descending to the
    /// configured pick height is a separate, supervised command.
    ///
    /// This first pass does not persist calibration or claim independent
    /// solenoid proof: the legacy solenoid interface is not per-head on Marlin.
    /// </summary>
    public sealed partial class VacuumCalibrationViewModel : ViewModelBase
    {
        private enum Phase { Ready, Idle, Open, AtFiducial, AtPickHeight, Sealed, Released, Finished, Faulted, Aborted }
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
        private VacuumPressureBand _lastReleased;
        private VacuumCalibrationCandidate _leftCandidate;
        private VacuumCalibrationCandidate _rightCandidate;
        private bool _busy;
        private bool _runAllActive;
        private bool _calibrationSaved;
        private bool _pumpActive;
        private bool _operatorAcknowledged;
        private bool _operatorConfirmedContact;
        private string _status = ManufacturingResources.VacuumCalibration_Ready;
        private string _rawReading = "-";
        private string _leftSummary = "-";
        private string _rightSummary = "-";
        private int _head = 1;
        private double? _commandedNozzleZ;
        private int _approachApprovedHead;
        private int _contactConfirmedHead;
        private bool _runAllPaused;

        public VacuumCalibrationViewModel(IMachineRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _machine = repo.CurrentMachine ?? throw new InvalidOperationException("Select a machine before starting.");
            InitializeVacuumDisplay();
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
            set
            {
                if (Busy || (_phase != Phase.AtPickHeight && value)) return;
                Set(ref _operatorConfirmedContact, value);
                _contactConfirmedHead = value && _phase == Phase.AtPickHeight ? _head : 0;
                NotifyButtons();
            }
        }

        /// <summary>
        /// This is NOT the initial machine safety acknowledgement. The
        /// operator verifies fixture/nozzle clearance at this head's
        /// actual configured test location before any Z descent.
        /// A new head always requires an independent confirmation.
        /// </summary>
        public bool OperatorApprovedPickHeight
        {
            get => _phase == Phase.AtFiducial && _approachApprovedHead == _head;
            set
            {
                if (Busy || _runAllActive || _phase != Phase.AtFiducial) return;
                _approachApprovedHead = value ? _head : 0;
                NotifyButtons();
            }
        }
        public bool NeedsPickHeightApproval => !_runAllActive && _phase == Phase.AtFiducial;
        public bool AwaitingOperatorCheckpoint =>
            !_runAllActive && (_phase == Phase.AtFiducial || _phase == Phase.AtPickHeight);
        public string RunAllAction => _phase == Phase.Ready
            ? "Run Both Heads (pause for contact)"
            : "Resume Both-Head Procedure";
        public string Status
        {
            get => _status;
            private set
            {
                Set(ref _status, value);
                RefreshVacuumDisplay();
            }
        }
        public string RawReading { get => _rawReading; private set => Set(ref _rawReading, value); }
        public string LeftSummary { get => _leftSummary; private set => Set(ref _leftSummary, value); }
        public string RightSummary { get => _rightSummary; private set => Set(ref _rightSummary, value); }
        public bool RunAllActive => _runAllActive;
        public bool CanRunAll => !_runAllActive && !Busy && OperatorAcknowledged &&
            (_phase == Phase.Ready ||
             (_runAllPaused && _phase == Phase.AtFiducial && OperatorApprovedPickHeight) ||
             (_runAllPaused && _phase == Phase.AtPickHeight &&
               VacuumContactCheckpoint.CanCaptureSealed(_head, _contactConfirmedHead)));
        public bool CanApplyCalibration => !Busy && !_runAllActive && !_calibrationSaved &&
            _phase == Phase.Finished && _leftCandidate != null && _rightCandidate != null;
        public bool CanContinue => !_runAllActive && !Busy && _phase != Phase.Finished &&
            _phase != Phase.Faulted && _phase != Phase.Aborted &&
            (_phase != Phase.Ready || OperatorAcknowledged) &&
            (_phase != Phase.AtFiducial ||
                VacuumContactCheckpoint.CanDescend(_head, _approachApprovedHead, OperatorAcknowledged)) &&
            (_phase != Phase.AtPickHeight ||
                VacuumContactCheckpoint.CanCaptureSealed(_head, _contactConfirmedHead));
        public bool CanAbort => _phase != Phase.Aborted && _phase != Phase.Finished;
        public bool CanClose => !_runAllActive && !Busy && !PumpActive && (_phase == Phase.Ready || _phase == Phase.Aborted || _phase == Phase.Finished);
        public bool NeedsPlateContact => !_runAllActive && _phase == Phase.AtPickHeight;
        public string NextAction => _phase == Phase.Ready ? ManufacturingResources.VacuumCalibration_CaptureIdle :
            _phase == Phase.Idle ? ManufacturingResources.VacuumCalibration_PumpOpen :
            _phase == Phase.Open ? ManufacturingResources.VacuumCalibration_MoveFiducial :
            _phase == Phase.AtFiducial ? ManufacturingResources.VacuumCalibration_LowerPick :
            _phase == Phase.AtPickHeight ? ManufacturingResources.VacuumCalibration_CaptureSealed :
            _phase == Phase.Sealed ? ManufacturingResources.VacuumCalibration_Release :
            _phase == Phase.Released ? ManufacturingResources.VacuumCalibration_NextHead :
            ManufacturingResources.VacuumCalibration_Complete;
        public string ProcedureStatus => _session?.Snapshot?.Status.ToString() ?? "Pending";

        private void NotifyButtons()
        {
            RaisePropertyChanged(nameof(CanContinue));
            RaisePropertyChanged(nameof(CanRunAll));
            RaisePropertyChanged(nameof(OperatorApprovedPickHeight));
            RaisePropertyChanged(nameof(NeedsPickHeightApproval));
            RaisePropertyChanged(nameof(AwaitingOperatorCheckpoint));
            RaisePropertyChanged(nameof(RunAllAction));
            RaisePropertyChanged(nameof(CanApplyCalibration));
            RaisePropertyChanged(nameof(RunAllActive));
            RaisePropertyChanged(nameof(CanAbort));
            RaisePropertyChanged(nameof(CanClose));
            RaisePropertyChanged(nameof(NeedsPlateContact));
            RaisePropertyChanged(nameof(NextAction));
            RaisePropertyChanged(nameof(ProcedureStatus));
            RaisePropertyChanged(nameof(HeadIndex));
            RaisePropertyChanged(nameof(HeadDescription));
            RefreshVacuumDisplay();
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

        private async Task WaitForActuatorIdleAsync(string stage, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var settled = await _machine.SpinUntilIdleAsync(15000);
            token.ThrowIfCancellationRequested();
            // SpinUntilIdleAsync is the existing machine motion-completion
            // contract (M400). Do not add a second instantaneous gate on
            // Busy / queue / unacknowledged-byte counters: these track other
            // layers and can transiently remain nonzero after a good wait.
            // The movement stages also verify their expected actual XY via M114.
            if (!settled.Successful)
                throw new InvalidOperationException(
                    stage + ": motion-completion wait failed: " + settled.ErrorMessage);
        }

        private async Task<VacuumPressureBand> CaptureAsync(int head, CancellationToken token, VacuumPressureBand idleBaseline = null)
        {
            // Each attempt collects an independent, fresh six-reading window.
            // A noisy window is never silently clamped or included in the
            // candidate. Permit brief sensor settling, but fail closed after
            // three windows and retain the raw counts in the operator UI.
            const int samplesPerWindow = 6;
            const int maxWindows = 3;
            for (var window = 1; window <= maxWindows; window++)
            {
                var samples = new List<VacuumPressureSample>();
                for (var i = 0; i < samplesPerWindow; i++)
                {
                    token.ThrowIfCancellationRequested();
                    if (!ContextSafe(_phase == Phase.AtFiducial || _phase == Phase.AtPickHeight ||
                    _phase == Phase.Sealed,
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
                    if (i < samplesPerWindow - 1)
                        await Task.Delay(150, token);
                }

                var mean = samples.Average(s => (double)s.RawCount);
                var spreadLimit = VacuumPressureSampling.AllowedRawSpread(idleBaseline, mean);
                var accepted = VacuumPressureSampling.TryCaptureBand(
                    samples, head, DateTimeOffset.UtcNow,
                    TimeSpan.FromSeconds(8), samplesPerWindow, spreadLimit,
                    out var band, out var failure);

                var observed = String.Join(", ", samples.Select(s => s.RawCount.ToString()));
                RawReading = "Head " + head + " sample window " + window + "/" + maxWindows +
                    ": [" + observed + "] | span " +
                    (samples.Max(s => s.RawCount) - samples.Min(s => s.RawCount)) +
                    " counts | " + (idleBaseline == null
                        ? "idle noise baseline (not yet scaled)"
                        : "limit " + spreadLimit.ToString("F1") + " counts");

                if (accepted)
                {
                    // Only a fresh, stable, accepted sensor window counts as
                    // a measured value. Never label commanded pump state
                    // as sensor proof in the operator dashboard.
                    RecordAcceptedVacuumBand(head, band);
                    return band;
                }

                if (!failure.StartsWith("Pressure has not settled", StringComparison.Ordinal))
                    throw new InvalidOperationException("Head " + head + ": " + failure);
                if (window == maxWindows)
                    throw new InvalidOperationException("Head " + head +
                        ": baseline remained unstable after " + maxWindows +
                        " fresh sample windows. " + failure);

                // A late settled window can pass, but the stability requirement
                // remains the same for every window and every pressure stage.
                await Task.Delay(600, token);
            }
            throw new InvalidOperationException("Pressure sampling did not complete.");
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
                Source = band == null ? "operator" :
                    "vacuum-sensor-" + _machine.Settings.VacuumSensorTransport?.Value.ToString(),
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

        /// <summary>
        /// Runs the same already-proven supervised stages without requiring a
        /// Next click between each one. A single operator readiness approval
        /// covers both configured tool contact heights. There is no implied
        /// physical contact confirmation: the open/sealed sensor evidence
        /// must pass the existing calibration candidate checks.
        /// </summary>
        /// <summary>
        /// Explicit operator-approved persistence after both heads have
        /// passed. Reuse existing persisted tool-head fields so older machine
        /// settings and server API contracts need no migration.
        /// </summary>
        public async Task ApplyCalibrationAsync()
        {
            if (!CanApplyCalibration)
                return;
            Busy = true;
            var heads = _machine.Settings.ToolHeads
                .Where(h => h.HeadIndex == 1 || h.HeadIndex == 2).ToList();
            var before = heads.Select(h => new
            {
                Head = h,
                Idle = h.IdleVacuum,
                Open = h.NoPartPickedVacuum,
                Sealed = h.PartPickedVacuum,
                ThresholdFraction = h.PercentAboveNoPartPicked,
                Tolerance = h.VacuumTolerancePercent
            }).ToList();

            try
            {
                if (!ReferenceEquals(_repo.CurrentMachine, _machine) ||
                    _machine.LeftVacuumPump || _machine.RightVacuumPump ||
                    heads.Count != 2 || heads.Select(h => h.HeadIndex).Distinct().Count() != 2)
                    throw new InvalidOperationException(
                        "The same machine and both safely stopped tool heads are required.");

                foreach (var h in heads)
                {
                    var candidate = h.HeadIndex == 1 ? _leftCandidate : _rightCandidate;
                    if (candidate == null || candidate.HeadIndex != h.HeadIndex)
                        throw new InvalidOperationException(
                            "Missing validated candidate for Head " + h.HeadIndex);

                    h.IdleVacuum = candidate.Idle.MeanRawCount;
                    h.NoPartPickedVacuum = candidate.NoPart.MeanRawCount;
                    h.PartPickedVacuum = candidate.Sealed.MeanRawCount;
                    // Midpoint between observed open and sealed responses.
                    // Saved raw readings remain signed; normalization works
                    // for either polarity and is evaluated at read time.
                    h.PercentAboveNoPartPicked = 50.0;
                    h.VacuumTolerancePercent = candidate.HysteresisPercent;

                    if (!VacuumCalibrationProfile.TryFromHead(h, out var profile) ||
                        profile.Classify(h.NoPartPickedVacuum) != VacuumPickupClassification.NoPart ||
                        profile.Classify(h.PartPickedVacuum) != VacuumPickupClassification.PartPresent)
                        throw new InvalidOperationException(
                            "Saved profile cannot classify Head " + h.HeadIndex);
                }

                var saved = await _repo.SaveCurrentMachineAsync();
                if (!saved.Successful)
                    throw new InvalidOperationException(saved.ErrorMessage ??
                        "Machine configuration API rejected the save.");

                _calibrationSaved = true;
                Status = ManufacturingResources.VacuumCalibration_Saved;
            }
            catch (Exception ex)
            {
                // Restore the live in-memory machine settings if the server
                // save fails; do not leave a half-applied local configuration.
                foreach (var previous in before)
                {
                    previous.Head.IdleVacuum = previous.Idle;
                    previous.Head.NoPartPickedVacuum = previous.Open;
                    previous.Head.PartPickedVacuum = previous.Sealed;
                    previous.Head.PercentAboveNoPartPicked = previous.ThresholdFraction;
                    previous.Head.VacuumTolerancePercent = previous.Tolerance;
                }
                Status = "Calibration was not saved; previous local tool-head settings restored. " +
                    ex.Message;
            }
            finally
            {
                Busy = false;
            }
        }

        public async Task RunAllAsync()
        {
            if (!CanRunAll) return;
            if (!ValidateAutomaticPreflight(out var reason))
            {
                Status = "Run Both Heads not started: " + reason;
                return;
            }

            _runAllPaused = false;
            _runAllActive = true;
            NotifyButtons();
            try
            {
                // Ready -> idle -> pump/open -> test spot -> pick height ->
                // obstructed -> retract/release, repeated for Tool 2. Reuse
                // AdvanceAsync so there is only ONE hardware motion path.
                for (var step = 0; step < 16 &&
                    _phase != Phase.Finished && _phase != Phase.Faulted &&
                    _phase != Phase.Aborted; step++)
                {
                    // Never let the generic initial approval become
                    // authorization for head-specific Z descent or for
                    // claiming sealed nozzle contact. Pause BEFORE either
                    // hardware action and require explicit head approval.
                    if (_phase == Phase.AtFiducial &&
                        !VacuumContactCheckpoint.CanDescend(
                            _head, _approachApprovedHead, OperatorAcknowledged))
                    {
                        _runAllPaused = true;
                        Status = "Paused for Head " + _head +
                            ": verify fixture clearance and PickHeight, check the Head " +
                            _head + " descent approval, then Resume. Pump remains commanded on.";
                        return;
                    }
                    if (_phase == Phase.AtPickHeight &&
                        !VacuumContactCheckpoint.CanCaptureSealed(
                            _head, _contactConfirmedHead))
                    {
                        _runAllPaused = true;
                        Status = "Paused at configured PickHeight for Head " + _head +
                            ": visually verify a lightly sealed nozzle (no collision), " +
                            "check Contact Confirmed, then Resume. Use Abort to retract and vent.";
                        return;
                    }
                    var previousPhase = _phase;
                    var previousHead = _head;
                    await AdvanceAsync(automatic: true);

                    if (_phase == previousPhase && _head == previousHead &&
                        _phase != Phase.Faulted && _phase != Phase.Aborted)
                    {
                        // A non-advancing state must not spin endlessly with a
                        // running pump. Cancel and reuse existing safe cleanup.
                        _cancel.Cancel();
                        await AdvanceAsync(automatic: true);
                        break;
                    }
                }
                if (_phase != Phase.Finished && _phase != Phase.Faulted &&
                    _phase != Phase.Aborted)
                {
                    _cancel.Cancel();
                    await AdvanceAsync(automatic: true);
                }
            }
            finally
            {
                _runAllActive = false;
                if (_phase == Phase.Finished || _phase == Phase.Faulted ||
                    _phase == Phase.Aborted)
                    _runAllPaused = false;
                NotifyButtons();
            }
        }

        private bool ValidateAutomaticPreflight(out string reason)
        {
            if (!OperatorAcknowledged)
            {
                reason = "Operator safety acknowledgement is required.";
                return false;
            }
            if (!ContextSafe(false, out reason))
                return false;

            var settings = _machine.Settings;
            foreach (var headIndex in new[] { 1, 2 })
            {
                var tool = settings.ToolHeads.FirstOrDefault(h => h.HeadIndex == headIndex);
                if (tool == null || tool.CurrentNozzle == null ||
                    !Finite(tool.PickHeight) || tool.PickHeight <= 0 ||
                    tool.PickHeight >= settings.SafMoveHeight ||
                    tool.Offset == null ||
                    !Finite(tool.Offset.X) || !Finite(tool.Offset.Y))
                {
                    reason = "Tool " + headIndex +
                        " requires an installed nozzle, valid offsets, and a safe configured PickHeight.";
                    return false;
                }
                var x = settings.MachineFiducial.X + tool.Offset.X;
                var y = settings.MachineFiducial.Y + tool.Offset.Y;
                if (x <= 0 || y <= 0 ||
                    x >= settings.WorkAreaSize.X || y >= settings.WorkAreaSize.Y)
                {
                    reason = "Tool " + headIndex + " offset moves the test spot outside the configured work area.";
                    return false;
                }
            }

            reason = null;
            return true;
        }

        public Task ContinueAsync() => AdvanceAsync(automatic: false);

        private async Task AdvanceAsync(bool automatic)
        {
            if (Busy || _phase == Phase.Finished || _phase == Phase.Faulted ||
                _phase == Phase.Aborted || (automatic ? !_runAllActive : !CanContinue))
                return;
            Busy = true;
            try
            {
                _cancel.Token.ThrowIfCancellationRequested();
                if (!ContextSafe(_phase == Phase.AtFiducial || _phase == Phase.AtPickHeight ||
                    _phase == Phase.Sealed,
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
                        Status = ManufacturingResources.VacuumCalibration_CameraReady;
                        break;
                    case Phase.AtFiducial:
                        if (!VacuumContactCheckpoint.CanDescend(
                            _head, _approachApprovedHead, OperatorAcknowledged))
                            throw new InvalidOperationException(
                                "Head " + _head + " requires explicit operator clearance before PickHeight descent.");
                        await LowerToPickHeightAsync(_cancel.Token);
                        _approachApprovedHead = 0;
                        OperatorConfirmedContact = false;
                        SetPhase(Phase.AtPickHeight);
                        Status = ManufacturingResources.VacuumCalibration_PickReady;
                        break;
                    case Phase.AtPickHeight:
                        // Operator confirmation is required in BOTH guided
                        // and Run Both modes. The independent stable vacuum
                        // candidate check remains authoritative afterward.
                        if (!VacuumContactCheckpoint.CanCaptureSealed(
                            _head, _contactConfirmedHead))
                            throw new InvalidOperationException(
                                "Head " + _head + " nozzle obstruction has not been independently confirmed.");
                        await CaptureSealedAsync(_cancel.Token);
                        _contactConfirmedHead = 0;
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
                            _approachApprovedHead = 0;
                            _contactConfirmedHead = 0;
                            _commandedNozzleZ = null;
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
                            if (!settled.Successful)
                                throw new InvalidOperationException(
                                    "Final motion-completion wait failed: " + settled.ErrorMessage);
                            if (_machine.LeftVacuumPump || _machine.RightVacuumPump)
                                throw new InvalidOperationException(
                                    "A vacuum pump is still commanded on at final shutdown.");
                            if (_lastReleased == null)
                                throw new InvalidOperationException("Missing measured idle-recovery evidence.");
                            RecordStep("both-safe", _lastReleased, MachineOperationVerificationKind.MachineObserved);
                            SetPhase(Phase.Finished);
                            Status = ManufacturingResources.VacuumCalibration_Review;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Idle and pump-on capture never commanded nozzle movement.
                // Sensor noise during those stages must not trigger an
                // unexpected physical Z move as part of error cleanup.
                var motionWasAttempted = _phase == Phase.Open ||
                    _phase == Phase.AtFiducial || _phase == Phase.AtPickHeight ||
                    _phase == Phase.Sealed || _phase == Phase.Released;
                SetPhase(Phase.Faulted);
                Status = "Procedure failed: " + ex.Message + " | Verify pump and Z state before restarting.";
                try
                {
                    StopPumps();
                    if (_machine.Connected && _machine.WasMachineHomed)
                    {
                        if (motionWasAttempted)
                        {
                            if (_machine.CurrentMachineToolHead != null)
                                _machine.SetToolHeadHeight(_machine.Settings.SafMoveHeight);
                            _machine.SendSafeMoveHeight();
                        }
                        var recovered = await _machine.SpinUntilIdleAsync(15000);
                        if (!recovered.Successful)
                            Status += " | Safe shutdown was NOT confirmed: " + recovered.ErrorMessage;
                        else if (!motionWasAttempted)
                            Status += " | Pumps off; no calibration Z move attempted.";
                    }
                    else
                        Status += " | Safe shutdown unavailable: machine disconnected or not homed.";
                }
                catch (Exception cleanupError)
                {
                    Status += " | Safe shutdown was NOT confirmed: " + cleanupError.Message;
                }
                if (_session?.Snapshot?.Status == MachineOperationRunStatus.Running)
                    _session.BlockActiveStep(ex.Message, DateTimeOffset.UtcNow);
                RefreshVacuumDisplay();
            }
            finally
            {
                Busy = false;
            }
        }

        private async Task CaptureIdleAsync(CancellationToken token)
        {
            StopPumps();
            await WaitForActuatorIdleAsync("Idle pump-off", token);
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
            await WaitForActuatorIdleAsync("Open-nozzle pump-on", token);
            await Task.Delay(450, token);
            _open = await CaptureAsync(_head, token, _idle);
            _oppositeOpen = await CaptureAsync(_head == 1 ? 2 : 1, token);
            var minResponse = VacuumPressureSampling.RequiredResponse(_idle);
            if (Math.Abs(_open.MeanRawCount - _idle.MeanRawCount) < minResponse)
                throw new InvalidOperationException("Pump on did not produce distinguishable sensor response.");
            var maxOtherDrift = Math.Max(40.0, 3.0 * Math.Max(
                _oppositeIdle.MaximumRawCount - _oppositeIdle.MinimumRawCount,
                _oppositeOpen.MaximumRawCount - _oppositeOpen.MinimumRawCount));
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
            // MachineFiducial is a pre-configured XY position used as a
            // convenient obstruction test spot. No fiducial detection or
            // origin adjustment is performed by this operation.
            var settings = _machine.Settings;
            var tool = settings.ToolHeads.Single(x => x.HeadIndex == _head);
            var point = settings.MachineFiducial;
            var area = settings.WorkAreaSize;
            var physicalX = point.X + tool.Offset.X;
            var physicalY = point.Y + tool.Offset.Y;
            if (!Finite(tool.Offset.X) || !Finite(tool.Offset.Y) ||
                physicalX <= 0 || physicalX >= area.X ||
                physicalY <= 0 || physicalY >= area.Y)
                throw new InvalidOperationException(
                    "Camera-to-tool offset would place tool outside the configured work area.");

            // Retract selected tool before returning to camera coordinates.
            if (_machine.CurrentMachineToolHead != null)
            {
                _machine.SetToolHeadHeight(settings.SafMoveHeight);
                await WaitForActuatorIdleAsync("Selected nozzle safe Z", token);
            }
            _machine.SendSafeMoveHeight();
            await WaitForActuatorIdleAsync("Machine safe Z", token);

            token.ThrowIfCancellationRequested();
            await _machine.MoveToCameraAsync();
            await WaitForActuatorIdleAsync("Camera selection", token);
            if (_machine.CurrentMachineToolHead != null)
                throw new InvalidOperationException("Machine did not switch to camera coordinates.");

            // 1. Camera directly above the existing machine test position.
            _machine.GotoPoint(point);
            await WaitForActuatorIdleAsync("Camera at test spot", token);
            await VerifyFiducialAsync(token);

            // 2. Switch to the selected tool using the machine's established
            // camera-to-tool offset, not a new hand-authored offset.
            await _machine.MoveToToolHeadAsync(tool);
            await WaitForActuatorIdleAsync("Select Tool " + _head, token);
            if (_machine.CurrentMachineToolHead?.HeadIndex != _head)
                throw new InvalidOperationException("Tool " + _head + " selection failed.");

            // The tool must remain retracted until the operator advances.
            _machine.SetToolHeadHeight(settings.SafMoveHeight);
            await WaitForActuatorIdleAsync("Tool Z safe", token);
            _commandedNozzleZ = settings.SafMoveHeight;
            await VerifyFiducialAsync(token);
            NotifyButtons();
        }

        /// <summary>
        /// Operator-triggered descent to the already calibrated PickHeight.
        /// No probing, automatic contact search, or new fiducial calibration.
        /// </summary>
        private async Task LowerToPickHeightAsync(CancellationToken token)
        {
            if (!ContextSafe(true, out var reason))
                throw new InvalidOperationException(reason);
            await VerifyFiducialAsync(token);
            var settings = _machine.Settings;
            var tool = settings.ToolHeads.Single(x => x.HeadIndex == _head);
            var pickHeight = tool.PickHeight;
            if (!Finite(pickHeight) || pickHeight <= 0 ||
                pickHeight >= settings.SafMoveHeight)
                throw new InvalidOperationException(
                    "Configured Tool " + _head + " PickHeight must be positive and below safe Z.");

            if (!_commandedNozzleZ.HasValue ||
                Math.Abs(_commandedNozzleZ.Value - settings.SafMoveHeight) > 0.01)
                throw new InvalidOperationException("Cannot lower: tool safe-height position was not established.");

            // The operator already approved the test location and the
            // configured pick height. This is one known absolute Z move.
            _machine.SetToolHeadHeight(pickHeight);
            await WaitForActuatorIdleAsync("Tool " + _head + " configured pick height", token);
            _commandedNozzleZ = pickHeight;
            NotifyButtons();
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
            var expectedPick = _machine.Settings.ToolHeads.Single(x => x.HeadIndex == _head).PickHeight;
            if (!_commandedNozzleZ.HasValue ||
                Math.Abs(_commandedNozzleZ.Value - expectedPick) > 0.01)
                throw new InvalidOperationException("Tool has not reached the configured pick height.");
            var idle = await _machine.SpinUntilIdleAsync(10000);
            if (!idle.Successful)
                throw new InvalidOperationException("Wait for configured pick-height move to settle before sampling.");

            _sealed = await CaptureAsync(_head, token, _idle);
            var minResponse = VacuumPressureSampling.RequiredResponse(_idle);
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
            // Retract selected nozzle explicitly before switching off vacuum.
            _machine.SetToolHeadHeight(_machine.Settings.SafMoveHeight);
            await WaitForActuatorIdleAsync("Nozzle retract", token);
            _commandedNozzleZ = _machine.Settings.SafMoveHeight;
            _machine.SendSafeMoveHeight();
            await WaitForActuatorIdleAsync("Common Z retract", token);
            StopPumps();
            await WaitForActuatorIdleAsync("Vacuum release", token);
            await Task.Delay(450, token);
            var release = await CaptureAsync(_head, token);
            var inactive = await CaptureAsync(_head == 1 ? 2 : 1, token);
            var candidate = _head == 1 ? _leftCandidate : _rightCandidate;
            if (!candidate.IsBackAtIdle(release, 8))
                throw new InvalidOperationException("Vacuum did not return to the measured idle band.");
            _lastReleased = release;
            var maxOtherDrift = Math.Max(40.0, 3.0 * Math.Max(
                _oppositeIdle.MaximumRawCount - _oppositeIdle.MinimumRawCount,
                _oppositeOpen.MaximumRawCount - _oppositeOpen.MinimumRawCount));
            if (Math.Abs(inactive.MeanRawCount - _oppositeIdle.MeanRawCount) > maxOtherDrift)
                throw new InvalidOperationException("Other head did not remain in its idle band.");
            RecordStep(_head == 1 ? "left-release" : "right-release",
                release, MachineOperationVerificationKind.MachineObserved);
            var summary = "Open 0% (" + candidate.NoPart.MeanRawCount.ToString("F0") +
                ") | Obstructed 100% (" + candidate.Sealed.MeanRawCount.ToString("F0") +
                ") | Part threshold " + candidate.PartPresentThresholdPercent.ToString("F0") +
                "% | Idle reference " + candidate.Idle.MeanRawCount.ToString("F0");
            if (_head == 1) LeftSummary = summary;
            else RightSummary = summary;
            Status = ManufacturingResources.VacuumCalibration_ReleaseCaptured;
        }

        public async Task AbortAsync()
        {
            if (Busy || _runAllActive)
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
                if (_machine.CurrentMachineToolHead != null)
                    _machine.SetToolHeadHeight(_machine.Settings.SafMoveHeight);
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
