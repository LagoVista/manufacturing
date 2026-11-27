using LagoVista.Core.Models;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Source-audited NOZZLE profile properties consumed by the 12-angle
    /// optical runout acquisition, camera/lighting or display paths.
    /// Dark-opening lock/diameter are transient products of the procedure,
    /// not user-settable or saved tuning inputs. Hough/contour thresholds do
    /// not drive the scale-locked dark-opening locator.
    /// </summary>
    public sealed class NozzleRunoutVisionTuning : ModelBase
    {
        private double _searchRadiusMm, _centerToleranceMm, _zoomLevel;
        private int _stabilizationCount;
        private double? _exposure, _brightness, _contrast, _focus, _pixelsPerMm;
        private byte? _lightPower, _lightRed, _lightGreen, _lightBlue;
        private bool _showOriginalImage, _showCrossHairs, _showCalibrationSquare;

        public double SearchRadiusMm { get => _searchRadiusMm; set => Set(ref _searchRadiusMm, value); }
        public double CenterToleranceMm { get => _centerToleranceMm; set => Set(ref _centerToleranceMm, value); }
        public int StabilizationCount { get => _stabilizationCount; set => Set(ref _stabilizationCount, value); }
        public double? Exposure { get => _exposure; set => Set(ref _exposure, value); }
        public double? Brightness { get => _brightness; set => Set(ref _brightness, value); }
        public double? Contrast { get => _contrast; set => Set(ref _contrast, value); }
        public double? Focus { get => _focus; set => Set(ref _focus, value); }
        public double? PixelsPerMm { get => _pixelsPerMm; set => Set(ref _pixelsPerMm, value); }
        public byte? LightPower { get => _lightPower; set => Set(ref _lightPower, value); }
        public byte? LightRed { get => _lightRed; set => Set(ref _lightRed, value); }
        public byte? LightGreen { get => _lightGreen; set => Set(ref _lightGreen, value); }
        public byte? LightBlue { get => _lightBlue; set => Set(ref _lightBlue, value); }
        public bool ShowOriginalImage { get => _showOriginalImage; set => Set(ref _showOriginalImage, value); }
        public bool ShowCrossHairs { get => _showCrossHairs; set => Set(ref _showCrossHairs, value); }
        public bool ShowCalibrationSquare { get => _showCalibrationSquare; set => Set(ref _showCalibrationSquare, value); }
        public double ZoomLevel { get => _zoomLevel; set => Set(ref _zoomLevel, value); }

        public static NozzleRunoutVisionTuning FromProfile(VisionProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return new NozzleRunoutVisionTuning
            {
                SearchRadiusMm = profile.TargetImageRadius,
                CenterToleranceMm = profile.ErrorToleranceMM,
                StabilizationCount = profile.StabilizationCount,
                Exposure = profile.Exposure, Brightness = profile.Brightness,
                Contrast = profile.Contrast, Focus = profile.Focus,
                PixelsPerMm = profile.PixelsPerMM,
                LightPower = profile.LightPower, LightRed = profile.LightRed,
                LightGreen = profile.LightGreen, LightBlue = profile.LightBlue,
                ShowOriginalImage = profile.ShowOriginalImage,
                ShowCrossHairs = profile.ShowCrossHairs,
                ShowCalibrationSquare = profile.Show200PixelSquare,
                ZoomLevel = profile.ZoomLevel
            };
        }

        public bool Validate(out string reason)
        {
            if (!InRange(SearchRadiusMm, 0.1, 30))
                reason = "Nozzle acquisition radius must be 0.1-30 mm.";
            else if (!InRange(CenterToleranceMm, 0.01, 0.5))
                reason = "Nozzle centering tolerance must be 0.01-0.5 mm (runner caps at 0.10 mm).";
            else if (StabilizationCount < 2 || StabilizationCount > 60)
                reason = "Stable nozzle observations must be 2-60 frames.";
            else if (!ValidNullable(Exposure, -100, 100) ||
                     !ValidNullable(Brightness, -1000, 1000) ||
                     !ValidNullable(Contrast, -1000, 1000) ||
                     !ValidNullable(Focus, -1000, 1000))
                reason = "Camera overrides must be finite and within supported bounds.";
            else if (!ValidNullable(PixelsPerMm, 0.1, 2000))
                reason = "Scale must be blank (inherit) or 0.1-2000 px/mm.";
            else if (!InRange(ZoomLevel, 0.1, 20))
                reason = "Preview zoom must be 0.1-20.";
            else { reason = null; return true; }
            return false;
        }

        public bool Matches(VisionProfile profile) =>
            profile != null &&
            SearchRadiusMm == profile.TargetImageRadius &&
            CenterToleranceMm == profile.ErrorToleranceMM &&
            StabilizationCount == profile.StabilizationCount &&
            Exposure == profile.Exposure && Brightness == profile.Brightness &&
            Contrast == profile.Contrast && Focus == profile.Focus &&
            PixelsPerMm == profile.PixelsPerMM &&
            LightPower == profile.LightPower && LightRed == profile.LightRed &&
            LightGreen == profile.LightGreen && LightBlue == profile.LightBlue &&
            ShowOriginalImage == profile.ShowOriginalImage &&
            ShowCrossHairs == profile.ShowCrossHairs &&
            ShowCalibrationSquare == profile.Show200PixelSquare &&
            ZoomLevel == profile.ZoomLevel;

        public void ApplyTo(VisionProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!Validate(out var reason)) throw new InvalidOperationException(reason);
            profile.TargetImageRadius = SearchRadiusMm;
            profile.ErrorToleranceMM = CenterToleranceMm;
            profile.StabilizationCount = StabilizationCount;
            profile.Exposure = Exposure;
            profile.Brightness = Brightness;
            profile.Contrast = Contrast;
            profile.Focus = Focus;
            profile.PixelsPerMM = PixelsPerMm;
            profile.LightPower = LightPower;
            profile.LightRed = LightRed;
            profile.LightGreen = LightGreen;
            profile.LightBlue = LightBlue;
            profile.ShowOriginalImage = ShowOriginalImage;
            profile.ShowCrossHairs = ShowCrossHairs;
            profile.Show200PixelSquare = ShowCalibrationSquare;
            profile.ZoomLevel = ZoomLevel;
        }

        public static double EffectiveSearchRadiusPixels(int width, int height,
            double pixelsPerMm, double searchRadiusMm) =>
            DarkCircularOpeningLocator.EffectiveAcquisitionRadiusPixels(
                width, height, pixelsPerMm, searchRadiusMm);

        private static bool InRange(double value, double min, double max) =>
            !double.IsNaN(value) && !double.IsInfinity(value) &&
            value >= min && value <= max;

        private static bool ValidNullable(double? value, double min, double max) =>
            !value.HasValue || InRange(value.Value, min, max);
    }
}
