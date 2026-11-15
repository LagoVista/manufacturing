using LagoVista.Manufacturing.Models;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Procedure-owned runtime Nozzle profile setup. Camera tuning remains
    /// whatever the machine already saved; operator need not enable detectors.
    /// </summary>
    public static class NozzleRunoutVisionPreparation
    {
        public static bool TryEnableCircleDetection(VisionProfile profile, out string reason)
        {
            reason = null;
            if (profile == null || profile.Key != VisionProfile.VisionProfile_Nozzle)
            {
                reason = "Could not select the bottom camera's Nozzle profile automatically.";
                return false;
            }
            profile.PerformShapeDetection = true;
            profile.FindCircles = true;
            // Calibration owns its illumination setup as well. Preserve
            // saved brightness/color/intensity; enable only the on/off flag.
            profile.LightOn = true;
            if (!profile.PerformShapeDetection || !profile.FindCircles)
            {
                reason = "The Nozzle profile rejected circle-detection configuration.";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Only lock a fresh, proven optical opening. This is a transient
        /// per-head runout setting; the procedure must not silently persist
        /// one head's diameter for another head.
        /// </summary>
        public static bool TryLockAdaptiveOpening(VisionProfile profile,
            double cameraPixelsPerMm, DarkCircularOpeningLocator.TuneResult tuned,
            out string reason)
        {
            reason = null;
            if (profile == null || profile.Key != VisionProfile.VisionProfile_Nozzle ||
                !Finite(cameraPixelsPerMm) || cameraPixelsPerMm <= 0 ||
                tuned == null || !Finite(tuned.DiameterMm) ||
                tuned.DiameterMm < DarkCircularOpeningLocator.MinimumOpeningDiameterMm ||
                tuned.DiameterMm > DarkCircularOpeningLocator.MaximumOpeningDiameterMm ||
                !Finite(tuned.MedianContrast) || tuned.MedianContrast < 35 ||
                !Finite(tuned.MaxCenterDeviationMm) ||
                tuned.MaxCenterDeviationMm > 0.12 ||
                tuned.TotalFrames < 20 ||
                tuned.GoodFrames < Math.Max(18, (int)Math.Ceiling(tuned.TotalFrames * 0.8)))
            {
                reason = "The bottom-camera nozzle opening did not meet diameter, contrast, and repeatability requirements.";
                return false;
            }

            profile.ExpectedDarkOpeningDiameterMm = tuned.DiameterMm;
            profile.DarkOpeningTunedPixelsPerMM = cameraPixelsPerMm;
            profile.UseAdaptiveDarkOpening = true;
            profile.FindCircles = true;
            profile.PerformShapeDetection = true;
            return true;
        }

        /// <summary>
        /// Require every runout angle to measure the same verified optical
        /// feature at the unchanged camera scale. Does not accept a fallback
        /// Hough circle or a tracked circle from an earlier calibration.
        /// </summary>
        public static bool MatchesLockedOpening(VisionProfile profile,
            double cameraPixelsPerMm, MVLocatedCircle circle)
        {
            if (profile == null || circle == null ||
                profile.Key != VisionProfile.VisionProfile_Nozzle ||
                !profile.UseAdaptiveDarkOpening ||
                !profile.PerformShapeDetection || !profile.FindCircles ||
                circle.CameraType != CameraTypes.PartInspection ||
                !circle.Stabilized || !circle.PositionStable ||
                !Finite(cameraPixelsPerMm) || cameraPixelsPerMm <= 0 ||
                !Finite(circle.PixelsPerMM) ||
                Math.Abs(circle.PixelsPerMM - cameraPixelsPerMm) > 0.01)
                return false;

            var expected = DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                profile.ExpectedDarkOpeningDiameterMm,
                profile.DarkOpeningTunedPixelsPerMM, cameraPixelsPerMm);
            if (expected <= 0 || !Finite(circle.RadiusPixels) || circle.RadiusPixels <= 0)
                return false;

            var diameterMm = 2 * circle.RadiusPixels / cameraPixelsPerMm;
            return Math.Abs(diameterMm - expected) <= Math.Max(0.12, expected * 0.30);
        }

        private static bool Finite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
