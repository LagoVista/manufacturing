using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using LagoVista.Core.Models.Drawing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Optional T009 operator controls. All machine motion and profile writes are
    /// explicitly requested; nothing starts simply by opening the window.
    /// </summary>
    public sealed partial class MachineFiducialCalibrationViewModel
    {
        private MachineCamera _tuningCamera;
        private VisionProfile _tuningProfile;
        private FiducialDetectorTuning _savedTuning;
        private FiducialDetectorTuning _tuning;
        private bool _unsavedTuning;
        private bool _manualBusy;
        private string _manualStatus =
            "Edit Hough values, Apply Preview, observe, then Save or Revert. Manual jog requires camera navigation.";

        private CirclePreviewHandler _previewHandler;
        private DateTimeOffset _previewStartedAt;
        private int _previewObservationCount;

        public FiducialDetectorTuning Tuning
        {
            get => _tuning;
            private set => Set(ref _tuning, value);
        }

        public string ManualStatus
        {
            get => _manualStatus;
            private set => Set(ref _manualStatus, value);
        }

        public bool HasUnsavedTuning
        {
            get => _unsavedTuning;
            private set
            {
                Set(ref _unsavedTuning, value);
                RaisePropertyChanged(nameof(CanRun));
            }
        }

        public bool ManualBusy
        {
            get => _manualBusy;
            private set
            {
                Set(ref _manualBusy, value);
                RaisePropertyChanged(nameof(CanRun));
                RaisePropertyChanged(nameof(CanClose));
                RaisePropertyChanged(nameof(CanManualAdjust));
            }
        }

        public bool CanClose => !Running && !ManualBusy;
        public bool CanManualAdjust => !Running && !ManualBusy;
        public int PreviewObservationCount
        {
            get => _previewObservationCount;
            private set => Set(ref _previewObservationCount, value);
        }

        public void ReportInvalidTuningInput()
        {
            ManualStatus = "Please correct the highlighted numeric input before applying Hough tuning.";
        }

        private void InitializeManualControls()
        {
            var camera = Machine?.PositionImageCaptureService?.Camera ??
                MachineConfiguration?.Cameras?.FirstOrDefault(c =>
                    c.CameraType?.Value == CameraTypes.Position);
            var profile = camera?.VisionProfiles?.FirstOrDefault(p =>
                p.Key == VisionProfile.VisionProfile_MachineFiducual);
            _tuningCamera = camera;
            _tuningProfile = profile;
            if (profile != null)
            {
                _savedTuning = FiducialDetectorTuning.FromProfile(profile);
                Tuning = FiducialDetectorTuning.FromProfile(profile);
            }
            else
            {
                Tuning = new FiducialDetectorTuning();
                ManualStatus = "Configure an active position camera and macfiducial profile first.";
            }
        }

        private bool PreflightManual(out IImageCaptureService capture, out string reason)
        {
            capture = null;
            var machine = Machine;
            if (machine == null || !machine.Connected || !machine.WasMachineHomed ||
                machine.Mode != OperatingMode.Manual || machine.Busy ||
                machine.IsLocating || _locator.IsLocating)
            {
                reason = "Manual adjustment requires a connected, homed, idle machine in Manual mode and no active locator.";
                return false;
            }

            capture = machine.PositionImageCaptureService;
            if (capture == null || capture.Camera == null ||
                capture.Camera.CameraType?.Value != CameraTypes.Position)
            {
                reason = "Select the live position camera before manual adjustment.";
                return false;
            }

            // Window may have been opened before the position camera/profile
            // was loaded. Refresh only if no uncommitted changes exist.
            if ((_tuningCamera == null || _tuningProfile == null) && !HasUnsavedTuning)
                InitializeManualControls();

            if (_tuningProfile == null || _savedTuning == null ||
                !ReferenceEquals(capture.Camera, _tuningCamera) ||
                MachineConfiguration?.Cameras?.Contains(_tuningCamera) != true)
            {
                reason = "Machine/camera changed or macfiducial profile unavailable; close and reopen calibration.";
                return false;
            }

            if (!_tuningCamera.VisionProfiles.Contains(_tuningProfile))
            {
                reason = "Machine-fiducial profile was replaced; reopen calibration.";
                return false;
            }
            if (!_tuningProfile.PerformShapeDetection || !_tuningProfile.FindCircles)
            {
                reason = "The macfiducial profile must enable the production circle detector.";
                return false;
            }

            reason = null;
            return true;
        }

        private bool BeginManual()
        {
            if (Running || ManualBusy)
            {
                ManualStatus = "Finish the current calibration or preview before making manual changes.";
                return false;
            }
            ManualBusy = true;
            _uiContext = SynchronizationContext.Current;
            return true;
        }

        private void ClearManualObservation(string message)
        {
            CurrentObservation = null;
            LastRejection = "Manual adjustment invalidated prior detection; collect fresh frames.";
            Machine?.PositionImageCaptureService?.ResetDetections();
            ResetSteps();
            Status = message;
            Log(message);
        }

        public async Task ApplyTuningPreviewAsync()
        {
            if (!BeginManual()) return;
            try
            {
                if (!PreflightManual(out var capture, out var reason))
                {
                    ManualStatus = reason;
                    return;
                }
                if (Tuning == null || !Tuning.Validate(out reason))
                {
                    ManualStatus = "Invalid Hough parameters: " + reason;
                    return;
                }

                Machine.SetVisionProfile(CameraTypes.Position,
                    VisionProfile.VisionProfile_MachineFiducual);
                if (!ReferenceEquals(_tuningCamera.CurrentVisionProfile, _tuningProfile))
                {
                    ManualStatus = "macfiducial profile changed during preview setup.";
                    return;
                }
                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful)
                {
                    ManualStatus = "Live camera unavailable: " + ready.ErrorMessage;
                    return;
                }
                if (!Machine.Connected || Machine.Busy ||
                    !ReferenceEquals(capture.Camera, _tuningCamera) ||
                    !ReferenceEquals(_tuningCamera.CurrentVisionProfile, _tuningProfile))
                {
                    ManualStatus = "Camera or machine changed while setting up preview.";
                    return;
                }

                var rollback = FiducialDetectorTuning.FromProfile(_tuningProfile);
                try { Tuning.ApplyTo(_tuningProfile); }
                catch
                {
                    rollback.ApplyTo(_tuningProfile);
                    throw;
                }

                // VisionProfile normalizes some numeric inputs (e.g. radius,
                // thresholds). Reflect the actual applied values in the editor.
                Tuning = FiducialDetectorTuning.FromProfile(_tuningProfile);
                HasUnsavedTuning = !_savedTuning.Matches(_tuningProfile);
                ClearManualObservation("Hough parameters updated for preview; prior circle evidence cleared.");
                ManualStatus = HasUnsavedTuning
                    ? "PREVIEW ONLY — not saved. Run an 8-second detection preview, then Save Profile or Revert before automatic calibration."
                    : "Current saved parameters restored. Run a detection preview to inspect them.";
            }
            catch (Exception ex)
            {
                ManualStatus = "Could not preview Hough settings: " + ex.Message;
            }
            finally { ManualBusy = false; }
        }

        public async Task SaveTuningAsync()
        {
            if (!BeginManual()) return;
            try
            {
                if (!HasUnsavedTuning)
                {
                    ManualStatus = "No applied tuning changes to save.";
                    return;
                }
                if (!PreflightManual(out var capture, out var reason) ||
                    !ReferenceEquals(capture?.Profile, _tuningProfile))
                {
                    ManualStatus = "Tuning save blocked: " + (reason ??
                        "macfiducial is no longer the active profile.");
                    return;
                }
                var result = await MachineRepo.SaveCurrentMachineAsync();
                if (!result.Successful)
                {
                    ManualStatus = "Save failed. Preview remains unsaved: " + result.ErrorMessage;
                    return;
                }

                _savedTuning = FiducialDetectorTuning.FromProfile(_tuningProfile);
                HasUnsavedTuning = false;
                ManualStatus = "macfiducial Hough tuning saved. Previous circle evidence remains invalid.";
                Log(ManualStatus);
            }
            catch (Exception ex) { ManualStatus = "Tuning save failed: " + ex.Message; }
            finally { ManualBusy = false; }
        }

        public void RevertTuningPreview()
        {
            if (Running || ManualBusy) return;
            try
            {
                if (_savedTuning != null && _tuningProfile != null &&
                    _tuningCamera != null &&
                    MachineConfiguration?.Cameras?.Contains(_tuningCamera) == true &&
                    _tuningCamera.VisionProfiles.Contains(_tuningProfile))
                {
                    if (HasUnsavedTuning)
                        _savedTuning.ApplyTo(_tuningProfile);
                    Tuning = FiducialDetectorTuning.FromProfile(_tuningProfile);
                    HasUnsavedTuning = false;
                    ClearManualObservation("Unsaved tuning discarded; verified calibration evidence reset.");
                    ManualStatus = "Restored the last saved macfiducial tuning.";
                }
            }
            catch (Exception ex)
            {
                ManualStatus = "Could not restore tuning: " + ex.Message;
            }
        }

        private sealed class CirclePreviewHandler : ICircleLocatedHandler
        {
            private readonly MachineFiducialCalibrationViewModel _owner;
            public CirclePreviewHandler(MachineFiducialCalibrationViewModel owner) => _owner = owner;
            public void CircleLocated(MVLocatedCircle circle)
            {
                if (ReferenceEquals(_owner._previewHandler, this))
                    _owner.RecordPreviewCircle(circle);
            }
            public void CircleLocatorTimeout() { }
            public void CircleLocatorAborted() { }
        }

        private void RecordPreviewCircle(MVLocatedCircle circle)
        {
            var capture = Machine?.PositionImageCaptureService;
            var camera = capture?.Camera;
            if (circle == null || circle.CameraType != CameraTypes.Position ||
                !ReferenceEquals(camera, _tuningCamera) ||
                !ReferenceEquals(camera.CurrentVisionProfile, _tuningProfile) ||
                !ReferenceEquals(capture.Profile, _tuningProfile)) return;

            var now = DateTimeOffset.UtcNow;
            var offset = circle.OffsetMM;
            var offsetPx = circle.OffsetPixels;
            var center = circle.CenterPixels;
            var observation = new FiducialCircleObservation
            {
                TrackId = circle.Id,
                CameraType = circle.CameraType,
                ObservedAtUtc = now,
                FrameAtUtc = capture.LastProcessedFrameUtc,
                ProfileKey = capture.Profile.Key,
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
                OffsetXmm = offset.X,
                OffsetYmm = offset.Y,
                PositionSpreadMm = circle.PositionSpreadMM,
                SearchRadiusPx = _tuningProfile.TargetImageRadius * camera.EffectivePixelsPerMM,
                ErrorToleranceMm = _tuningProfile.ErrorToleranceMM
            };
            observation.Qualified = FiducialCalibrationGate.TryQualify(observation,
                VisionProfile.VisionProfile_MachineFiducual,
                camera.EffectivePixelsPerMM, _previewStartedAt, now, out var reject);
            observation.RejectionReason = reject;

            void Publish()
            {
                if (!ManualBusy || _previewHandler == null) return;
                CurrentObservation = observation;
                PreviewObservationCount++;
                if (observation.Qualified)
                    LastRejection = "Preview: stable Hough circle observed.";
                else
                    LastRejection = reject;
            }

            if (_uiContext == null || SynchronizationContext.Current == _uiContext)
                Publish();
            else
                _uiContext.Post(_ => Publish(), null);
        }

        public async Task ObserveTuningAsync()
        {
            if (!BeginManual()) return;
            CirclePreviewHandler handler = null;
            try
            {
                if (!PreflightManual(out var capture, out var reason))
                {
                    ManualStatus = reason;
                    return;
                }
                Machine.SetVisionProfile(CameraTypes.Position,
                    VisionProfile.VisionProfile_MachineFiducual);
                if (!ReferenceEquals(_tuningCamera.CurrentVisionProfile, _tuningProfile))
                {
                    ManualStatus = "The macfiducial profile is no longer current.";
                    return;
                }
                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful)
                {
                    ManualStatus = "Cannot observe without fresh live frames: " +
                        ready.ErrorMessage;
                    return;
                }
                capture.ResetDetections();
                CurrentObservation = null;
                PreviewObservationCount = 0;
                _previewStartedAt = DateTimeOffset.UtcNow;
                handler = new CirclePreviewHandler(this);
                _previewHandler = handler;
                _locator.RegisterCircleLocatedHandler(handler, 9000);
                ManualStatus = "Observing real Hough-circle detections for 8 seconds. Machine will not move.";
                await Task.Delay(8000);
                ManualStatus = "Preview finished: " + PreviewObservationCount +
                    " observed circle updates. Measurements are historical; use fresh evidence for origin reset.";
                Log(ManualStatus);
            }
            catch (Exception ex)
            {
                ManualStatus = "Circle preview failed: " + ex.Message;
            }
            finally
            {
                if (handler != null)
                {
                    _previewHandler = null;
                    _locator.UnregisterCircleLocatedHandler(handler);
                }
                ManualBusy = false;
            }
        }

        /// <summary>
        /// A bounded, single-axis, operator-requested XY jog. It requires
        /// camera navigation and performs safe-Z and machine-idle checks.
        /// A jog never resets machine coordinates or saves calibration.
        /// </summary>
        public async Task JogAsync(double xMm, double yMm)
        {
            if (!BeginManual()) return;
            try
            {
                if (!PreflightManual(out var capture, out var reason))
                {
                    ManualStatus = reason;
                    return;
                }
                if (Machine.CurrentMachineToolHead != null ||
                    Machine.ViewType != LagoVista.PickAndPlace.ViewTypes.Camera)
                {
                    ManualStatus = "Select position-camera navigation first; never jog while a tool head or moving view is active.";
                    return;
                }

                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful)
                {
                    ManualStatus = "No live position camera: " + ready.ErrorMessage;
                    return;
                }
                var current = Machine.MachinePosition;
                var limits = Machine.Settings.WorkAreaSize;
                if (!FiducialManualJogPolicy.TryValidate(
                    current.X, current.Y, xMm, yMm, limits.X, limits.Y, out reason))
                {
                    ManualStatus = "Jog rejected: " + reason;
                    return;
                }

                Machine.SetVisionProfile(CameraTypes.Position,
                    VisionProfile.VisionProfile_MachineFiducual);
                if (!ReferenceEquals(_tuningCamera.CurrentVisionProfile, _tuningProfile))
                {
                    ManualStatus = "Active macfiducial profile was replaced.";
                    return;
                }

                Machine.SendSafeMoveHeight();
                var safe = await Machine.SpinUntilIdleAsync(10000);
                if (!safe.Successful || !Machine.Connected || Machine.Mode != OperatingMode.Manual)
                {
                    ManualStatus = "Safe-Z move not acknowledged; XY jog was not issued.";
                    return;
                }
                if (Machine.Busy || Machine.IsLocating ||
                    Machine.CurrentMachineToolHead != null ||
                    Machine.ViewType != LagoVista.PickAndPlace.ViewTypes.Camera ||
                    !ReferenceEquals(Machine.PositionImageCaptureService?.Camera, _tuningCamera) ||
                    !ReferenceEquals(_tuningCamera.CurrentVisionProfile, _tuningProfile) ||
                    !FiducialManualJogPolicy.TryValidate(
                        Machine.MachinePosition.X, Machine.MachinePosition.Y,
                        xMm, yMm, limits.X, limits.Y, out reason))
                {
                    ManualStatus = "Machine context or safe destination changed; XY jog was not issued.";
                    return;
                }
                Machine.GotoPoint(new Point2D<double>(xMm, yMm), relativeMove: true);
                var moved = await Machine.SpinUntilIdleAsync(10000);
                if (!moved.Successful || !Machine.Connected)
                {
                    ManualStatus = "XY jog was issued but completion is unverified. Check machine before further motion.";
                    return;
                }
                _moveCompletedUtc = DateTimeOffset.UtcNow;
                ClearManualObservation("Manual position changed; automatic calibration requires fresh circle evidence.");
                RaisePropertyChanged(nameof(MachinePositionSummary));
                ManualStatus = "Jogged X=" + xMm.ToString("+0.0;-0.0;0.0") +
                    " mm, Y=" + yMm.ToString("+0.0;-0.0;0.0") +
                    " mm. No origin reset or save was performed.";
                Log(ManualStatus);
            }
            catch (Exception ex)
            {
                ManualStatus = "Jog stopped: " + ex.Message +
                    ". Verify machine state before further motion.";
            }
            finally { ManualBusy = false; }
        }

    }
}
