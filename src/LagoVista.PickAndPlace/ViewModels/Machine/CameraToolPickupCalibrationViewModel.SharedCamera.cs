using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public sealed partial class CameraToolPickupCalibrationViewModel
    {
        private const bool DefaultFullDiagnostics = true;
        private bool _fullDiagnostics = DefaultFullDiagnostics;
        private string _diagnosticIterations = "10";
        private string _sharedCameraStatus = "Complete and approve both pickup offsets first. Then verify both saved pickups and propose the shared bottom camera.";
        private bool _sharedOperationActive;
        private bool _sharedCameraVerified;
        private CameraToolNozzleCenterProof _headOneCenterProof;
        private CameraToolNozzleCenterProof _headTwoCenterProof;
        private CameraToolNozzleCenterProof _visibleNozzleProof;
        private bool _sharedCandidateReady;
        private string _headOnePickupState = "Pending";
        private string _headTwoPickupState = "Pending";
        private string _headOnePickupResult = "Not measured";
        private string _headTwoPickupResult = "Not measured";
        private string _headOneDiscResult = "Not measured in this session";
        private string _headTwoDiscResult = "Not measured in this session";
        private string _headOneDiscState = "Pending";
        private string _headTwoDiscState = "Pending";
        private bool _stopShared;
        private IMachine _sharedCandidateMachine;
        private string _sharedCandidateIdentity;
        private Point2D<double> _sharedCandidate;
        private DateTimeOffset _sharedCandidateAt;
        private const string SharedStep = "shared-camera";

        public bool FullDiagnostics { get => _fullDiagnostics; set => Set(ref _fullDiagnostics, value); }
        public string DiagnosticIterations { get => _diagnosticIterations; set => Set(ref _diagnosticIterations, value); }
        public string SharedCameraStatus { get => _sharedCameraStatus; private set => Set(ref _sharedCameraStatus, value); }

        public bool SharedOperationActive => _sharedOperationActive;
        public bool SharedCandidateReady
        {
            get => _sharedCandidateReady;
            private set => Set(ref _sharedCandidateReady, value);
        }
        public CameraToolNozzleCenterProof HeadOneCenterProof
        {
            get => _headOneCenterProof;
            private set => Set(ref _headOneCenterProof, value);
        }
        public CameraToolNozzleCenterProof HeadTwoCenterProof
        {
            get => _headTwoCenterProof;
            private set => Set(ref _headTwoCenterProof, value);
        }
        public CameraToolNozzleCenterProof VisibleNozzleProof
        {
            get => _visibleNozzleProof;
            private set => Set(ref _visibleNozzleProof, value);
        }
        public string HeadOnePickupState { get => _headOnePickupState; private set => Set(ref _headOnePickupState, value); }
        public string HeadTwoPickupState { get => _headTwoPickupState; private set => Set(ref _headTwoPickupState, value); }
        public string HeadOnePickupResult { get => _headOnePickupResult; private set => Set(ref _headOnePickupResult, value); }
        public string HeadTwoPickupResult { get => _headTwoPickupResult; private set => Set(ref _headTwoPickupResult, value); }

        private void SetPickupResult(int index, string state, string result)
        {
            if (index == 1) { HeadOnePickupState = state; HeadOnePickupResult = result; }
            else if (index == 2) { HeadTwoPickupState = state; HeadTwoPickupResult = result; }
        }

        public string HeadOneDiscResult { get => _headOneDiscResult; private set => Set(ref _headOneDiscResult, value); }
        public string HeadTwoDiscResult { get => _headTwoDiscResult; private set => Set(ref _headTwoDiscResult, value); }
        public string HeadOneDiscState { get => _headOneDiscState; private set => Set(ref _headOneDiscState, value); }
        public string HeadTwoDiscState { get => _headTwoDiscState; private set => Set(ref _headTwoDiscState, value); }

        private void ResetNozzleResults()
        {
            HeadOneCenterProof = null;
            HeadTwoCenterProof = null;
            VisibleNozzleProof = null;
        }

        private void SetDiscResult(int index, string state, string result)
        {
            if (index == 1) { HeadOneDiscState = state; HeadOneDiscResult = result; }
            else { HeadTwoDiscState = state; HeadTwoDiscResult = result; }
        }

        private async Task DisplayUncorrectedNozzleAsync(MachineToolHead head, BottomCircle circle)
        {
            var proof = new CameraToolNozzleCenterProof
            {
                HeadIndex = head.HeadIndex,
                Xmm = circle.OffsetMm.X,
                Ymm = circle.OffsetMm.Y,
                DiameterMm = circle.DiameterMm,
                ResidualMm = CameraToolBottomCameraFraming.Radius(circle.OffsetMm)
            };
            if (head.HeadIndex == 1) HeadOneCenterProof = proof;
            else HeadTwoCenterProof = proof;
            VisibleNozzleProof = proof;
            try
            {
                Status = proof.Summary + " — uncorrected bottom-camera approach";
                await Task.Delay(3000);
            }
            finally { VisibleNozzleProof = null; }
        }
        public bool SharedCameraVerified
        {
            get => _sharedCameraVerified;
            private set => Set(ref _sharedCameraVerified, value);
        }

        public void RequestDiagnosticStop()
        {
            if (_sharedOperationActive)
            {
                _stopShared = true;
                SharedCameraStatus = "Stop requested; completing current guarded motion/recovery before stopping.";
            }
        }

        private void CheckSharedStop()
        {
            if (_stopShared) throw new CalibrationStopped("Operator requested stop.");
            RequireValid();
        }

        private void ClearSharedCandidate()
        {
            SharedCameraVerified = false;
            SharedCandidateReady = false;
            _sharedCandidate = null;
            _sharedCandidateMachine = null;
            _sharedCandidateIdentity = null;
        }

        private MachineCamera SharedCamera() => _machine.Settings.Cameras.Single(c =>
            c.CameraType.Value == CameraTypes.PartInspection);

        private static bool SameXY(Point2D<double> a, Point2D<double> b) =>
            a != null && b != null && Finite(a.X) && Finite(a.Y) &&
            Finite(b.X) && Finite(b.Y) &&
            Math.Abs(a.X - b.X) <= 0.00001 && Math.Abs(a.Y - b.Y) <= 0.00001;

        private static string Describe(Point2D<double> p) =>
            $"X={p.X:F3}, Y={p.Y:F3}, radial={CameraToolBottomCameraFraming.Radius(p):F3} mm";

        private MachineToolHead SharedHead(int index) =>
            _machine.Settings.ToolHeads.Single(h => h != null && h.HeadIndex == index);

        private string SharedIdentity()
        {
            var camera = SharedCamera();
            return FormattableString.Invariant($"{camera.Id}/{camera.AbsolutePosition.X:R}/{camera.AbsolutePosition.Y:R}/{camera.EffectivePixelsPerMM:R}") +
                String.Concat(new[] { 1, 2 }.Select(i =>
                {
                    var h = SharedHead(i);
                    return FormattableString.Invariant($"|{h.HeadIndex}/{h.CurrentNozzle?.Id}/{h.Offset.X:R}/{h.Offset.Y:R}/{h.PickHeight:R}/{h.PlaceHeight:R}");
                }));
        }

        private void CheckSharedReadiness()
        {
            RequireValid();
            if (_machine.Busy || _machine.IsLocating || _locator.IsLocating ||
                _machine.ToSendQueueCount != 0 || _machine.UnacknowledgedBytesSent != 0 ||
                _machine.CurrentMachineToolHead != null || _machine.LeftVacuumPump || _machine.RightVacuumPump)
                throw new CalibrationStopped("Both heads must be empty, vacuum off, and machine idle in camera mode.");
            if (!Finite(_machine.Settings.SafMoveHeight))
                throw new CalibrationStopped("Invalid safe move height.");
            var camera = SharedCamera();
            foreach (var i in new[] { 1, 2 })
            {
                var h = SharedHead(i);
                if (h.CurrentNozzle == null || !h.IsCalibrated || !h.RunoutZeroAngle.HasValue ||
                    h.RotationOffsets?.Count != NozzleRunoutMath.SampleCount ||
                    h.Offset == null || !Finite(h.Offset.X) || !Finite(h.Offset.Y) ||
                    !Finite(h.PickHeight) || !Finite(h.PlaceHeight) ||
                    Math.Abs(h.PickHeight) < 0.001 || Math.Abs(h.PlaceHeight) < 0.001 ||
                    Math.Abs(h.PickHeight - h.PlaceHeight) > 0.5 ||
                    !VacuumCalibrationProfile.TryFromHead(h, out _))
                    throw new CalibrationStopped($"Head {i}: invalid nozzle/runout, pickup/return heights, offset or vacuum calibration.");
                if (!CameraToolBottomCameraFraming.TryValidateInspectionXY(camera.AbsolutePosition,
                    h.Offset, new Point2D<double>(0, 0), _machine.Settings.WorkAreaSize, out _, out var error))
                    throw new CalibrationStopped($"Head {i}: {error}");
            }
        }

        private async Task<LagoVista.Manufacturing.Models.Machine> ReadSharedGeometryAsync()
        {
            var remote = await _restClient.GetAsync<DetailResponse<LagoVista.Manufacturing.Models.Machine>>(
                $"/api/mfg/machine/{_machine.Settings.Id}");
            var saved = remote?.Result?.Model;
            var camera = SharedCamera();
            var remoteCamera = saved?.Cameras?.SingleOrDefault(c =>
                c.CameraType.Value == CameraTypes.PartInspection);
            if (remote?.Successful != true || remoteCamera == null ||
                Convert.ToString(remoteCamera.Id) != Convert.ToString(camera.Id) ||
                !SameXY(remoteCamera.AbsolutePosition, camera.AbsolutePosition))
                throw new CalibrationStopped("Authoritative shared camera read-back differs; reload machine settings before motion.");
            foreach (var i in new[] { 1, 2 })
            {
                var h = SharedHead(i);
                var rh = saved.ToolHeads?.SingleOrDefault(t => t.HeadIndex == i);
                if (rh == null || !SameXY(rh.Offset, h.Offset))
                    throw new CalibrationStopped($"Head {i} authoritative saved pickup offset differs; reload before motion.");
            }
            return saved;
        }

        private void SharedEvidence(string detail, bool saved = false)
        {
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = _session.Snapshot.RunId,
                StepKey = SharedStep,
                Name = detail,
                Value = detail,
                Source = "T001.SharedBottomCamera",
                Verification = saved ? MachineOperationVerificationKind.MachineObserved : MachineOperationVerificationKind.VisionVerified,
                Saved = saved,
                CapturedAtUtc = DateTimeOffset.UtcNow
            });
        }

        private void SelectSharedHead(int index)
        {
            CheckSharedStop();
            _head = SharedHead(index);
            _nozzleIdentity = Convert.ToString(_head.CurrentNozzle.Id);
            _pickupZ = _head.PickHeight;
            _returnZ = _head.PlaceHeight;
            _inspectionFramingOffsetMm = null;
        }

        private async Task SharedCameraModeAsync()
        {
            RequireValid();
            _machine.SendSafeMoveHeight();
            await NeedIdleAsync("shared calibration safe Z");
            await _machine.MoveToCameraAsync();
            await NeedIdleAsync("shared calibration camera mode");
            if (_machine.CurrentMachineToolHead != null)
                throw new CalibrationStopped("Camera mode not restored.");
        }

        private async Task<double> SharedNozzleAsync(int index)
        {
            SelectSharedHead(index);
            var nozzle = await MeasureEmptyNozzleAsync(_head, frame: false, displayUncorrectedProof: true);
            var error = CameraToolBottomCameraFraming.Radius(nozzle.OffsetMm);
            SharedEvidence($"Head {index} saved camera UNCORRECTED: {Describe(nozzle.OffsetMm)}; {(error <= CameraToolBottomCameraFraming.NozzleCameraToleranceMm ? "PASS" : "FAIL")}");
            await SharedCameraModeAsync();
            CheckSharedStop();
            return error;
        }

        private async Task ValidateBothSharedNozzlesAsync()
        {
            var one = await SharedNozzleAsync(1);
            var two = await SharedNozzleAsync(2);
            if (!CameraToolBottomCameraFraming.BothHeadsPassUncorrected(one, two))
                throw new CalibrationStopped($"Saved inspection geometry disagrees: Head 1 {one:F3}, Head 2 {two:F3} mm (max 0.200 each). Do not change pickup offsets or frame into a pass.");
        }

        private async Task<double> SharedPickupAsync(int index, bool frame,
            Action<double> cameraSample = null, Action<double> pickupSample = null)
        {
            SelectSharedHead(index);
            SetDiscResult(index, "Pending", "Measuring saved-offset pickup...");
            var nozzle = await MeasureEmptyNozzleAsync(_head, frame);
            var nozzleError = CameraToolBottomCameraFraming.Radius(nozzle.OffsetMm);
            cameraSample?.Invoke(nozzleError);
            if (!frame && nozzleError > CameraToolBottomCameraFraming.NozzleCameraToleranceMm)
                throw new CalibrationStopped($"Head {index} uncorrected camera error {nozzleError:F3} mm.");
            CheckSharedStop();
            var target = await LocateCylinderAsync();
            CheckSharedStop();
            await MoveHeadToCylinderAsync(target, null);
            await PickCylinderAsync(target, null);
            SharedEvidence($"Head {index} vacuum confirmed pickup; saved offset, no candidate jog.");
            var motion = DateTimeOffset.UtcNow;
            var disc = await ObserveHeldCylinderAsync(retainFocusForSavedProof: frame);
            var observation = CreateObservation(_head, nozzle, disc, motion);
            var residual = CameraToolBottomCameraFraming.Radius(observation.PartRelativeToNozzleMm);
            pickupSample?.Invoke(residual);
            var discCameraError = CameraToolBottomCameraFraming.Radius(disc.OffsetMm);
            var displayPassed = residual <= CameraToolBottomCameraFraming.NozzleCameraToleranceMm &&
                CameraToolBottomCameraFraming.TryVerifyCenteredPickup(nozzle.OffsetMm, disc.OffsetMm,
                    out _, out _, out _);
            if (frame)
            {
                SavedPickupProof = new CameraToolSavedPickupProof
                {
                    DiscOffsetXmm = disc.OffsetMm.X,
                    DiscOffsetYmm = disc.OffsetMm.Y,
                    NozzleOffsetXmm = nozzle.OffsetMm.X,
                    NozzleOffsetYmm = nozzle.OffsetMm.Y,
                    DiscDiameterMm = disc.DiameterMm,
                    ResidualMm = residual,
                    NozzleCameraResidualMm = nozzleError,
                    DiscCameraResidualMm = discCameraError,
                    Verified = displayPassed
                };
                ShowSavedPickupRing = true;
                try { await Task.Delay(3000); }
                finally
                {
                    ShowSavedPickupRing = false;
                    _machine.SendSafeMoveHeight();
                    await NeedIdleAsync("safe Z after displayed shared pickup proof");
                }
            }
            SharedEvidence($"Head {index} {(frame ? "framed" : "uncorrected")} nozzle {Describe(nozzle.OffsetMm)}; disc {Describe(disc.OffsetMm)}; pickup residual {Describe(observation.PartRelativeToNozzleMm)}");
            await ReturnCylinderAsync(target, null);
            if (_machine.LeftVacuumPump || _machine.RightVacuumPump)
                throw new CalibrationStopped("Vacuum OFF state did not verify after disc return.");
            SharedEvidence($"Head {index} disc returned; vacuum OFF command settled and safe retract completed.");
            await SharedCameraModeAsync();
            if (residual > CameraToolBottomCameraFraming.NozzleCameraToleranceMm)
                throw new CalibrationStopped($"Head {index} saved pickup residual {residual:F3} mm exceeds 0.200 mm.");
            CheckSharedStop();
            SetDiscResult(index, displayPassed ? "Passed" : "Failed",
                $"{(displayPassed ? "PASS" : "FAIL")} · {discCameraError:F3} mm");
            return residual;
        }

        public Task ProposeSharedCameraAsync() => RunSharedAsync("propose", async () =>
        {
            ClearSharedCandidate();
            ResetNozzleResults();
            SetDiscResult(1, "Pending", "Awaiting saved-offset recheck");
            SetDiscResult(2, "Pending", "Awaiting saved-offset recheck");
            // Reprove current saved offsets, including a previously accepted Head 2;
            // no repeat write of its pickup offset is needed.
            await SharedPickupAsync(1, true);
            await SharedPickupAsync(2, true);
            SelectSharedHead(1);
            await MeasureEmptyNozzleAsync(_head);
            var camera = SharedCamera();
            var jog = _inspectionFramingOffsetMm;
            if (jog == null) throw new CalibrationStopped("No verified Head 1 framing correction.");
            if (!CameraToolBottomCameraFraming.TryProposeSharedCamera(camera.AbsolutePosition,
                SharedHead(1).Offset, SharedHead(2).Offset, jog, _machine.Settings.WorkAreaSize,
                out var candidate, out var reason))
                throw new CalibrationStopped(reason);
            await SharedCameraModeAsync();
            CheckSharedStop();
            _sharedCandidateMachine = _machine;
            _sharedCandidateIdentity = SharedIdentity();
            _sharedCandidate = candidate;
            _sharedCandidateAt = DateTimeOffset.UtcNow;
            SharedCandidateReady = true;
            SharedCameraStatus = $"UNSAVED shared camera: ({camera.AbsolutePosition.X:F3}, {camera.AbsolutePosition.Y:F3}) -> ({candidate.X:F3}, {candidate.Y:F3}) mm. Head 1 reference; both saved pickups reverified. Approve explicitly.";
            SharedEvidence(SharedCameraStatus);
        });

        public Task ApproveSharedCameraAsync() => RunSharedAsync("approve", async () =>
        {
            if (_sharedCandidate == null || !ReferenceEquals(_sharedCandidateMachine, _machine) ||
                DateTimeOffset.UtcNow - _sharedCandidateAt > TimeSpan.FromMinutes(15) ||
                _sharedCandidateIdentity != SharedIdentity())
                throw new CalibrationStopped("No current shared camera candidate, or its machine/head/camera context changed. Re-propose.");
            var camera = SharedCamera();
            var candidate = _sharedCandidate;
            // Use a fresh authoritative model for the existing machine PUT.
            // Do not persist local transient detector tuning or unrelated edits.
            var persisted = await ReadSharedGeometryAsync();
            var remoteCamera = persisted.Cameras.Single(c =>
                c.CameraType?.Value == CameraTypes.PartInspection);
            ClearSharedCandidate();
            CheckSharedStop();
            remoteCamera.AbsolutePosition = candidate;
            try
            {
                var result = await _restClient.PutAsync("/api/mfg/machine", persisted);
                if (!result.Successful)
                    throw new CalibrationStopped("Shared camera save not acknowledged: " + result.ErrorMessage);
            }
            catch
            {
                throw new CalibrationStopped("Shared camera save uncertain; inspect/reload authoritative settings before further motion.");
            }
            camera.AbsolutePosition = new Point2D<double>(candidate.X, candidate.Y);
            await ReadSharedGeometryAsync();
            SharedEvidence($"Explicitly approved shared camera saved and read back: ({candidate.X:F3}, {candidate.Y:F3}) mm.", true);
            ResetNozzleResults();
            await ValidateBothSharedNozzlesAsync();
            SharedCameraVerified = true;
            SharedCameraStatus = "PASS: shared camera saved/read back; both heads independently within 0.200 mm without correction.";
        });

        public Task ValidateSharedCameraAsync() => RunSharedAsync("validate", async () =>
        {
            SharedCameraVerified = false;
            ResetNozzleResults();
            await ValidateBothSharedNozzlesAsync();
            SharedCameraVerified = true;
            SharedCameraStatus = "PASS: both saved inspection approaches within 0.200 mm before any correction.";
        });

        public Task RunFullDiagnosticsAsync()
        {
            if (!FullDiagnostics || !Int32.TryParse(DiagnosticIterations, out var count) || count < 1 || count > 100)
            {
                SharedCameraStatus = "Enable Full Diagnostics and enter 1–100 iterations per head.";
                return Task.CompletedTask;
            }
            return RunSharedAsync("repeatability", async () =>
            {
                var errors = new Dictionary<int, List<double>> { { 1, new List<double>() }, { 2, new List<double>() } };
                var pickups = new Dictionary<int, List<double>> { { 1, new List<double>() }, { 2, new List<double>() } };
                try
                {
                    for (var n = 1; n <= count; n++)
                        foreach (var i in new[] { 1, 2 })
                        {
                            CheckSharedStop();
                            SharedEvidence($"Iteration {n}/{count}, Head {i}; uncorrected validation, no writes.");
                            await SharedPickupAsync(i, false, errors[i].Add, pickups[i].Add);
                        }
                    SharedCameraVerified = true;
                    SharedCameraStatus = $"PASS: {count} supervised iterations per head; no optical correction or settings writes.";
                }
                finally
                {
                    foreach (var i in new[] { 1, 2 })
                        foreach (var group in new[] { (Name: "camera", Values: errors[i]), (Name: "pickup", Values: pickups[i]) })
                            SharedEvidence(group.Values.Count == 0
                                ? $"Head {i} {group.Name}: no passing samples."
                                : $"Head {i} {group.Name}: {group.Values.Count}/{count} measured samples; min {group.Values.Min():F3}, max {group.Values.Max():F3}, mean {group.Values.Average():F3} mm.");
                }
            });
        }

        private async Task RunSharedAsync(string mode, Func<Task> action)
        {
            if (!CanOperate || _pendingHistorySnapshot != null)
            {
                SharedCameraStatus = "Machine or history busy; settle previous run first.";
                return;
            }
            if (_approvedCandidate != null)
            {
                SharedCameraStatus = "Resolve the pending pickup offset candidate first.";
                return;
            }
            CalibrationRunning = true;
            CalibrationFinished = false;
            _sharedOperationActive = true;
            SharedCameraStatus = mode == "propose" ? "Phase 3: verifying saved pickups and measuring Head 1 camera reference..." :
                mode == "approve" || mode == "validate" ? "Phase 3: validating saved camera with both heads..." :
                "Extended diagnostics running...";
            RaisePropertyChanged(nameof(SharedOperationActive));
            _stopShared = false;
            _machine = _repo.CurrentMachine;
            _head = null;
            _inspectionFramingOffsetMm = null;
            _possibleDiscHeld = false;
            _discReturnStarted = false;
            _possibleDiscReturnTarget = null;
            _possibleDiscReturnCandidate = null;
            var definition = new MachineOperationDefinition
            {
                Key = "camera-tool-shared-bottom-camera-" + mode,
                Version = 1,
                Title = "T001 shared bottom-camera " + mode
            };
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key = SharedStep, Title = definition.Title,
                RequiredVerification = MachineOperationVerificationKind.VisionVerified
            });
            var uiStep = new FeederCalibrationFlowStep { Number = 1, StepKey = SharedStep, Title = definition.Title };
            CalibrationSteps = new ObservableCollection<FeederCalibrationFlowStep> { uiStep };
            try
            {
                _session = new MachineOperationSession(definition, new MachineOperationTarget
                {
                    MachineId = Convert.ToString(_machine?.Settings?.Id),
                    CameraId = Convert.ToString(_machine?.Settings?.Cameras?.FirstOrDefault(c => c?.CameraType?.Value == CameraTypes.PartInspection)?.Id)
                }, DateTimeOffset.UtcNow);
                _session.Start(DateTimeOffset.UtcNow);
                NewHistorySession(_session);
                uiStep.Begin(definition.Title);
                CheckSharedReadiness();
                await ReadSharedGeometryAsync();
                CheckSharedStop();
                await action();
                SharedEvidence(SharedCameraStatus);
                _session.CompleteActiveStep(SharedStep, DateTimeOffset.UtcNow);
                uiStep.Pass(SharedCameraStatus);
                Status = SharedCameraStatus;
            }
            catch (Exception ex)
            {
                ClearSharedCandidate();
                if (_head != null && (mode == "propose" || mode == "repeatability"))
                {
                    var state = _head.HeadIndex == 1 ? HeadOneDiscState : HeadTwoDiscState;
                    if (state == "Pending") SetDiscResult(_head.HeadIndex, "Failed", "STOPPED · pickup proof incomplete");
                }
                var recovery = await RecoverPossibleDiscAsync();
                if (!_possibleDiscHeld && ValidMachine(out _) &&
                    !_machine.Busy && _machine.ToSendQueueCount == 0 &&
                    _machine.UnacknowledgedBytesSent == 0 && _machine.CurrentMachineToolHead != null)
                {
                    try { await SharedCameraModeAsync(); }
                    catch (Exception cleanup) { recovery += " Camera-mode recovery: " + cleanup.Message; }
                }
                SharedCameraStatus = "FAIL / STOP: " + ex.Message + " " + recovery;
                if (mode == "approve") SharedCameraStatus += " Inspect persisted camera position; no pickup offsets were changed.";
                Status = SharedCameraStatus;
                uiStep.Fail(Status);
                BlockHistoryStep(Status);
            }
            finally
            {
                lock (_observationGate) _pendingCircle = null;
                _locator.UnregisterCircleLocatedHandler(this);
                _emptyNozzleOpticalLock = false;
                _emptyNozzleCameraId = null;
                _emptyNozzleCameraScale = 0;
                _inspectionFramingOffsetMm = null;
                VisibleNozzleProof = null;
                ShowSavedPickupRing = false;
                _sharedOperationActive = false;
                RaisePropertyChanged(nameof(SharedOperationActive));
                _head = null;
                _machine = null;
                await CameraToolCalibrationRunFinalization.FinishAsync(
                    SaveSettledHistoryAsync, () => _session = null,
                    () => CalibrationRunning = false, () => CalibrationFinished = true);
            }
        }
    }
}
