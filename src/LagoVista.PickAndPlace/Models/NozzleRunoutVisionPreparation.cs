using LagoVista.Manufacturing.Models;

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
            if (!profile.PerformShapeDetection || !profile.FindCircles)
            {
                reason = "The Nozzle profile rejected circle-detection configuration.";
                return false;
            }
            return true;
        }
    }
}
