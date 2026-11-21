using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    /// <summary>
    /// T012 supervised runout camera/vision presentation. No camera preview,
    /// profile editor or video callback commands machine movement. The
    /// original StartAsync remains the only operator-started motion sequence.
    /// </summary>
    public partial class NozzleTipCalibrationViewModel
    {
        private SynchronizationContext _runoutUiContext;
        private VisionProfile _runoutEditingProfile;
        private MachineCamera _runoutEditingCamera;
        private NozzleRunoutVisionTuning _runoutSavedTuning;
        private NozzleRunoutVisionTuning _runoutVisionTuning;
        private FiducialCircleObservation _runoutVideoObservation;
        private bool _runoutHasAppliedPreview, _runoutPreviewBusy;
        private string _runoutCameraStatus = "No motion on opening. Click Preview Bottom Camera to start live video.";
        private string _runoutVisionStatus = "Edit nozzle settings, Apply Preview, then Save Profile or Revert.";

        public IMachine RunoutMachine => _machineRepo.CurrentMachine;

        public FiducialCircleObservation RunoutVideoObservation
        {
            get => _runoutVideoObservation;
            private set => Set(ref _runoutVideoObservation, value);
        }

        public string RunoutCameraStatus
        {
            get => _runoutCameraStatus;
            private set => Set(ref _runoutCameraStatus, value);
        }

        public string RunoutVisionStatus
        {
            get => _runoutVisionStatus;
            private set => Set(ref _runoutVisionStatus, value);
        }

        public bool RunoutPreviewBusy
        {
            get => _runoutPreviewBusy;
            private set
            {
                Set(ref _runoutPreviewBusy, value);
                RaisePropertyChanged(nameof(CanStartRunout));
                RaisePropertyChanged(nameof(CanTuneRunoutVision));
            }
        }

        public bool HasRunoutVisionPreview
        {
            get => _runoutHasAppliedPreview;
            private set
            {
                Set(ref _runoutHasAppliedPreview, value);
                RaisePropertyChanged(nameof(CanStartRunout));
                RaisePropertyChanged(nameof(RunoutSearchSummary));
            }
        }

        public NozzleRunoutVisionTuning RunoutVisionTuning
        {
            get => _runoutVisionTuning;
            private set
            {
                if (_runoutVisionTuning != null)
                    _runoutVisionTuning.PropertyChanged -= RunoutTuningChanged;
                Set(ref _runoutVisionTuning, value);
                if (_runoutVisionTuning != null)
                    _runoutVisionTuning.PropertyChanged += RunoutTuningChanged;
                RaisePropertyChanged(nameof(HasRunoutVisionDraft));
                RaisePropertyChanged(nameof(CanStartRunout));
            }
        }

        public bool HasRunoutVisionDraft =>
            RunoutVisionTuning != null && _runoutEditingProfile != null &&
            !RunoutVisionTuning.Matches(_runoutEditingProfile);

        public bool CanStartRunout =>
            !RunoutRunning && !RunoutPreviewBusy &&
            !HasRunoutVisionDraft && !HasRunoutVisionPreview &&
            SelectedRunoutHead != null;

        public bool CanTuneRunoutVision =>
            !RunoutRunning && !RunoutPreviewBusy;

        public string RunoutSearchSummary
        {
            get
            {
                var machine = RunoutMachine;
                var capture = machine?.PartInspectionCaptureService;
                var profile = _runoutEditingProfile;
                if (profile == null)
                    return "No installed bottom-camera Nozzle vision profile.";
                var scale = capture?.Camera?.EffectivePixelsPerMM ?? 0;
                var width = capture?.CaptureFrameSize.Width ?? 0;
                var height = capture?.CaptureFrameSize.Height ?? 0;
                var px = profile.UseAdaptiveDarkOpening
                    ? NozzleRunoutVisionTuning.EffectiveSearchRadiusPixels(
                        width, height, scale, profile.TargetImageRadius)
                    : scale * profile.TargetImageRadius;
                return "ACTIVE SEARCH: " + profile.TargetImageRadius.ToString("F1") +
                    " mm · effective " + px.ToString("F0") + " px" +
                    (profile.UseAdaptiveDarkOpening ? " (minimum 72px when space permits)" :
                                                      " (before opening lock)") +
                    (HasRunoutVisionPreview ? " · UNSAVED" : "");
            }
        }

        private void LoadRunoutVisionSettings()
        {
            var camera = RunoutMachine?.Settings?.Cameras?.FirstOrDefault(c =>
                c.CameraType?.Value == CameraTypes.PartInspection);
            var profile = camera?.VisionProfiles?.FirstOrDefault(p =>
                p.Key == VisionProfile.VisionProfile_Nozzle);
            _runoutEditingCamera = camera;
            _runoutEditingProfile = profile;
            _runoutSavedTuning = profile == null ? null : NozzleRunoutVisionTuning.FromProfile(profile);
            RunoutVisionTuning = profile == null ? null : NozzleRunoutVisionTuning.FromProfile(profile);
            HasRunoutVisionPreview = false;
            RaisePropertyChanged(nameof(RunoutSearchSummary));
        }

        private void RunoutTuningChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NozzleRunoutVisionTuning.SearchRadiusMm))
                ApplyRunoutSearchRadiusLive();
            RaisePropertyChanged(nameof(HasRunoutVisionDraft));
            RaisePropertyChanged(nameof(CanStartRunout));
        }

        private bool RunoutPreviewContext(out string reason)
        {
            var machine = RunoutMachine;
            var camera = machine?.PartInspectionCaptureService?.Camera;
            if (machine == null || !machine.Connected ||
                machine.Mode != OperatingMode.Manual ||
                machine.Busy || machine.IsLocating || _locator.IsLocating)
            {
                reason = "Live nozzle preview requires a connected, idle Manual-mode machine with no active locator.";
                return false;
            }
            if (_runoutEditingCamera == null || _runoutEditingProfile == null ||
                machine.Settings?.Cameras?.Contains(_runoutEditingCamera) != true ||
                !_runoutEditingCamera.VisionProfiles.Contains(_runoutEditingProfile) ||
                !ReferenceEquals(camera, _runoutEditingCamera) ||
                camera?.CameraType?.Value != CameraTypes.PartInspection)
            {
                reason = "Bottom camera or Nozzle profile changed. Reopen calibration.";
                return false;
            }
            reason = null;
            return true;
        }

        private void ApplyRunoutSearchRadiusLive()
        {
            if (!CanTuneRunoutVision || RunoutVisionTuning == null ||
                _runoutEditingProfile == null || _runoutSavedTuning == null)
                return;
            var radius = RunoutVisionTuning.SearchRadiusMm;
            if (double.IsNaN(radius) || double.IsInfinity(radius) ||
                radius < 0.1 || radius > 30)
            {
                RunoutVisionStatus = "Search radius draft is invalid (0.1-30 mm); active detector unchanged.";
                return;
            }
            if (!RunoutPreviewContext(out var reason))
            {
                RunoutVisionStatus = "Search edit is a draft only: " + reason;
                return;
            }
            var capture = RunoutMachine.PartInspectionCaptureService;
            if (!capture.IsCapturing ||
                !ReferenceEquals(capture.Profile, _runoutEditingProfile))
            {
                RunoutVisionStatus = "Start Preview Bottom Camera before live search adjustment.";
                return;
            }

            _runoutEditingProfile.TargetImageRadius = radius;
            capture.ResetDetections();
            RunoutVideoObservation = null;
            HasRunoutVisionPreview = !_runoutSavedTuning.Matches(_runoutEditingProfile);
            RaisePropertyChanged(nameof(RunoutSearchSummary));
            RunoutVisionStatus = "LIVE SEARCH UPDATED (UNSAVED). Detector search and video ROI match the new radius. Save Profile or Revert.";
        }

        private async Task<bool> PrepareRunoutCameraAsync()
        {
            if (!RunoutPreviewContextForBootstrap(out var reason))
            {
                RunoutCameraStatus = reason;
                return false;
            }

            var machine = RunoutMachine;
            if (machine.PartInspectionCaptureService == null)
                _bottomCameraBootstrap.CameraType = CameraTypes.PartInspection;

            var capture = machine.PartInspectionCaptureService;
            if (capture?.Camera == null ||
                capture.CameraType != CameraTypes.PartInspection)
            {
                RunoutCameraStatus = "No live bottom-camera service was available. No motion sent.";
                return false;
            }

            // Only switches the optical profile and enables its light/circles.
            // It does not jog the head, rotate the nozzle or modify G92.
            machine.SetVisionProfile(CameraTypes.PartInspection,
                VisionProfile.VisionProfile_Nozzle);
            if (!NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                capture.Profile, out reason))
            {
                RunoutCameraStatus = reason;
                return false;
            }
            machine.SetVisionProfile(CameraTypes.PartInspection,
                VisionProfile.VisionProfile_Nozzle);
            var ready = await capture.EnsureCaptureReadyAsync();
            if (!ready.Successful)
            {
                RunoutCameraStatus = "Bottom camera unavailable: " + ready.ErrorMessage;
                return false;
            }
            if (!RunoutPreviewContext(out reason) ||
                !ReferenceEquals(capture.Profile, _runoutEditingProfile))
            {
                RunoutCameraStatus = reason ?? "Bottom camera changed during optical preview.";
                return false;
            }

            RaisePropertyChanged(nameof(RunoutMachine));
            RaisePropertyChanged(nameof(RunoutSearchSummary));
            RunoutCameraStatus = "Live bottom-camera video ready. No machine motion commanded.";
            return true;
        }

        private bool RunoutPreviewContextForBootstrap(out string reason)
        {
            var machine = RunoutMachine;
            if (machine == null || !machine.Connected ||
                machine.Mode != OperatingMode.Manual || machine.Busy ||
                machine.IsLocating || _locator.IsLocating)
            {
                reason = "Connect the machine, select Manual mode and clear Busy/locator state.";
                return false;
            }
            if (_runoutEditingProfile == null)
                LoadRunoutVisionSettings();
            if (_runoutEditingProfile == null)
            {
                reason = "No Nozzle profile configured for bottom camera.";
                return false;
            }
            reason = null;
            return true;
        }

        public async Task PreviewBottomCameraAsync()
        {
            if (!CanTuneRunoutVision) return;
            _runoutUiContext = SynchronizationContext.Current;
            RunoutPreviewBusy = true;
            try { await PrepareRunoutCameraAsync(); }
            catch (Exception ex) { RunoutCameraStatus = "Could not preview bottom camera: " + ex.Message; }
            finally { RunoutPreviewBusy = false; }
        }

        public async Task ApplyRunoutVisionPreviewAsync()
        {
            if (!CanTuneRunoutVision) return;
            _runoutUiContext = SynchronizationContext.Current;
            RunoutPreviewBusy = true;
            try
            {
                string reason = null;
                if (RunoutVisionTuning == null ||
                    !RunoutVisionTuning.Validate(out reason))
                {
                    RunoutVisionStatus = reason ?? "No valid nozzle profile to preview.";
                    return;
                }
                if (!await PrepareRunoutCameraAsync()) return;
                var capture = RunoutMachine.PartInspectionCaptureService;
                var previous = NozzleRunoutVisionTuning.FromProfile(_runoutEditingProfile);
                try
                {
                    RunoutVisionTuning.ApplyTo(_runoutEditingProfile);
                    RunoutMachine.SetVisionProfile(CameraTypes.PartInspection,
                        VisionProfile.VisionProfile_Nozzle);
                    if (!RunoutPreviewContext(out reason))
                        throw new InvalidOperationException(reason);
                }
                catch
                {
                    previous.ApplyTo(_runoutEditingProfile);
                    RunoutMachine.SetVisionProfile(CameraTypes.PartInspection,
                        VisionProfile.VisionProfile_Nozzle);
                    throw;
                }
                RunoutVisionTuning = NozzleRunoutVisionTuning.FromProfile(_runoutEditingProfile);
                capture.ResetDetections();
                RunoutVideoObservation = null;
                HasRunoutVisionPreview = !_runoutSavedTuning.Matches(_runoutEditingProfile);
                RaisePropertyChanged(nameof(RunoutSearchSummary));
                RunoutVisionStatus = HasRunoutVisionPreview
                    ? "UNSAVED nozzle profile preview. Save Profile or Revert before physical runout."
                    : "Nozzle profile matches saved settings; camera is ready.";
            }
            catch (Exception ex) { RunoutVisionStatus = "Nozzle profile preview failed: " + ex.Message; }
            finally { RunoutPreviewBusy = false; }
        }

        public async Task SaveRunoutVisionAsync()
        {
            if (!CanTuneRunoutVision) return;
            if (HasRunoutVisionDraft)
            {
                RunoutVisionStatus = "Apply Preview first; only verified live settings can be saved.";
                return;
            }
            if (!HasRunoutVisionPreview)
            {
                RunoutVisionStatus = "No applied nozzle-profile changes to save.";
                return;
            }
            if (!RunoutPreviewContext(out var reason) ||
                !ReferenceEquals(RunoutMachine.PartInspectionCaptureService?.Profile,
                    _runoutEditingProfile))
            {
                RunoutVisionStatus = reason ?? "Active Nozzle profile changed before save.";
                return;
            }
            if (_runoutEditingProfile.UseAdaptiveDarkOpening)
            {
                // A per-nozzle optical lock is calculated during each run;
                // persisting it as a camera-wide profile would be unsafe.
                RunoutVisionStatus =
                    "The run-specific dark-opening lock is active. Reopen runout calibration to edit the base Nozzle profile.";
                return;
            }

            RunoutPreviewBusy = true;
            try
            {
                var saved = await _machineRepo.SaveCurrentMachineAsync();
                if (!saved.Successful)
                {
                    RunoutVisionStatus = "Nozzle profile save failed: " + saved.ErrorMessage;
                    return;
                }
                _runoutSavedTuning = NozzleRunoutVisionTuning.FromProfile(_runoutEditingProfile);
                HasRunoutVisionPreview = false;
                RunoutVisionStatus = "Nozzle profile saved to machine configuration. No motion performed.";
            }
            catch (Exception ex) { RunoutVisionStatus = "Nozzle profile save failed: " + ex.Message; }
            finally { RunoutPreviewBusy = false; }
        }

        public void RevertRunoutVision()
        {
            if (!CanTuneRunoutVision) return;
            if (_runoutSavedTuning == null || _runoutEditingProfile == null)
                return;
            try
            {
                if (HasRunoutVisionPreview)
                {
                    _runoutSavedTuning.ApplyTo(_runoutEditingProfile);
                    var capture = RunoutMachine?.PartInspectionCaptureService;
                    if (RunoutMachine?.Connected == true &&
                        ReferenceEquals(capture?.Profile, _runoutEditingProfile) &&
                        !RunoutMachine.Busy)
                        RunoutMachine.SetVisionProfile(CameraTypes.PartInspection,
                            VisionProfile.VisionProfile_Nozzle);
                    capture?.ResetDetections();
                    RunoutVideoObservation = null;
                }
                RunoutVisionTuning = NozzleRunoutVisionTuning.FromProfile(_runoutEditingProfile);
                HasRunoutVisionPreview = false;
                RaisePropertyChanged(nameof(RunoutSearchSummary));
                RunoutVisionStatus = "Restored saved Nozzle profile values.";
            }
            catch (Exception ex) { RunoutVisionStatus = "Could not revert Nozzle profile: " + ex.Message; }
        }

        private void PublishRunoutObservation(MVLocatedCircle circle, bool qualified)
        {
            if (circle == null || _activeMachine?.PartInspectionCaptureService == null)
                return;
            var capture = _activeMachine.PartInspectionCaptureService;
            var center = circle.CenterPixels;
            var offsetPx = circle.OffsetPixels;
            // The legacy OffsetMM rounds to tenths. The 12-angle analysis
            // uses full-precision pixel displacement; diagnostics must agree.
            var validScale = circle.PixelsPerMM > 0 &&
                !double.IsNaN(circle.PixelsPerMM) &&
                !double.IsInfinity(circle.PixelsPerMM);
            var offsetXmm = validScale ? offsetPx.X / circle.PixelsPerMM : 0;
            var offsetYmm = validScale ? -offsetPx.Y / circle.PixelsPerMM : 0;
            var now = DateTimeOffset.UtcNow;
            var observation = new FiducialCircleObservation
            {
                TrackId = circle.Id,
                CameraType = circle.CameraType,
                ObservedAtUtc = now,
                FrameAtUtc = capture.LastProcessedFrameUtc,
                ProfileKey = capture.Profile?.Key,
                PixelsPerMillimeter = circle.PixelsPerMM,
                FrameWidth = capture.CaptureFrameSize.Width,
                FrameHeight = capture.CaptureFrameSize.Height,
                FoundCount = circle.FoundCount,
                Stabilized = circle.Stabilized,
                PositionStable = circle.PositionStable,
                Centered = circle.Centered,
                CenterXpx = center.X,
                CenterYpx = center.Y,
                RadiusPx = circle.RadiusPixels,
                RadiusMm = validScale ? circle.RadiusPixels / circle.PixelsPerMM : 0,
                OffsetXpx = offsetPx.X,
                OffsetYpx = offsetPx.Y,
                OffsetXmm = offsetXmm,
                OffsetYmm = offsetYmm,
                PositionSpreadMm = circle.PositionSpreadMM,
                Qualified = qualified,
                RejectionReason = qualified ? null :
                    "Candidate is not a fresh, stabilized, scale-locked dark nozzle opening."
            };
            void Show()
            {
                if (_activeMachine?.PartInspectionCaptureService != capture)
                    return;
                RunoutVideoObservation = observation;
                RaisePropertyChanged(nameof(RunoutSearchSummary));
            }
            if (_runoutUiContext == null || _runoutUiContext == SynchronizationContext.Current)
                Show();
            else _runoutUiContext.Post(_ => Show(), null);
        }
    }
}
