using LagoVista.Manufacturing.Models;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Only operator prerequisite for nozzle runout: machine fiducial and
    /// all configured staging plates have been calibrated in this machine session.
    /// A staging plate has two measured reference holes; machines may
    /// configure one physical plate (Lumen Jr.) or two (Lumen PNP).
    /// Camera/profile setup is procedure-owned and happens afterward.
    /// </summary>
    public static class NozzleRunoutReadinessPolicy
    {
        public static bool TryValidate(LagoVista.Manufacturing.Models.Machine settings, bool machineFiducialCalibrated,
            out string reason)
        {
            reason = null;
            if (settings == null)
            {
                reason = "No machine configuration is loaded.";
                return false;
            }
            if (!machineFiducialCalibrated)
            {
                reason = "Complete machine fiducial calibration first.";
                return false;
            }
            var plates = settings.StagingPlates;
            if (plates == null || plates.Count == 0 ||
                plates.Any(plate => plate == null || !plate.IsCalibrated))
            {
                reason = "Calibrate every configured staging plate (both reference holes per plate) for this machine session first.";
                return false;
            }
            return true;
        }
    }
}
