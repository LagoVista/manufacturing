using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Determines whether a measured machine-fiducial scene is usable for
    /// subsequent vision calibration. The narrow Auto-Tune targets remain the
    /// preference; these limits permit repeatable high-contrast alternatives.
    /// No acceptance is based on exposure setting alone.
    /// </summary>
    public static class CameraExposureQuality
    {
        public static bool IsUsable(double dark, double white, double darkRange, double whiteRange)
        {
            if (double.IsNaN(dark) || double.IsInfinity(dark) ||
                double.IsNaN(white) || double.IsInfinity(white) ||
                double.IsNaN(darkRange) || double.IsInfinity(darkRange) ||
                double.IsNaN(whiteRange) || double.IsInfinity(whiteRange))
                return false;

            // Reject a dark/flat image, blown-out highlights and frame flicker.
            // A dark region need not be exactly 64 and a bright region need not
            // be exactly 235 to provide useful, stable fiducial contrast.
            return dark >= 0 && dark <= 108 &&
                   white >= 195 && white <= 252 &&
                   white - dark >= 125 &&
                   darkRange >= 0 && darkRange <= 10 &&
                   whiteRange >= 0 && whiteRange <= 15;
        }
    }
}
