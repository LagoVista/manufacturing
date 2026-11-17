using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// One-start, machine-observed two-pass calibration, shown in a dedicated
    /// WPF modal just like T004 nozzle runout. This is explicitly an early
    /// supervised hardware procedure: operator supplies fixture-specific
    /// pickup and return Z, and auto-save defaults OFF.
    /// </summary>
    public sealed class CameraToolPickupCalibrationViewModel : ViewModelBase,
        ICameraToolPickupCalibrationViewModel, ICircleLocatedHandler
    {
        private readonly IMachineRepo _repo;
        private readonly ILocatorViewModel _locator;
        private readonly IImageCaptureService _bottomCameraBootstrap;
        private readonly KnownLocationCylinderCenteringRunner _knownLocation;
        private readonly object _observationGate = new object();
        private TaskCompletionSource<BottomCircle> _pendingCircle;
        private string _expectedBottomProfile;
        // Diagnostics for each live bottom-camera acquisition; camera callbacks
        // may arrive off the UI thread, so UI updates happen in the await loop.
        private int _bottomCircleCandidateFrames;
        private int _bottomCircleStableFrames;
        private int _bottomCircleWrongSizeFrames;
        private int _bottomCircleContextRejectedFrames;
        // Transient, per-run optical proof for the installed nozzle only.
        // Re-established through exactly the same T004 tuning/lock checks.
        private bool _emptyNozzleOpticalLock;
        private string _emptyNozzleCameraId;
        private double _emptyNozzleCameraScale;
        private IMachine _machine;
        private MachineToolHead _head;
        private string _nozzleIdentity;
        private MachineOperationSession _session;
        private double _pickupZ;
        private double _returnZ;
        private string _headIndexText = "1";
        private string _status = "Select Head 1 or 2. Pickup/return use saved head heights; bottom inspection uses machine safe Z. First run must be supervised.";
        private string _candidateSummary = "No measured candidate yet.";
        private string _toolHeadName = "Installed heads";
        private bool _running;
        private bool _finished = true;
        private bool _autoSave;
        private ObservableCollection<FeederCalibrationFlowStep> _steps =
            new ObservableCollection<FeederCalibrationFlowStep>();

        private sealed class BottomCircle
        {
            public Point2D<double> OffsetMm { get; set; }
            public double DiameterMm { get; set; }
            public DateTimeOffset CapturedAtUtc { get; set; }
        }

        private sealed class CalibrationStopped : Exception
        {
            public CalibrationStopped(string message) : base(message) { }
        }

        public CameraToolPickupCalibrationViewModel(
            IMachineRepo repo, ILocatorViewModel locator,
            IImageCaptureService bottomCameraBootstrap)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
            _bottomCameraBootstrap = bottomCameraBootstrap ??
                throw new ArgumentNullException(nameof(bottomCameraBootstrap));
            // The modal owns step-level failure reporting, so avoid a second,
            // duplicate machine warning from the nested centering runner.
            _knownLocation = new KnownLocationCylinderCenteringRunner(
                repo, locator, emitWarnings:false);
        }

        public string HeadIndexText
        {
            get => _headIndexText;
            set
            {
                Set(ref _headIndexText, value);
                RaisePropertyChanged(nameof(ConfiguredHeights));
            }
        }

        public string ConfiguredHeights
        {
            get
            {
                var machine = _repo.CurrentMachine;
                if (machine?.Settings?.ToolHeads == null ||
                    !Int32.TryParse(HeadIndexText, out var selected) ||
                    (selected != 1 && selected != 2))
                    return "Select an installed head to view the machine-configured heights.";
                var head = machine.Settings.ToolHeads.FirstOrDefault(h => h.HeadIndex == selected);
                if (head == null)
                    return "No configured tool head at that index.";
                return $"Head {head.HeadIndex}: Pick {head.PickHeight:F2} mm / " +
                    $"Place {head.PlaceHeight:F2} mm (ZL/ZR). " +
                    $"Common safe move Z {machine.Settings.SafMoveHeight:F2} mm.";
            }
        }
        public bool SaveVerifiedOffset { get => _autoSave; set => Set(ref _autoSave, value); }
        public string Status { get => _status; private set => Set(ref _status, value); }
        public string CandidateSummary
        {
            get => _candidateSummary;
            private set => Set(ref _candidateSummary, value);
        }
        public string ToolHeadName { get => _toolHeadName; private set => Set(ref _toolHeadName, value); }
        public bool CalibrationRunning
        {
            get => _running;
            private set => Set(ref _running, value);
        }
        public bool CalibrationFinished
        {
            get => _finished;
            private set => Set(ref _finished, value);
        }
        public ObservableCollection<FeederCalibrationFlowStep> CalibrationSteps
        {
            get => _steps;
            private set => Set(ref _steps, value);
        }

        public async Task StartAsync()
        {
            if (CalibrationRunning)
                return;
            CalibrationRunning = true;
            CalibrationFinished = false;
            CandidateSummary = "No verified offset yet.";
            _machine = _repo.CurrentMachine;
            _head = null;
            _session = null;
            _emptyNozzleOpticalLock = false;
            _emptyNozzleCameraId = null;
            _emptyNozzleCameraScale = 0;
            try
            {
                if (!Int32.TryParse(HeadIndexText, out var selectedHeadIndex) ||
                    (selectedHeadIndex != 1 && selectedHeadIndex != 2))
                    throw new CalibrationStopped("Select exactly one physical head (1 or 2) for the first supervised hardware test.");
                // T004's measured nozzle rotational axis is not yet accepted
                // into T003's source authority. This first machine trial can
                // verify two pickups, but must not write a static translation
                // based on a temporary same-angle empty-nozzle reference.
                if (SaveVerifiedOffset)
                    throw new CalibrationStopped(
                        "Auto-save is disabled for the first physical trial until T004 nozzle-axis integration and hardware verification. Uncheck Auto-save.");

                if (!ValidMachine(out var error))
                    throw new CalibrationStopped(error + " No movement was sent.");
                if (_machine.Settings.ToolHeads == null)
                    throw new CalibrationStopped("No installed tool heads are configured.");

                var heads = _machine.Settings.ToolHeads
                    .Where(h => h?.CurrentNozzle != null && h.HeadIndex == selectedHeadIndex)
                    .OrderBy(h => h.HeadIndex)
                    .ToList();
                if (heads.Count == 0)
                    throw new CalibrationStopped("The selected head has no installed nozzle.");
                if (heads.Any(h => h.HeadIndex < 1 || h.HeadIndex > 2 || h.Offset == null))
                    throw new CalibrationStopped("An installed head has no supported index or XY offset.");
                var plan = new ObservableCollection<FeederCalibrationFlowStep>();
                foreach (var head in heads)
                {
                    foreach (var stage in CameraToolPickupProcedureDefinition.Create().Steps)
                        plan.Add(new FeederCalibrationFlowStep
                        {
                            Number = plan.Count + 1,
                            StepKey = $"head-{head.HeadIndex}/{stage.Key}",
                            Title = $"Head {head.HeadIndex} - {stage.Title}"
                        });
                }
                CalibrationSteps = plan;

                // One start automatically advances through each installed head;
                // the first hardware trial should be attended for safety.
                foreach (var head in heads)
                {
                    ToolHeadName = $"Head {head.HeadIndex} - {head.Name}";
                    await ExecuteHeadAsync(head);
                }
                Status = SaveVerifiedOffset
                    ? "Selected head passed both pickups; verified offset was saved."
                    : "Selected head passed both pickups. Candidate remains UNSAVED (auto-save disabled).";
            }
            catch (CalibrationStopped ex)
            {
                Status = "Calibration stopped: " + ex.Message;
                _machine?.AddStatusMessage(StatusMessageTypes.Warning, Status);
                var active = CalibrationSteps.FirstOrDefault(s => s.Status == "Running");
                active?.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                Status = "Calibration failed: " + ex.Message;
                _machine?.AddStatusMessage(StatusMessageTypes.Warning, Status);
                var active = CalibrationSteps.FirstOrDefault(s => s.Status == "Running");
                active?.Fail(ex.Message);
            }
            finally
            {
                // Keep vacuum ON if a pickup failed away from the holder;
                // never drop a held cylinder into the machine on an exception.
                // Operator uses the independent feed hold / emergency stop.
                lock (_observationGate) _pendingCircle = null;
                _emptyNozzleOpticalLock = false;
                _emptyNozzleCameraId = null;
                _emptyNozzleCameraScale = 0;
                _locator.UnregisterCircleLocatedHandler(this);
                _session = null;
                _head = null;
                _machine = null;
                CalibrationRunning = false;
                CalibrationFinished = true;
            }
        }

        private async Task ExecuteHeadAsync(MachineToolHead head)
        {
            _head = head;
            _nozzleIdentity = Convert.ToString(head.CurrentNozzle.Id);
            var definition = CameraToolPickupProcedureDefinition.Create();
            _session = new MachineOperationSession(definition,
                new MachineOperationTarget { MachineId = Convert.ToString(_machine.Settings.Id) },
                DateTimeOffset.UtcNow);
            _session.Start(DateTimeOffset.UtcNow);

            Begin("readiness", "Checking Manual machine, T004 runout and configured head pickup/return heights.");
            RequireValid();
            if (head.CurrentNozzle == null ||
                !String.Equals(Convert.ToString(head.CurrentNozzle.Id), _nozzleIdentity, StringComparison.Ordinal))
                throw new CalibrationStopped("Installed nozzle changed.");
            // T004 owns this session-only proof. A fresh empty-nozzle image
            // is NOT equivalent to completing its 12-angle runout procedure.
            if (!head.IsCalibrated || !head.RunoutZeroAngle.HasValue ||
                head.RotationOffsets?.Count != NozzleRunoutMath.SampleCount)
                throw new CalibrationStopped(
                    "Complete T004 optical runout calibration for this installed head/nozzle " +
                    "in the current machine session before camera-to-tool pickup.");
            if (!Finite(head.Offset.X) || !Finite(head.Offset.Y))
                throw new CalibrationStopped("Previous tool-head offset is invalid.");
            // These Z coordinates are DIFFERENT axes: Pick/Place are nozzle
            // ZL/ZR, whereas SendSafeMoveHeight moves the common machine Z.
            // Never issue SetToolHeadHeight(safeMoveHeight) here.
            _pickupZ = head.PickHeight;
            _returnZ = head.PlaceHeight;
            if (!Finite(_pickupZ) || !Finite(_returnZ) ||
                Math.Abs(_pickupZ) < 0.001 || Math.Abs(_returnZ) < 0.001)
                throw new CalibrationStopped(
                    "The selected head must have valid configured Pick Height and Place Height before any pickup.");
            if (!Finite(_machine.Settings.SafMoveHeight))
                throw new CalibrationStopped(
                    "Machine common-axis Safe Move Height is invalid.");
            // The calibration cylinder returns to the same fixed holder.
            // Large pickup/return Z discrepancies are unsafe until explicitly
            // checked against the fixture. The existing two heads use equal values.
            if (Math.Abs(_pickupZ - _returnZ) > 0.5)
                throw new CalibrationStopped(
                    "The head's Pick Height and Place Height differ by more than 0.5 mm. Verify fixture geometry before enabling an automatic return.");
            if (!VacuumCalibrationProfile.TryFromHead(head, out _))
                throw new CalibrationStopped(
                    "Calibrate and save the selected head's T007 vacuum profile before any pickup motion.");
            if ((head.HeadIndex == 1 && _machine.LeftVacuumPump) ||
                (head.HeadIndex == 2 && _machine.RightVacuumPump))
                throw new CalibrationStopped(
                    "Head vacuum is already active. Recover any previously held cylinder before starting.");
            Pass("readiness",
                $"Head {head.HeadIndex}: Pick ZL/ZR={_pickupZ:F2}, Return ZL/ZR={_returnZ:F2}, " +
                $"common Safe Z={_machine.Settings.SafMoveHeight:F2} mm. T004 runout present; no manual Z inputs.",
                MachineOperationVerificationKind.MachineObserved);

            Begin("known-location", "Using the existing Known Calibration Point.");
            var known = _machine.Settings.KnownCalibrationPoint;
            if (known == null || (known.X == 0 && known.Y == 0))
                throw new CalibrationStopped("Known Location is not configured.");
            Pass("known-location", $"Known Location: X={known.X:F3}, Y={known.Y:F3}; original value preserved.",
                MachineOperationVerificationKind.MachineObserved);

            Begin("first-target", "Locating the cylinder using existing Known Location circle vision.");
            var firstTarget = await LocateCylinderAsync();
            Pass("first-target", $"First cylinder center X={firstTarget.X:F3}, Y={firstTarget.Y:F3}.",
                MachineOperationVerificationKind.VisionVerified);

            Begin("nozzle-reference", "Measuring the empty nozzle center with the bottom camera.");
            var nozzleReference = await MeasureEmptyNozzleAsync(head);
            Pass("nozzle-reference", $"Nozzle reference X={nozzleReference.OffsetMm.X:F3}, " +
                $"Y={nozzleReference.OffsetMm.Y:F3} mm (bottom camera).",
                MachineOperationVerificationKind.VisionVerified);

            Begin("first-approach", "Returning over measured top-camera cylinder center.");
            await MoveHeadToCylinderAsync(firstTarget, null);
            Pass("first-approach", "Head aligned at cylinder target. Previous XY offset remains active.",
                MachineOperationVerificationKind.MachineObserved);

            Begin("first-pick", $"Lowering selected head to configured Pick Height {_pickupZ:F2} mm and checking vacuum.");
            await PickCylinderAsync(firstTarget);
            var firstMotion = DateTimeOffset.UtcNow;
            Pass("first-pick", "Vacuum sensor confirms the cylinder was picked. Retracted to safe Z.",
                MachineOperationVerificationKind.MachineObserved);

            Begin("first-inspect", "Inspecting the picked white cylinder with the bottom camera.");
            var firstCircle = await ObserveHeldCylinderAsync();
            var first = CreateObservation(head, nozzleReference, firstCircle, firstMotion);
            Pass("first-inspect", $"Cylinder minus nozzle: X={first.PartRelativeToNozzleMm.X:+0.000;-0.000;0.000}, " +
                $"Y={first.PartRelativeToNozzleMm.Y:+0.000;-0.000;0.000} mm.",
                MachineOperationVerificationKind.VisionVerified);

            Begin("candidate", "Computing the unsaved static XY correction.");
            if (!CameraToolPickupCalibrationMath.TryPropose(
                head.Offset, first, DateTimeOffset.UtcNow, out var candidate, out var reason))
                throw new CalibrationStopped(reason);
            CandidateSummary = $"Head {head.HeadIndex}: previous ({candidate.PreviousOffsetMm.X:F3}, " +
                $"{candidate.PreviousOffsetMm.Y:F3}) -> candidate ({candidate.ProposedOffsetMm.X:F3}, " +
                $"{candidate.ProposedOffsetMm.Y:F3}) mm. NOT SAVED.";
            Pass("candidate", CandidateSummary, MachineOperationVerificationKind.VisionVerified);

            Begin("replace-part", $"Returning cylinder at configured Place Height {_returnZ:F2} mm without modifying XY offsets.");
            await ReturnCylinderAsync(firstTarget);
            Pass("replace-part", "Returned cylinder and released vacuum at the configured return Z.",
                MachineOperationVerificationKind.MachineObserved);

            Begin("second-target", "Locating the returned cylinder again with Known Location vision.");
            var secondTarget = await LocateCylinderAsync();
            Pass("second-target", $"Second cylinder center X={secondTarget.X:F3}, Y={secondTarget.Y:F3}.",
                MachineOperationVerificationKind.VisionVerified);

            Begin("second-approach", "Applying the proposed offset ONLY as a bounded relative jog.");
            await MoveHeadToCylinderAsync(secondTarget, candidate);
            Pass("second-approach", $"Candidate test jog X={candidate.SecondPickupJogMm.X:+0.000;-0.000;0.000}, " +
                $"Y={candidate.SecondPickupJogMm.Y:+0.000;-0.000;0.000} mm. Stored offset unchanged.",
                MachineOperationVerificationKind.MachineObserved);

            Begin("second-pick", "Performing an independent cylinder pickup with the candidate correction.");
            await PickCylinderAsync(secondTarget, candidate);
            var secondMotion = DateTimeOffset.UtcNow;
            Pass("second-pick", "Fresh vacuum sensor pickup confirmation after candidate jog.",
                MachineOperationVerificationKind.MachineObserved);

            Begin("second-inspect", "Inspecting the second picked cylinder with the bottom camera.");
            var secondCircle = await ObserveHeldCylinderAsync();
            var second = CreateObservation(head, nozzleReference, secondCircle, secondMotion);
            Pass("second-inspect", $"Second pickup residual X={second.PartRelativeToNozzleMm.X:+0.000;-0.000;0.000}, " +
                $"Y={second.PartRelativeToNozzleMm.Y:+0.000;-0.000;0.000} mm.",
                MachineOperationVerificationKind.VisionVerified);

            Begin("verify", "Accepting only an independently verified second pickup.");
            if (!CameraToolPickupCalibrationMath.TryVerify(candidate, second, DateTimeOffset.UtcNow,
                out var residual, out reason))
                throw new CalibrationStopped(reason);
            CandidateSummary = $"Head {head.HeadIndex} second pickup VERIFIED: residual {residual:F3} mm; " +
                $"candidate ({candidate.ProposedOffsetMm.X:F3}, {candidate.ProposedOffsetMm.Y:F3}) mm.";
            Pass("verify", CandidateSummary, MachineOperationVerificationKind.VisionVerified);

            // Return the verified cylinder before switching camera/navigation.
            await ReturnCylinderAsync(secondTarget, candidate);
            Begin("safe-camera-mode", "Restoring safe Z and camera navigation with OLD offset.");
            _machine.SendSafeMoveHeight();
            await NeedIdleAsync("final safe height");
            await _machine.MoveToCameraAsync();
            await NeedIdleAsync("camera navigation");
            if (_machine.CurrentMachineToolHead != null)
                throw new CalibrationStopped("Camera navigation was not restored. Candidate was not saved.");
            Pass("safe-camera-mode", "Machine at safe Z in camera mode, with previous tool-head offset.",
                MachineOperationVerificationKind.MachineObserved);

            Begin("commit-offset", "Committing candidate only if explicitly enabled.");
            if (!SaveVerifiedOffset)
            {
                var step = Step("commit-offset");
                step.Detail = "Measured and verified only. Auto-save is OFF; the previous offset was preserved.";
                step.Status = "Unsaved";
                return;
            }
            RequireValid();
            var old = new Point2D<double>(head.Offset.X, head.Offset.Y);
            head.Offset = new Point2D<double>(candidate.ProposedOffsetMm.X, candidate.ProposedOffsetMm.Y);
            var saved = await _repo.SaveCurrentMachineAsync();
            if (!saved.Successful)
            {
                head.Offset = old;
                throw new CalibrationStopped("The verified offset was NOT saved: " + saved.ErrorMessage);
            }
            Pass("commit-offset", $"Saved verified XY ({head.Offset.X:F3}, {head.Offset.Y:F3}) mm.",
                MachineOperationVerificationKind.MachineObserved, saved:true);
        }

        private async Task<Point2D<double>> LocateCylinderAsync()
        {
            RequireValid();
            var result = await _knownLocation.RunAsync();
            if (!result.Successful || result.Result == null)
                throw new CalibrationStopped(_knownLocation.Status);
            return result.Result;
        }

        private async Task<BottomCircle> MeasureEmptyNozzleAsync(MachineToolHead head)
        {
            RequireValid();
            _emptyNozzleOpticalLock = false;
            _emptyNozzleCameraId = null;
            _emptyNozzleCameraScale = 0;

            // T004: prepare the live Nozzle profile, turn on its existing
            // illumination, and enable circle detection before approaching
            // the bottom camera. Do not depend on saved or previous-session
            // adaptive detector state.
            var capture = _machine.PartInspectionCaptureService;
            if (capture == null)
            {
                _bottomCameraBootstrap.CameraType = CameraTypes.PartInspection;
                capture = _machine.PartInspectionCaptureService;
            }
            if (capture?.Camera == null || capture.CameraType != CameraTypes.PartInspection)
                throw new CalibrationStopped("T004 nozzle reference: bottom camera is unavailable.");

            _machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_Nozzle);
            if (!NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                capture.Profile, out var preparationReason))
                throw new CalibrationStopped("T004 nozzle reference: " + preparationReason);
            // Same re-selection as T004: applies light enable to hardware.
            _machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_Nozzle);
            var captureReady = await capture.EnsureCaptureReadyAsync();
            if (!captureReady.Successful)
                throw new CalibrationStopped("T004 nozzle reference: " + captureReady.ErrorMessage);

            _emptyNozzleCameraId = Convert.ToString(capture.Camera.Id);
            _emptyNozzleCameraScale = capture.Camera.EffectivePixelsPerMM;
            if (!Finite(_emptyNozzleCameraScale) || _emptyNozzleCameraScale <= 0)
                throw new CalibrationStopped("T004 nozzle reference: invalid bottom-camera pixel scale.");

            await SwitchToHeadAsync(head);
            _machine.ClearToolHeadAngle();
            await NeedIdleAsync("nozzle zero-angle positioning");
            await MoveToInspectionCameraAsync(holdingCylinder:false);
            await NeedIdleAsync("nozzle image auto-tune readiness");

            // Reuse T004's proven 24-fresh-frame dark-opening tuning. Each
            // head is tuned separately, with no camera-exposure writes and
            // no persistence of transient vision-profile settings.
            var tuned = await capture.TuneNozzleOpeningAsync();
            if (!tuned.Successful || tuned.Result == null)
                throw new CalibrationStopped("T004 adaptive nozzle detector: " +
                    (tuned.ErrorMessage ?? "No repeatable dark nozzle opening."));
            RequireValid();
            await NeedIdleAsync("post-tune nozzle image stability");
            if (!ReferenceEquals(_machine.PartInspectionCaptureService, capture) ||
                !String.Equals(Convert.ToString(capture.Camera?.Id),
                    _emptyNozzleCameraId, StringComparison.Ordinal) ||
                Math.Abs(capture.Camera.EffectivePixelsPerMM - _emptyNozzleCameraScale) > 0.01 ||
                capture.Profile?.Key != VisionProfile.VisionProfile_Nozzle ||
                !ReferenceEquals(_machine.CurrentMachineToolHead, head))
                throw new CalibrationStopped(
                    "T004 nozzle reference camera, scale, profile, or selected head changed during tuning.");

            if (!NozzleRunoutVisionPreparation.TryLockAdaptiveOpening(
                capture.Profile, _emptyNozzleCameraScale, tuned.Result, out var lockReason))
                throw new CalibrationStopped("T004 adaptive nozzle lock rejected tuning: " + lockReason);
            _emptyNozzleOpticalLock = true;
            capture.ResetDetections();

            Status = $"Head {head.HeadIndex}: T004 dark nozzle opening locked, " +
                $"diameter {tuned.Result.DiameterMm:F3} mm, " +
                $"{tuned.Result.GoodFrames}/{tuned.Result.TotalFrames} accepted frames, " +
                $"scale {_emptyNozzleCameraScale:F2} px/mm. Acquiring fresh center.";

            return await ObserveBottomAsync(VisionProfile.VisionProfile_Nozzle, isCylinder:false);
        }

        private async Task<BottomCircle> ObserveHeldCylinderAsync()
        {
            // The locked nozzle detector is for the small DARK opening only.
            // Reuse the existing stabilized circle detector for the white disc.
            _emptyNozzleOpticalLock = false;
            _machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);
            var capture = _machine.PartInspectionCaptureService;
            if (capture?.Profile?.Key != VisionProfile.VisionProfile_PartInspection)
                throw new CalibrationStopped("Cannot activate the bottom camera Part Inspection profile.");
            capture.Profile.UseAdaptiveDarkOpening = false;
            capture.Profile.PerformShapeDetection = true;
            capture.Profile.FindCircles = true;
            // Reapply the selected profile so runtime camera settings are active.
            _machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);
            var captureReady = await capture.EnsureCaptureReadyAsync();
            if (!captureReady.Successful)
                throw new CalibrationStopped("Held-disc bottom camera is not capturing fresh frames: " +
                    captureReady.ErrorMessage);
            try
            {
                await MoveToInspectionCameraAsync(holdingCylinder:true);
                return await ObserveBottomAsync(VisionProfile.VisionProfile_PartInspection, isCylinder:true);
            }
            finally
            {
                // The inspection moves common Z below Safe Move Height.
                // Restore clearance even if optical detection fails, but never
                // issue new motion if the machine/head safety context was lost.
                if (ValidMachine(out _) &&
                    ReferenceEquals(_machine.CurrentMachineToolHead, _head))
                {
                    _machine.SendSafeMoveHeight();
                    await NeedIdleAsync("safe Z after held-disc inspection");
                }
            }
        }

        private async Task SwitchToHeadAsync(MachineToolHead head)
        {
            RequireValid();
            _machine.SendSafeMoveHeight();
            await NeedIdleAsync("safe head transition");
            await _machine.MoveToToolHeadAsync(head);
            await NeedIdleAsync("selected tool head");
            if (!ReferenceEquals(_machine.CurrentMachineToolHead, head))
                throw new CalibrationStopped("The requested tool head was not selected.");
        }

        private async Task MoveHeadToCylinderAsync(Point2D<double> target,
            CameraToolPickupCandidate candidate)
        {
            await SwitchToHeadAsync(_head);
            var area = _machine.Settings.WorkAreaSize;
            var physicalX = target.X + _head.Offset.X;
            var physicalY = target.Y + _head.Offset.Y;
            if (physicalX < 5 || physicalY < 5 ||
                physicalX > area.X - 5 || physicalY > area.Y - 5)
                throw new CalibrationStopped("Target pickup position would violate physical gantry travel limits.");
            _machine.GotoPoint(target);
            await NeedIdleAsync("known cylinder pickup XY");
            if (candidate != null)
            {
                var current = await _machine.GetCurrentLocationAsync(5000);
                if (!current.Successful)
                    throw new CalibrationStopped("Cannot read physical XY for second-pass jog.");
                if (!CameraToolPickupCalibrationMath.TryPlanSecondPickup(
                        candidate, current.Result, area, out var jog, out var reason))
                    throw new CalibrationStopped(reason);
                _machine.GotoPoint(jog, relativeMove:true);
                await NeedIdleAsync("candidate second-pickup jog");
            }
        }

        private async Task PickCylinderAsync(Point2D<double> holder,
            CameraToolPickupCandidate candidate = null)
        {
            RequireValid();
            if (_machine.CurrentMachineToolHead != _head)
                throw new CalibrationStopped("Pickup head selection changed.");
            _machine.SetToolHeadHeight(_pickupZ);
            await NeedIdleAsync("head's configured pickup ZL/ZR");
            SetVacuum(true);
            await NeedIdleAsync("vacuum activation");
            await Task.Delay(250);
            _machine.SendSafeMoveHeight();
            await NeedIdleAsync("retracting after pickup");

            // A classified NoPart/Unknown reading does not prove the cylinder
            // is absent. Assume it is held and use the SAME guarded holder-return
            // path as the successful pickup. Stop calibration without saving.
            var detectionFailure = await VacuumConfirmsPartAsync();
            if (detectionFailure == null)
                return;

            try
            {
                RequireValid();
                if (!ReferenceEquals(_machine.CurrentMachineToolHead, _head))
                    throw new CalibrationStopped("Pickup head selection changed before recovery.");
                await ReturnCylinderAsync(holder, candidate);
            }
            catch (Exception recoveryError)
            {
                throw new CalibrationStopped(
                    detectionFailure + " Automatic holder return did not complete: " +
                    recoveryError.Message +
                    " Stop and verify cylinder, vacuum and Z before any further motion.");
            }
            throw new CalibrationStopped(
                detectionFailure +
                " Assuming the cylinder was picked: returned it to its holder, " +
                "turned off vacuum and retracted to Safe Move Height. " +
                "Calibration stopped without changing the stored offset.");
        }

        private async Task ReturnCylinderAsync(Point2D<double> target,
            CameraToolPickupCandidate candidate = null)
        {
            RequireValid();
            _machine.SendSafeMoveHeight();
            await NeedIdleAsync("pre-return safe height");
            _machine.GotoPoint(target);
            await NeedIdleAsync("cylinder return XY");
            if (candidate != null)
            {
                var current = await _machine.GetCurrentLocationAsync(5000);
                if (!current.Successful)
                    throw new CalibrationStopped("Unable to verify XY before corrected-cylinder return.");
                if (!CameraToolPickupCalibrationMath.TryPlanSecondPickup(
                        candidate, current.Result, _machine.Settings.WorkAreaSize,
                        out var returnJog, out var reason))
                    throw new CalibrationStopped("Unsafe corrected-cylinder return: " + reason);
                _machine.GotoPoint(returnJog, relativeMove:true);
                await NeedIdleAsync("corrected-cylinder return XY");
            }
            _machine.SetToolHeadHeight(_returnZ);
            await NeedIdleAsync("head's configured return ZL/ZR");
            SetVacuum(false);
            await NeedIdleAsync("vacuum release");
            await Task.Delay(300);
            _machine.SendSafeMoveHeight();
            await NeedIdleAsync("safe retract after return");
        }

        private async Task MoveToInspectionCameraAsync(bool holdingCylinder)
        {
            RequireValid();
            var capture = _machine.PartInspectionCaptureService;
            if (capture == null)
            {
                _bottomCameraBootstrap.CameraType = CameraTypes.PartInspection;
                capture = _machine.PartInspectionCaptureService;
            }
            if (capture?.Camera == null || capture.CameraType != CameraTypes.PartInspection)
                throw new CalibrationStopped("Bottom camera capture could not initialize.");
            if (!ReferenceEquals(_machine.CurrentMachineToolHead, _head))
                throw new CalibrationStopped("Tool-head selection changed before bottom-camera inspection. No Z descent.");
            if (capture.Camera.AbsolutePosition == null)
                throw new CalibrationStopped("Bottom camera has no configured XY center.");
            var xy = capture.Camera.AbsolutePosition;
            var physicalX = xy.X + _head.Offset.X;
            var physicalY = xy.Y + _head.Offset.Y;
            var area = _machine.Settings.WorkAreaSize;
            if (physicalX < 5 || physicalY < 5 ||
                physicalX > area.X - 5 || physicalY > area.Y - 5)
                throw new CalibrationStopped("Bottom camera approach exceeds physical XY travel limits.");
            _machine.SendSafeMoveHeight();
            await NeedIdleAsync("safe inspection approach");
            if (holdingCylinder)
            {
                // The operator's proven focus position is the selected head's
                // Pick Height on the common Marlin Z axis (not ZL/ZR).
                // Left (Pick) = ToolCommonZ; Right (Pick) is mirrored about
                // Safe Move Height in the existing machine-position model.
                // First finish XY travel and verify common Safe Z before
                // commanding the focus move. Do not use the camera's
                // DetectionHeight/FocusHeight or reissue its ZL/ZR command.
                var inspectionCommonZ = _head.HeadIndex == 1
                    ? _pickupZ
                    : 2 * _machine.Settings.SafMoveHeight - _pickupZ;
                if (!Finite(inspectionCommonZ) || inspectionCommonZ < 0)
                    throw new CalibrationStopped(
                        "Configured head Pick Height maps to an invalid common-Z inspection position.");
                _machine.GotoPoint(xy);
                await NeedIdleAsync("bottom-camera XY at common safe height with cylinder held");
                var location = await _machine.GetCurrentLocationAsync(5000);
                if (!location.Successful ||
                    Math.Abs(_machine.ToolCommonZ - _machine.Settings.SafMoveHeight) > 0.2)
                    throw new CalibrationStopped(
                        "Common Z did not verify at Safe Move Height before held-cylinder inspection.");
                if (!ReferenceEquals(_machine.CurrentMachineToolHead, _head))
                    throw new CalibrationStopped("Tool head changed before bottom-camera focus motion.");

                // Same G0 Z/PickHeight axis and feed rate as the existing
                // operator 'Move to pick height' control.
                _machine.SendCommand(String.Format(CultureInfo.InvariantCulture,
                    "G0 Z{0:0.###} F5000", inspectionCommonZ));
                await NeedIdleAsync("held-disc inspection at selected head Pick Height");
                var focused = await _machine.GetCurrentLocationAsync(5000);
                var reportedPickZ = _head.HeadIndex == 1
                    ? _machine.LeftToolHeadZ : _machine.RightToolHeadZ;
                if (!focused.Successful ||
                    Math.Abs(_machine.ToolCommonZ - inspectionCommonZ) > 0.2 ||
                    Math.Abs(reportedPickZ - _pickupZ) > 0.2 ||
                    !ReferenceEquals(_machine.CurrentMachineToolHead, _head))
                    throw new CalibrationStopped(
                        "Held-disc inspection did not verify the selected head's Pick Height; stopping optical measurement.");
            }
            else
            {
                // The proven empty-nozzle optical path can use its own profile
                // height. The profile must be active BEFORE this motion.
                await _machine.GoToPartInspectionCameraAsync();
                await NeedIdleAsync("empty-nozzle bottom-camera position");
            }
        }

        private async Task<BottomCircle> ObserveBottomAsync(string profileKey, bool isCylinder)
        {
            var capture = _machine.PartInspectionCaptureService;
            if (capture == null || capture.Camera == null)
                throw new CalibrationStopped("Bottom camera is unavailable.");
            if (capture.Profile?.Key != profileKey || !capture.Profile.FindCircles ||
                capture.Camera.EffectivePixelsPerMM <= 0)
                throw new CalibrationStopped("Configure a circular detector for bottom profile '" +
                    profileKey + "'. No unverified detection will be used.");
            if (profileKey == VisionProfile.VisionProfile_Nozzle &&
                (!_emptyNozzleOpticalLock ||
                 !String.Equals(Convert.ToString(capture.Camera.Id),
                     _emptyNozzleCameraId, StringComparison.Ordinal) ||
                 Math.Abs(capture.Camera.EffectivePixelsPerMM - _emptyNozzleCameraScale) > 0.01))
                throw new CalibrationStopped(
                    "T004 nozzle optical lock or camera-scale identity is missing; refusing a generic-circle fallback.");
            if (capture.CaptureFrameSize.Width <= 0)
                capture.StartCapture();

            Interlocked.Exchange(ref _bottomCircleCandidateFrames, 0);
            Interlocked.Exchange(ref _bottomCircleStableFrames, 0);
            Interlocked.Exchange(ref _bottomCircleWrongSizeFrames, 0);
            Interlocked.Exchange(ref _bottomCircleContextRejectedFrames, 0);
            var request = new TaskCompletionSource<BottomCircle>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_observationGate)
            {
                _expectedBottomProfile = profileKey;
                _pendingCircle = request;
            }
            try
            {
                capture.ResetDetections();
                _locator.RegisterCircleLocatedHandler(this, 9000);
                var target = isCylinder ? "white calibration disc (5.5-10.5 mm)" : "empty nozzle opening";
                Status = $"Head {_head.HeadIndex}: Bottom camera actively searching for {target} " +
                    $"at {capture.Camera.EffectivePixelsPerMM:F1} px/mm; awaiting stable circle (9 s).";
                var deadline = DateTimeOffset.UtcNow.AddMilliseconds(9000);
                while (!request.Task.IsCompleted && DateTimeOffset.UtcNow < deadline)
                {
                    var remainingMs = Math.Max(1, (int)Math.Min(1000,
                        (deadline - DateTimeOffset.UtcNow).TotalMilliseconds));
                    await Task.WhenAny(request.Task, Task.Delay(remainingMs));
                    if (!request.Task.IsCompleted)
                        Status = $"Head {_head.HeadIndex}: Searching for {target}; " +
                            $"candidate frames {Volatile.Read(ref _bottomCircleCandidateFrames)}, " +
                            $"stable {Volatile.Read(ref _bottomCircleStableFrames)}, " +
                            $"wrong size {Volatile.Read(ref _bottomCircleWrongSizeFrames)}, " +
                            $"context rejected {Volatile.Read(ref _bottomCircleContextRejectedFrames)}.";
                }
                var circle = request.Task.IsCompleted ? await request.Task : null;
                if (circle == null)
                {
                    var profile = capture.Profile;
                    throw new CalibrationStopped(
                        $"No fresh stable bottom-camera circle for {profileKey}: " +
                        $"candidate frames {Volatile.Read(ref _bottomCircleCandidateFrames)}, " +
                        $"stable {Volatile.Read(ref _bottomCircleStableFrames)}, " +
                        $"wrong size {Volatile.Read(ref _bottomCircleWrongSizeFrames)}, " +
                        $"context rejected {Volatile.Read(ref _bottomCircleContextRejectedFrames)}. " +
                        $"Camera capturing={capture.IsCapturing}, profile={profile?.Key ?? "none"}, " +
                        $"FindCircles={profile?.FindCircles}, " +
                        $"search radius={profile?.TargetImageRadius:F1} mm, " +
                        $"Hough radius={profile?.HoughCirclesMinRadius:F1}-{profile?.HoughCirclesMaxRadius:F1} mm, " +
                        $"Hough threshold={profile?.HoughCirclesParam2:F1}.");
                }
                if (isCylinder && (circle.DiameterMm < 5.5 || circle.DiameterMm > 10.5))
                    throw new CalibrationStopped("Held-cylinder diameter is inconsistent with the 7 mm fixture.");
                return circle;
            }
            finally
            {
                _locator.UnregisterCircleLocatedHandler(this);
                lock (_observationGate)
                {
                    if (ReferenceEquals(_pendingCircle, request))
                        _pendingCircle = null;
                }
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            TaskCompletionSource<BottomCircle> request;
            string expected;
            lock (_observationGate)
            {
                request = _pendingCircle;
                expected = _expectedBottomProfile;
            }
            var machine = _machine;
            var capture = machine?.PartInspectionCaptureService;
            if (request == null || circle?.CameraType != CameraTypes.PartInspection)
                return;
            Interlocked.Increment(ref _bottomCircleCandidateFrames);
            if (!circle.Stabilized || !circle.PositionStable)
                return;
            Interlocked.Increment(ref _bottomCircleStableFrames);
            if (machine.Busy || capture?.Profile?.Key != expected ||
                !ValidMachine(out _) || machine.CurrentMachineToolHead != _head ||
                !String.Equals(Convert.ToString(_head.CurrentNozzle?.Id),
                    _nozzleIdentity, StringComparison.Ordinal))
            {
                Interlocked.Increment(ref _bottomCircleContextRejectedFrames);
                return;
            }
            var scale = capture.Camera?.EffectivePixelsPerMM ?? 0;
            // Ignore an outer fixture/overlay circle and continue seeking
            // the actual white 7 mm disc instead of failing on the first circle.
            if (expected == VisionProfile.VisionProfile_PartInspection &&
                (scale <= 0 || 2 * circle.RadiusPixels / scale < 5.5 ||
                 2 * circle.RadiusPixels / scale > 10.5))
            {
                Interlocked.Increment(ref _bottomCircleWrongSizeFrames);
                return;
            }
            if (expected == VisionProfile.VisionProfile_Nozzle &&
                (!_emptyNozzleOpticalLock ||
                 !String.Equals(Convert.ToString(capture.Camera?.Id),
                     _emptyNozzleCameraId, StringComparison.Ordinal) ||
                 Math.Abs(scale - _emptyNozzleCameraScale) > 0.01 ||
                 !NozzleRunoutVisionPreparation.MatchesLockedOpening(
                     capture.Profile, _emptyNozzleCameraScale, circle)))
                return;
            var offset = circle.OffsetPixels;
            if (scale <= 0 || offset == null || !Finite(offset.X) || !Finite(offset.Y))
                return;
            request.TrySetResult(new BottomCircle
            {
                OffsetMm = new Point2D<double>(offset.X / scale, -offset.Y / scale),
                DiameterMm = 2 * circle.RadiusPixels / scale,
                CapturedAtUtc = DateTimeOffset.UtcNow
            });
        }

        public void CircleLocatorTimeout()
        {
            lock (_observationGate) _pendingCircle?.TrySetResult(null);
        }
        public void CircleLocatorAborted() => CircleLocatorTimeout();

        private CameraToolPickupObservation CreateObservation(MachineToolHead head,
            BottomCircle nozzle, BottomCircle part, DateTimeOffset motion)
        {
            if (nozzle == null || part == null || part.CapturedAtUtc < motion)
                throw new CalibrationStopped("The nozzle/part image pair is not a fresh valid observation.");
            return new CameraToolPickupObservation
            {
                HeadIndex = head.HeadIndex,
                NozzleId = _nozzleIdentity,
                FixtureId = "known-location-7mm-cylinder",
                PartRelativeToNozzleMm = new Point2D<double>(
                    part.OffsetMm.X - nozzle.OffsetMm.X,
                    part.OffsetMm.Y - nozzle.OffsetMm.Y),
                VacuumConfirmed = true, // independently checked at pickup
                StableVision = true, // both bottom observations were stabilized
                // T004's session-only runout table belongs to this head and
                // is invalidated automatically if the installed nozzle changes.
                NozzleCalibrated = head.IsCalibrated &&
                    head.RunoutZeroAngle.HasValue &&
                    head.RotationOffsets?.Count == NozzleRunoutMath.SampleCount,
                MotionFinishedUtc = motion,
                CapturedUtc = part.CapturedAtUtc
            };
        }

        private async Task<string> VacuumConfirmsPartAsync()
        {
            // T007's polarity-independent calibration is the single authority
            // for part presence, including its configured hysteresis.
            if (!VacuumCalibrationProfile.TryFromHead(_head, out var profile))
                throw new CalibrationStopped(
                    "The selected head has no valid saved T007 vacuum calibration.");

            var values = new double[3];
            for (var i = 0; i < values.Length; i++)
            {
                if (!ReferenceEquals(_machine.CurrentMachineToolHead, _head))
                    throw new CalibrationStopped("Selected head changed during vacuum pickup verification.");
                var sample = _head.HeadIndex == 1
                    ? await _machine.ReadLeftVacuumAsync()
                    : await _machine.ReadRightVacuumAsync();
                if (!sample.Successful)
                    throw new CalibrationStopped("Vacuum sensor read failed: " + sample.ErrorMessage);
                values[i] = sample.Result;
                if (i < values.Length - 1)
                    await Task.Delay(80);
            }

            var mean = values.Average();
            var classification = profile.Classify(mean);
            if (classification == VacuumPickupClassification.PartPresent)
                return null;

            return $"Vacuum did not confirm the picked cylinder: Head {_head.HeadIndex}, " +
                $"{classification}, normalized {profile.Normalize(mean):F1}% " +
                $"(part threshold {profile.ThresholdPercent + profile.HysteresisPercent:F1}%, " +
                $"raw mean {mean:F0}, open {profile.OpenRaw:F0}, sealed {profile.SealedRaw:F0}).";
        }

        private void SetVacuum(bool on)
        {
            if (_head.HeadIndex == 1) _machine.LeftVacuumPump = on;
            else if (_head.HeadIndex == 2) _machine.RightVacuumPump = on;
            else throw new CalibrationStopped("Only two supported nozzle heads may operate vacuum.");
        }

        private async Task NeedIdleAsync(string description)
        {
            if (!ValidMachine(out var reason))
                throw new CalibrationStopped(reason + " during " + description);
            var idle = await _machine.SpinUntilIdleAsync(30000);
            if (!idle.Successful)
                throw new CalibrationStopped(description + " failed: " + idle.ErrorMessage);
            for (var i = 0; i < 30; i++)
            {
                if (!ValidMachine(out reason))
                    throw new CalibrationStopped(reason + " during " + description);
                if (!_machine.Busy && _machine.ToSendQueueCount == 0 &&
                    _machine.UnacknowledgedBytesSent == 0)
                    return;
                await Task.Delay(100);
            }
            throw new CalibrationStopped("Serial command queue did not settle after " + description);
        }

        private void RequireValid()
        {
            if (!ValidMachine(out var reason))
                throw new CalibrationStopped(reason);
            if (_head != null && !String.Equals(Convert.ToString(_head.CurrentNozzle?.Id),
                    _nozzleIdentity, StringComparison.Ordinal))
                throw new CalibrationStopped("Installed nozzle changed during the procedure.");
        }

        private bool ValidMachine(out string error)
        {
            var m = _machine;
            if (m == null || !ReferenceEquals(_repo.CurrentMachine, m) ||
                !m.Connected || m.Settings == null || m.Mode != OperatingMode.Manual ||
                !m.WasMachineHomed || !m.WasMachinOriginCalibrated)
            {
                error = "Machine must remain connected, homed, origin-calibrated and in Manual mode.";
                return false;
            }
            error = null;
            return true;
        }

        private FeederCalibrationFlowStep Step(string key) =>
            CalibrationSteps.FirstOrDefault(s => s.StepKey == $"head-{_head?.HeadIndex}/{key}");

        private void Begin(string key, string description)
        {
            RequireValid();
            var step = Step(key);
            if (step == null)
                throw new CalibrationStopped("Missing calibration procedure step: " + key);
            step.Begin(description);
            Status = $"Head {_head.HeadIndex}: {description}";
        }

        private void Pass(string key, string detail, MachineOperationVerificationKind verification,
            bool saved = false)
        {
            var step = Step(key);
            if (step == null || _session == null)
                throw new CalibrationStopped("Lost procedure state at " + key);
            var now = DateTimeOffset.UtcNow;
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = _session.Snapshot.RunId,
                StepKey = key,
                Name = detail,
                Verification = verification,
                Saved = saved,
                CapturedAtUtc = now
            });
            _session.CompleteActiveStep(key, now);
            step.Pass(detail);
        }

        private static bool Finite(double x) => !Double.IsNaN(x) && !Double.IsInfinity(x);
    }
}
