using LagoVista.Manufacturing.Models;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Only operator prerequisite for nozzle runout: machine fiducial and
    /// both staging plates have been calibrated in this machine session.
    /// Camera/profile setup is procedure-owned and happens afterward.
    /// </summary>
    public static class NozzleRunoutReadinessPolicy
    {
        public static bool TryValidate(Machine settings, bool machineFiducialCalibrated,
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
            if (plates == null || plates.Count < 2 ||
                plates.Count(plate => plate != null && plate.IsCalibrated) < 2)
            {
                reason = "Calibrate both staging plates for this machine session first.";
                return false;
            }
            return true;
        }
    }
}
