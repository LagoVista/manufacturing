using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Configurable per-plate calibration limits, always bounded by this
    /// independent hard ceiling. Never replaces work-area, safe-Z, homing,
    /// firmware-idle or fresh-circle evidence checks.
    /// </summary>
    public static class StagingPlateCenteringPolicy
    {
        public const double DefaultCorrectionPerAxisMm = 3.0;
        public const int DefaultMaximumCorrections = 6;
        public const double HardCorrectionPerAxisCeilingMm = 10.0;
        public const int HardMaximumCorrections = 12;

        public static bool TryValidateSettings(double maxCorrectionPerAxisMm,
            int maximumCorrectionMoves, out string reason)
        {
            if (!Finite(maxCorrectionPerAxisMm) ||
                maxCorrectionPerAxisMm < 0.1 ||
                maxCorrectionPerAxisMm > HardCorrectionPerAxisCeilingMm)
            {
                reason = "Per-axis correction limit must be between 0.1 and " +
                    HardCorrectionPerAxisCeilingMm.ToString("F1") + " mm.";
                return false;
            }
            if (maximumCorrectionMoves < 1 ||
                maximumCorrectionMoves > HardMaximumCorrections)
            {
                reason = "Maximum correction attempts must be 1-" +
                    HardMaximumCorrections + ".";
                return false;
            }

            reason = null;
            return true;
        }

        public static bool TryAuthorizeCorrection(double offsetXmm,
            double offsetYmm, double configuredPerAxisMm,
            int maximumCorrectionMoves, out string reason)
        {
            if (!TryValidateSettings(configuredPerAxisMm,
                maximumCorrectionMoves, out reason))
                return false;

            if (!Finite(offsetXmm) || !Finite(offsetYmm))
            {
                reason = "Centroid correction is not finite; no move issued.";
                return false;
            }
            if (Math.Abs(offsetXmm) > configuredPerAxisMm ||
                Math.Abs(offsetYmm) > configuredPerAxisMm)
            {
                reason = "Measured correction X=" +
                    offsetXmm.ToString("+0.000;-0.000;0.000") +
                    ", Y=" + offsetYmm.ToString("+0.000;-0.000;0.000") +
                    " mm exceeds the selected plate's configured " +
                    configuredPerAxisMm.ToString("F2") +
                    " mm per-axis correction limit. Hard maximum " +
                    HardCorrectionPerAxisCeilingMm.ToString("F1") +
                    " mm. No move issued.";
                return false;
            }
            reason = null;
            return true;
        }

        private static bool Finite(double x) =>
            !double.IsNaN(x) && !double.IsInfinity(x);
    }
}
