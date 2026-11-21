using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Optional stagingplatehole-specific vision tuning. This surface exposes
    /// only values actually consumed by the selected production centroid
    /// detector, image preprocessing, camera controls, lighting and rendering.
    ///
    /// Opening/typing never touches hardware. Apply Preview explicitly
    /// activates the profile and starts live imaging, NOT machine motion.
    /// Unsaved previews are reverted upon dialog close; calibration is
    /// blocked until all draft/preview changes are saved or reverted.
    /// </summary>
    public sealed partial class StagingPlateCalibrationViewModel
    {
        private MachineCamera _editingCamera;
        private VisionProfile _editingProfile;
        private StagingHoleVisionTuning _savedVisionSettings;
        private StagingHoleVisionTuning _visionTuning;
        private bool _hasVisionPreview;
        private bool _tuningBusy;
        private string _tuningStatus =
            "Edit staging-hole settings, Apply Preview, Observe, then Save or Revert.";
        private ICircleLocatedHandler _previewHandler;
        private DateTimeOffset _previewStartedUtc;
        private int _previewCircleCount;

        public StagingHoleVisionTuning VisionTuning
        {
            get => _visionTuning;
            private set
            {
                if (_visionTuning != null)
                    _visionTuning.PropertyChanged -= VisionTuning_PropertyChanged;
                Set(ref _visionTuning, value);
                if (_visionTuning != null)
                    _visionTuning.PropertyChanged += VisionTuning_PropertyChanged;
                RaisePropertyChanged(nameof(HasVisionDraftChanges));
                RaisePropertyChanged(nameof(CanRun));
            }
        }

        private void VisionTuning_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            RaisePropertyChanged(nameof(HasVisionDraftChanges));
            RaisePropertyChanged(nameof(CanRun));
        }

        public bool HasVisionDraftChanges =>
            VisionTuning != null && _editingProfile != null &&
            !VisionTuning.Matches(_editingProfile);

        public bool HasVisionPreview
        {
            get => _hasVisionPreview;
            private set
            {
                Set(ref _hasVisionPreview, value);
                RaisePropertyChanged(nameof(CanRun));
            }
        }

        public bool TuningBusy
        {
            get => _tuningBusy;
            private set
            {
                Set(ref _tuningBusy, value);
                RaisePropertyChanged(nameof(CanRun));
                RaisePropertyChanged(nameof(CanClose));
                RaisePropertyChanged(nameof(CanEditVision));
            }
        }

        public bool CanEditVision => !Running && !TuningBusy;

        public string TuningStatus
        {
            get => _tuningStatus;
            private set => Set(ref _tuningStatus, value);
        }

        public int PreviewCircleCount
        {
            get => _previewCircleCount;
            private set => Set(ref _previewCircleCount, value);
        }

        private void LoadVisionSettings()
        {
            // Camera data is authoritative; do not create another VisionProfile.
            var camera = MachineConfiguration?.Cameras?.FirstOrDefault(c =>
                c.CameraType?.Value == CameraTypes.Position);
            var profile = camera?.VisionProfiles?.FirstOrDefault(p =>
                p.Key == VisionProfile.VisionProfile_StagingPlateHole);

            _editingCamera = camera;
            _editingProfile = profile;
            _savedVisionSettings = profile == null
                ? null : StagingHoleVisionTuning.FromProfile(profile);
            VisionTuning = profile == null
                ? null : StagingHoleVisionTuning.FromProfile(profile);
            HasVisionPreview = false;
            if (profile == null)
                TuningStatus = "The position camera requires a stagingplatehole profile before tuning.";
        }

        public void ReportVisionInputError()
        {
            TuningStatus = "Correct the invalid numeric entries before previewing.";
        }

        public void NudgeSearchRegion(double deltaXmm, double deltaYmm)
        {
            if (!CanEditVision || VisionTuning == null) return;
            VisionTuning.SearchOffsetXmm =
                Math.Round(VisionTuning.SearchOffsetXmm + deltaXmm, 3);
            VisionTuning.SearchOffsetYmm =
                Math.Round(VisionTuning.SearchOffsetYmm + deltaYmm, 3);
            TuningStatus = "Search-center draft changed. Apply Preview to update the live detector.";
        }

        private bool BeginTuning()
        {
            if (Running || TuningBusy)
            {
                TuningStatus = "Finish the current measurement before adjusting the profile.";
                return false;
            }
            _uiContext = SynchronizationContext.Current;
            TuningBusy = true;
            return true;
        }

        private bool TuningContextValid(bool requireActiveProfile, out string reason)
        {
            var machine = Machine;
            var camera = machine?.PositionImageCaptureService?.Camera;
            if (machine == null || !machine.Connected ||
                machine.Mode != OperatingMode.Manual || machine.Busy ||
                machine.IsLocating || _locator.IsLocating)
            {
                reason = "Live preview requires connected, idle machine in Manual mode and no other locator.";
                return false;
            }
            if (_editingCamera == null || _editingProfile == null ||
                MachineConfiguration?.Cameras?.Contains(_editingCamera) != true ||
                !ReferenceEquals(camera, _editingCamera) ||
                !_editingCamera.VisionProfiles.Contains(_editingProfile) ||
                _editingCamera.CameraType?.Value != CameraTypes.Position)
            {
                reason = "Position camera or stagingplatehole profile changed; reopen the window.";
                return false;
            }
            if (requireActiveProfile &&
                (!ReferenceEquals(camera.CurrentVisionProfile, _editingProfile) ||
                 machine.PositionImageCaptureService?.Profile?.Key !=
                     VisionProfile.VisionProfile_StagingPlateHole))
            {
                reason = "Active camera profile is no longer stagingplatehole.";
                return false;
            }
            reason = null;
            return true;
        }

        private void InvalidateVisionEvidence(string message)
        {
            CurrentObservation = null;
            ClearPreviousMeasurement();
            Machine?.PositionImageCaptureService?.ResetDetections();
            ResetStages();
            Status = message;
            Log(message);
        }

        public async Task ApplyVisionPreviewAsync()
        {
            if (!BeginTuning()) return;
            try
            {
                if (!TuningContextValid(false, out var reason))
                {
                    TuningStatus = reason;
                    return;
                }
                if (VisionTuning == null || !VisionTuning.Validate(out reason))
                {
                    TuningStatus = "Invalid staging-hole settings: " + reason;
                    return;
                }

                Machine.SetVisionProfile(CameraTypes.Position,
                    VisionProfile.VisionProfile_StagingPlateHole);
                var capture = Machine.PositionImageCaptureService;
                if (!TuningContextValid(true, out reason))
                {
                    TuningStatus = reason;
                    return;
                }
                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful || !TuningContextValid(true, out reason))
                {
                    TuningStatus = !ready.Successful
                        ? "No fresh position-camera image: " + ready.ErrorMessage
                        : reason;
                    return;
                }
                var effectiveScale = VisionTuning.PixelsPerMm ??
                    _editingCamera.PixelsPerMM;
                if (!VisionTuning.FitsFrame(capture.CaptureFrameSize.Width,
                    capture.CaptureFrameSize.Height, effectiveScale, out reason))
                {
                    TuningStatus = reason;
                    return;
                }

                var previous = StagingHoleVisionTuning.FromProfile(_editingProfile);
                try
                {
                    VisionTuning.ApplyTo(_editingProfile);
                    // Re-selecting an active profile also applies physical
                    // RGB light/power settings to the top camera.
                    Machine.SetVisionProfile(CameraTypes.Position,
                        VisionProfile.VisionProfile_StagingPlateHole);
                }
                catch
                {
                    previous.ApplyTo(_editingProfile);
                    Machine.SetVisionProfile(CameraTypes.Position,
                        VisionProfile.VisionProfile_StagingPlateHole);
                    throw;
                }

                VisionTuning = StagingHoleVisionTuning.FromProfile(_editingProfile);
                HasVisionPreview = !_savedVisionSettings.Matches(_editingProfile);
                InvalidateVisionEvidence("Vision preview applied to the live staging-hole profile; previous circles invalidated.");
                RaisePropertyChanged(nameof(PixelsPerMillimeter));
                TuningStatus = HasVisionPreview
                    ? "UNSAVED PREVIEW: inspect the live search outline, then Save Profile or Revert before running calibration."
                    : "Saved profile values are active. Observe detection before beginning calibration.";
            }
            catch (Exception ex)
            {
                TuningStatus = "Cannot apply preview: " + ex.Message;
            }
            finally { TuningBusy = false; }
        }

        public async Task SaveVisionProfileAsync()
        {
            if (!BeginTuning()) return;
            try
            {
                if (!TuningContextValid(true, out var reason))
                {
                    TuningStatus = reason;
                    return;
                }
                if (HasVisionDraftChanges)
                {
                    TuningStatus = "Apply the current draft before saving; only the previewed profile is saved.";
                    return;
                }
                if (!HasVisionPreview)
                {
                    TuningStatus = "No previewed profile changes to save.";
                    return;
                }
                var saved = await MachineRepo.SaveCurrentMachineAsync();
                if (!saved.Successful)
                {
                    TuningStatus = "Could not save staging vision profile: " + saved.ErrorMessage +
                        ". Preview is still unsaved; choose Save or Revert.";
                    return;
                }
                _savedVisionSettings = StagingHoleVisionTuning.FromProfile(_editingProfile);
                HasVisionPreview = false;
                TuningStatus = "The stagingplatehole profile was saved. No machine movement or plate alignment was performed.";
                Log(TuningStatus);
            }
            catch (Exception ex) { TuningStatus = "Vision profile save failed: " + ex.Message; }
            finally { TuningBusy = false; }
        }

        public void RevertVisionPreview()
        {
            if (!CanEditVision) return;
            if (_savedVisionSettings == null || _editingProfile == null) return;
            try
            {
                if (HasVisionPreview)
                {
                    _savedVisionSettings.ApplyTo(_editingProfile);
                    if (Machine?.Connected == true &&
                        ReferenceEquals(Machine.PositionImageCaptureService?.Camera,
                            _editingCamera) &&
                        ReferenceEquals(_editingCamera.CurrentVisionProfile, _editingProfile))
                        Machine.SetVisionProfile(CameraTypes.Position,
                            VisionProfile.VisionProfile_StagingPlateHole);
                    InvalidateVisionEvidence("Unsaved staging-hole preview reverted; detection history cleared.");
                }
                VisionTuning = StagingHoleVisionTuning.FromProfile(_editingProfile);
                HasVisionPreview = false;
                RaisePropertyChanged(nameof(PixelsPerMillimeter));
                TuningStatus = "Restored saved staging-hole profile values.";
            }
            catch (Exception ex)
            {
                TuningStatus = "Could not restore previous profile: " + ex.Message;
            }
        }

        private sealed class PreviewCircleHandler : ICircleLocatedHandler
        {
            private readonly StagingPlateCalibrationViewModel _owner;
            public PreviewCircleHandler(StagingPlateCalibrationViewModel owner) =>
                _owner = owner;
            public void CircleLocated(MVLocatedCircle circle)
            {
                if (ReferenceEquals(_owner._previewHandler, this))
                    _owner.RecordVisionPreview(circle, this);
            }
            public void CircleLocatorTimeout() { }
            public void CircleLocatorAborted() { }
        }

        private void RecordVisionPreview(MVLocatedCircle circle, ICircleLocatedHandler handler)
        {
            if (circle?.CameraType != CameraTypes.Position ||
                !ReferenceEquals(_previewHandler, handler) ||
                !ReferenceEquals(_editingCamera?.CurrentVisionProfile, _editingProfile))
                return;
            var capture = Machine?.PositionImageCaptureService;
            if (capture == null ||
                !ReferenceEquals(capture.Camera, _editingCamera) ||
                !ReferenceEquals(capture.Profile, _editingProfile) ||
                Machine?.Busy != false) return;

            var now = DateTimeOffset.UtcNow;
            var center = circle.CenterPixels;
            var offsetMm = circle.OffsetMM;
            var offsetPx = circle.OffsetPixels;
            var observation = new FiducialCircleObservation
            {
                TrackId = circle.Id,
                CameraType = circle.CameraType,
                ProfileKey = _editingProfile.Key,
                ObservedAtUtc = now,
                FrameAtUtc = capture.LastProcessedFrameUtc,
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
                RadiusMm = circle.RadiusMM,
                OffsetXpx = offsetPx.X,
                OffsetYpx = offsetPx.Y,
                OffsetXmm = offsetMm.X,
                OffsetYmm = offsetMm.Y,
                PositionSpreadMm = circle.PositionSpreadMM,
                SearchRadiusPx = _editingProfile.TargetImageRadius *
                    _editingCamera.EffectivePixelsPerMM,
                ErrorToleranceMm = _editingProfile.ErrorToleranceMM
            };

            var accepted = FiducialCalibrationGate.TryQualify(observation,
                VisionProfile.VisionProfile_StagingPlateHole,
                _editingCamera.EffectivePixelsPerMM, _previewStartedUtc,
                now, out var rejection);
            observation.Qualified = accepted;
            observation.RejectionReason = rejection;

            void Publish()
            {
                if (!ReferenceEquals(_previewHandler, handler)) return;
                CurrentObservation = observation;
                PreviewCircleCount++;
                LastRejection = accepted
                    ? "Live preview: stable centroid detected. No motion commanded."
                    : rejection;
            }
            if (_uiContext == null || _uiContext == SynchronizationContext.Current)
                Publish();
            else _uiContext.Post(_ => Publish(), null);
        }

        public async Task ObserveVisionPreviewAsync()
        {
            if (!BeginTuning()) return;
            ICircleLocatedHandler handler = null;
            try
            {
                if (!TuningContextValid(false, out var reason))
                {
                    TuningStatus = reason;
                    return;
                }
                if (HasVisionDraftChanges)
                {
                    TuningStatus = "Apply Preview first so the detector uses the edited search and image-processing parameters.";
                    return;
                }
                Machine.SetVisionProfile(CameraTypes.Position,
                    VisionProfile.VisionProfile_StagingPlateHole);
                var capture = Machine.PositionImageCaptureService;
                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful || !TuningContextValid(true, out reason))
                {
                    TuningStatus = !ready.Successful
                        ? "Cannot preview without fresh camera frames: " + ready.ErrorMessage
                        : reason;
                    return;
                }
                capture.ResetDetections();
                PreviewCircleCount = 0;
                CurrentObservation = null;
                _previewStartedUtc = DateTimeOffset.UtcNow;
                handler = new PreviewCircleHandler(this);
                _previewHandler = handler;
                _locator.RegisterCircleLocatedHandler(handler, 9000);
                TuningStatus = "Observing staging-hole centroid detector for 8 seconds. Machine will not move.";
                await Task.Delay(8000);
                TuningStatus = "Preview completed: " + PreviewCircleCount +
                    " centroid observations. Old preview frames cannot authorize machine motion.";
                Log(TuningStatus);
            }
            catch (Exception ex) { TuningStatus = "Vision observation failed: " + ex.Message; }
            finally
            {
                if (handler != null)
                {
                    _previewHandler = null;
                    _locator.UnregisterCircleLocatedHandler(handler);
                }
                TuningBusy = false;
            }
        }
    }
}
