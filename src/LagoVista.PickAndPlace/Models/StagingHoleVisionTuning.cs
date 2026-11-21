using System;
using LagoVista.Core.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Explicit, source-audited stagingplatehole profile subset. This profile
    /// uses binary/contour centroid acquisition, NOT a Hough circle transform:
    /// Hough DP/accumulator/min-distance, lines/corners/rectangles and feeder/
    /// nozzle settings intentionally do not appear here.
    ///
    /// Capture overrides, lighting, processing and UI drawing are included
    /// because the active production pipeline actually consumes them.
    /// Draft values never mutate the profile until Apply Preview.
    /// </summary>
    public sealed class StagingHoleVisionTuning : ModelBase
    {
        private double _searchRadiusMm, _searchOffsetXmm, _searchOffsetYmm;
        private double _minimumRadiusMm, _maximumRadiusMm, _centerToleranceMm;
        private int _stabilizationCount, _blurKernelSize;
        private bool _applyMask, _invert, _applyThreshold, _useBlurredImage;
        private bool _showOriginalImage, _showCrossHairs, _showCalibrationSquare, _lightOn;
        private byte _primaryThreshold;
        private double _blurSigma, _zoomLevel;
        private double? _exposure, _brightness, _contrast, _focus, _pixelsPerMm;
        private byte? _lightPower, _lightRed, _lightGreen, _lightBlue;

        public double SearchRadiusMm { get => _searchRadiusMm; set => Set(ref _searchRadiusMm, value); }
        public double SearchOffsetXmm { get => _searchOffsetXmm; set => Set(ref _searchOffsetXmm, value); }
        public double SearchOffsetYmm { get => _searchOffsetYmm; set => Set(ref _searchOffsetYmm, value); }
        public double MinimumRadiusMm { get => _minimumRadiusMm; set => Set(ref _minimumRadiusMm, value); }
        public double MaximumRadiusMm { get => _maximumRadiusMm; set => Set(ref _maximumRadiusMm, value); }
        public double CenterToleranceMm { get => _centerToleranceMm; set => Set(ref _centerToleranceMm, value); }
        public int StabilizationCount { get => _stabilizationCount; set => Set(ref _stabilizationCount, value); }

        public bool ApplyMask { get => _applyMask; set => Set(ref _applyMask, value); }
        public bool Invert { get => _invert; set => Set(ref _invert, value); }
        public bool ApplyThreshold { get => _applyThreshold; set => Set(ref _applyThreshold, value); }
        public byte PrimaryThreshold { get => _primaryThreshold; set => Set(ref _primaryThreshold, value); }
        public bool UseBlurredImage { get => _useBlurredImage; set => Set(ref _useBlurredImage, value); }
        public int BlurKernelSize { get => _blurKernelSize; set => Set(ref _blurKernelSize, value); }
        public double BlurSigma { get => _blurSigma; set => Set(ref _blurSigma, value); }

        public double? Exposure { get => _exposure; set => Set(ref _exposure, value); }
        public double? Brightness { get => _brightness; set => Set(ref _brightness, value); }
        public double? Contrast { get => _contrast; set => Set(ref _contrast, value); }
        public double? Focus { get => _focus; set => Set(ref _focus, value); }
        public double? PixelsPerMm { get => _pixelsPerMm; set => Set(ref _pixelsPerMm, value); }

        public bool LightOn { get => _lightOn; set => Set(ref _lightOn, value); }
        public byte? LightPower { get => _lightPower; set => Set(ref _lightPower, value); }
        public byte? LightRed { get => _lightRed; set => Set(ref _lightRed, value); }
        public byte? LightGreen { get => _lightGreen; set => Set(ref _lightGreen, value); }
        public byte? LightBlue { get => _lightBlue; set => Set(ref _lightBlue, value); }

        public bool ShowOriginalImage { get => _showOriginalImage; set => Set(ref _showOriginalImage, value); }
        public bool ShowCrossHairs { get => _showCrossHairs; set => Set(ref _showCrossHairs, value); }
        public bool ShowCalibrationSquare { get => _showCalibrationSquare; set => Set(ref _showCalibrationSquare, value); }
        public double ZoomLevel { get => _zoomLevel; set => Set(ref _zoomLevel, value); }

        public static StagingHoleVisionTuning FromProfile(VisionProfile p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            return new StagingHoleVisionTuning
            {
                SearchRadiusMm = p.TargetImageRadius,
                SearchOffsetXmm = p.StagingSearchOffsetXmm,
                SearchOffsetYmm = p.StagingSearchOffsetYmm,
                MinimumRadiusMm = p.HoughCirclesMinRadius,
                MaximumRadiusMm = p.HoughCirclesMaxRadius,
                CenterToleranceMm = p.ErrorToleranceMM,
                StabilizationCount = p.StabilizationCount,
                ApplyMask = p.ApplyMask,
                Invert = p.Invert,
                ApplyThreshold = p.ApplyThreshold,
                PrimaryThreshold = p.PrimaryThreshold,
                UseBlurredImage = p.UseBlurredImage,
                BlurKernelSize = p.GausianKernellSize,
                BlurSigma = p.GaussianSigma,
                Exposure = p.Exposure, Brightness = p.Brightness,
                Contrast = p.Contrast, Focus = p.Focus,
                PixelsPerMm = p.PixelsPerMM,
                LightOn = p.LightOn, LightPower = p.LightPower,
                LightRed = p.LightRed, LightGreen = p.LightGreen,
                LightBlue = p.LightBlue,
                ShowOriginalImage = p.ShowOriginalImage,
                ShowCrossHairs = p.ShowCrossHairs,
                ShowCalibrationSquare = p.Show200PixelSquare,
                ZoomLevel = p.ZoomLevel
            };
        }

        public bool Validate(out string reason)
        {
            if (!InRange(SearchRadiusMm, 0.1, 100) ||
                !InRange(SearchOffsetXmm, -50, 50) ||
                !InRange(SearchOffsetYmm, -50, 50))
                reason = "Search radius must be 0.1-100 mm; X/Y search offsets must be within +/-50 mm.";
            else if (!InRange(MinimumRadiusMm, 0.01, 100) ||
                     !InRange(MaximumRadiusMm, 0.01, 100) ||
                     MaximumRadiusMm <= MinimumRadiusMm)
                reason = "Hole size bounds must be positive and maximum must exceed minimum.";
            else if (!InRange(CenterToleranceMm, 0.01, 1))
                reason = "Physical centering tolerance must be 0.01-1.0 mm.";
            else if (StabilizationCount < 2 || StabilizationCount > 60)
                reason = "Stable detection count must be 2-60 frames.";
            else if (ApplyThreshold && PrimaryThreshold > 100)
                reason = "Primary threshold must be a percentage 0-100.";
            else if (UseBlurredImage && (BlurKernelSize < 1 ||
                     BlurKernelSize > 31 || BlurKernelSize % 2 == 0 ||
                     !InRange(BlurSigma, 0, 50)))
                reason = "Blur kernel must be odd (1-31), sigma must be 0-50.";
            else if (!ValidNullable(Exposure, -100, 100) ||
                     !ValidNullable(Brightness, -1000, 1000) ||
                     !ValidNullable(Contrast, -1000, 1000) ||
                     !ValidNullable(Focus, -1000, 1000))
                reason = "Camera overrides must be finite and within supported tuning bounds.";
            else if (!ValidNullable(PixelsPerMm, 0.01, 2000))
                reason = "Camera scale override must be blank (inherit) or 0.01-2000 px/mm.";
            else if (!InRange(ZoomLevel, 0.1, 20))
                reason = "Overlay zoom must be within 0.1-20.";
            else
            {
                reason = null;
                return true;
            }
            return false;
        }

        public bool Matches(VisionProfile profile)
        {
            if (profile == null) return false;
            // Match values AFTER model normalization to avoid treating
            // rounded applied fields as forever unsaved.
            return SearchRadiusMm == profile.TargetImageRadius &&
                SearchOffsetXmm == profile.StagingSearchOffsetXmm &&
                SearchOffsetYmm == profile.StagingSearchOffsetYmm &&
                MinimumRadiusMm == profile.HoughCirclesMinRadius &&
                MaximumRadiusMm == profile.HoughCirclesMaxRadius &&
                CenterToleranceMm == profile.ErrorToleranceMM &&
                StabilizationCount == profile.StabilizationCount &&
                ApplyMask == profile.ApplyMask && Invert == profile.Invert &&
                ApplyThreshold == profile.ApplyThreshold &&
                PrimaryThreshold == profile.PrimaryThreshold &&
                UseBlurredImage == profile.UseBlurredImage &&
                BlurKernelSize == profile.GausianKernellSize &&
                BlurSigma == profile.GaussianSigma &&
                Exposure == profile.Exposure && Brightness == profile.Brightness &&
                Contrast == profile.Contrast && Focus == profile.Focus &&
                PixelsPerMm == profile.PixelsPerMM &&
                LightOn == profile.LightOn && LightPower == profile.LightPower &&
                LightRed == profile.LightRed && LightGreen == profile.LightGreen &&
                LightBlue == profile.LightBlue &&
                ShowOriginalImage == profile.ShowOriginalImage &&
                ShowCrossHairs == profile.ShowCrossHairs &&
                ShowCalibrationSquare == profile.Show200PixelSquare &&
                ZoomLevel == profile.ZoomLevel;
        }

        public void ApplyTo(VisionProfile p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (!Validate(out var error)) throw new InvalidOperationException(error);

            p.TargetImageRadius = SearchRadiusMm;
            p.StagingSearchOffsetXmm = SearchOffsetXmm;
            p.StagingSearchOffsetYmm = SearchOffsetYmm;
            p.HoughCirclesMinRadius = MinimumRadiusMm;
            p.HoughCirclesMaxRadius = MaximumRadiusMm;
            p.ErrorToleranceMM = CenterToleranceMm;
            p.StabilizationCount = StabilizationCount;
            p.ApplyMask = ApplyMask;
            p.Invert = Invert;
            p.ApplyThreshold = ApplyThreshold;
            p.PrimaryThreshold = PrimaryThreshold;
            p.UseBlurredImage = UseBlurredImage;
            p.GausianKernellSize = BlurKernelSize;
            p.GaussianSigma = BlurSigma;
            p.Exposure = Exposure;
            p.Brightness = Brightness;
            p.Contrast = Contrast;
            p.Focus = Focus;
            p.PixelsPerMM = PixelsPerMm;
            p.LightOn = LightOn;
            p.LightPower = LightPower;
            p.LightRed = LightRed;
            p.LightGreen = LightGreen;
            p.LightBlue = LightBlue;
            p.ShowOriginalImage = ShowOriginalImage;
            p.ShowCrossHairs = ShowCrossHairs;
            p.Show200PixelSquare = ShowCalibrationSquare;
            p.ZoomLevel = ZoomLevel;
            // ShapeDetection/FindCircles remain enforced by the procedure.
        }

        public bool FitsFrame(int width, int height, double effectivePixelsPerMm,
            out string reason)
        {
            // The existing mask is permitted to extend beyond the cropped
            // square camera frame; the relevant clipped ROI still works.
            // Only reject a search center outside the image itself.
            if (!StagingHoleSearchRegion.FitsFrame(width, height, effectivePixelsPerMm,
                SearchOffsetXmm, SearchOffsetYmm, SearchRadiusMm, false))
            {
                reason = "Search center falls outside the camera image; reduce X/Y offset.";
                return false;
            }
            reason = null;
            return true;
        }

        private static bool InRange(double v, double min, double max) =>
            !double.IsNaN(v) && !double.IsInfinity(v) && v >= min && v <= max;

        private static bool ValidNullable(double? v, double min, double max) =>
            !v.HasValue || InRange(v.Value, min, max);
    }
}
