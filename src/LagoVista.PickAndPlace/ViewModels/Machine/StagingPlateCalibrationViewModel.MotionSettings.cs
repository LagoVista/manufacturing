using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Independent, per-plate motion correction limits. Draft edits never
    /// affect live motion; Save Motion Limits validates and persists them
    /// before any future calibration is allowed to use them.
    /// </summary>
    public sealed partial class StagingPlateCalibrationViewModel
    {
        private StagingPlateCenteringSettings _motionSettings;
        private bool _motionSettingsBusy;
        private string _motionSettingsStatus =
            "Per-plate correction limits are not camera/vision profile values. Review and save before running.";

        public StagingPlateCenteringSettings MotionSettings
        {
            get => _motionSettings;
            private set
            {
                if (_motionSettings != null)
                    _motionSettings.PropertyChanged -= MotionSettings_PropertyChanged;
                Set(ref _motionSettings, value);
                if (_motionSettings != null)
                    _motionSettings.PropertyChanged += MotionSettings_PropertyChanged;
                RaisePropertyChanged(nameof(HasMotionDraftChanges));
                RaisePropertyChanged(nameof(CanRun));
            }
        }

        private void MotionSettings_PropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(HasMotionDraftChanges));
            RaisePropertyChanged(nameof(CanRun));
        }

        public bool HasMotionDraftChanges =>
            MotionSettings != null && SelectedPlate != null &&
            !MotionSettings.Matches(SelectedPlate);

        public bool MotionSettingsBusy
        {
            get => _motionSettingsBusy;
            private set
            {
                Set(ref _motionSettingsBusy, value);
                RaisePropertyChanged(nameof(CanRun));
                RaisePropertyChanged(nameof(CanClose));
                RaisePropertyChanged(nameof(CanEditVision));
                RaisePropertyChanged(nameof(CanEditMotion));
            }
        }

        public bool CanEditMotion => !Running && !TuningBusy && !MotionSettingsBusy;

        public string MotionSettingsStatus
        {
            get => _motionSettingsStatus;
            private set => Set(ref _motionSettingsStatus, value);
        }

        private void LoadMotionSettings()
        {
            MotionSettings = SelectedPlate == null
                ? null : StagingPlateCenteringSettings.FromPlate(SelectedPlate);
            MotionSettingsStatus = SelectedPlate == null
                ? "Select a staging plate to adjust centering motion limits."
                : "Current plate limit: " +
                    SelectedPlate.MaxCenteringCorrectionPerAxisMm.ToString("F1") +
                    " mm per axis, up to " +
                    SelectedPlate.MaximumCenteringMoves +
                    " corrections. Hard ceiling: " +
                    StagingPlateCenteringPolicy.HardCorrectionPerAxisCeilingMm.ToString("F1") +
                    " mm per axis, " +
                    StagingPlateCenteringPolicy.HardMaximumCorrections +
                    " corrections.";
        }

        public void ReportMotionInputError()
        {
            MotionSettingsStatus =
                "Correct invalid motion-limit numbers before saving them.";
        }

        public async Task SaveMotionSettingsAsync()
        {
            if (!CanEditMotion)
            {
                MotionSettingsStatus =
                    "Wait for calibration or another settings operation to finish.";
                return;
            }
            if (HasVisionDraftChanges || HasVisionPreview)
            {
                MotionSettingsStatus =
                    "Save or revert the staging vision profile first. Motion limits are stored separately.";
                return;
            }
            if (MotionSettings == null || SelectedPlate == null ||
                MachineConfiguration?.StagingPlates?.Contains(SelectedPlate) != true)
            {
                MotionSettingsStatus =
                    "Selected staging plate is unavailable or changed; reopen calibration.";
                return;
            }
            if (!MotionSettings.Validate(out var reason))
            {
                MotionSettingsStatus = reason;
                return;
            }
            if (Machine?.Busy == true || Machine?.IsLocating == true ||
                _locator.IsLocating)
            {
                MotionSettingsStatus =
                    "Finish outstanding machine motion or circle detection before saving settings.";
                return;
            }
            if (!HasMotionDraftChanges)
            {
                MotionSettingsStatus = "No motion limit changes to save.";
                return;
            }

            MotionSettingsBusy = true;
            var selected = SelectedPlate;
            var previous = StagingPlateCenteringSettings.FromPlate(selected);
            var saved = false;
            try
            {
                MotionSettings.ApplyTo(selected);
                var result = await MachineRepo.SaveCurrentMachineAsync();
                if (!result.Successful)
                {
                    MotionSettingsStatus = "Could not save plate motion limits: " +
                        result.ErrorMessage;
                    return;
                }

                saved = true;
                MotionSettings = StagingPlateCenteringSettings.FromPlate(selected);
                MotionSettingsStatus = "Saved for " + selected.Name +
                    ": " + selected.MaxCenteringCorrectionPerAxisMm.ToString("F1") +
                    " mm per axis and " + selected.MaximumCenteringMoves +
                    " corrections. Physical XY limits and safe-Z still apply.";
                Log(MotionSettingsStatus);
            }
            catch (Exception ex)
            {
                MotionSettingsStatus = "Motion limit save failed: " + ex.Message;
            }
            finally
            {
                if (!saved)
                    previous.ApplyTo(selected);
                MotionSettingsBusy = false;
            }
        }

        public void RevertMotionSettings()
        {
            if (!CanEditMotion) return;
            LoadMotionSettings();
        }
    }
}
