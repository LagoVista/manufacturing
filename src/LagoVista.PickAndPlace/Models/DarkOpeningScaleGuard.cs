using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// A dark-opening diameter inferred from pixels is only trustworthy
    /// under the pixels/mm calibration used when it was measured. Existing
    /// profiles predate scale provenance: do not silently enforce a stale
    /// millimeter diameter after changing camera calibration.
    /// </summary>
    public static class DarkOpeningScaleGuard
    {
        public static double EffectiveExpectedDiameterMm(
            double savedDiameterMm, double? tunedPixelsPerMM, double currentPixelsPerMM)
        {
            if (!Valid(savedDiameterMm) || savedDiameterMm <= 0 ||
                !Valid(currentPixelsPerMM) || currentPixelsPerMM <= 0 ||
                !tunedPixelsPerMM.HasValue || !Valid(tunedPixelsPerMM.Value) ||
                tunedPixelsPerMM.Value <= 0)
                return 0;

            var scaleDifference = Math.Abs(tunedPixelsPerMM.Value - currentPixelsPerMM);
            // Camera calibration is stored to one decimal place, while the
            // tuning snapshot is stored to three. Ignore tiny roundoff only.
            if (scaleDifference > Math.Max(0.01, currentPixelsPerMM * 0.005))
                return 0;

            return savedDiameterMm;
        }

        private static bool Valid(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
