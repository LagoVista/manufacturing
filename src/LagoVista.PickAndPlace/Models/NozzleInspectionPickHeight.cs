using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Translate the installed head's configured PickHeight into the common
    /// Marlin Z coordinate used by the existing manual "Move to pick height"
    /// command. The right head is mirrored about the configured safe height.
    /// This calculation never sends movement commands.
    /// </summary>
    public static class NozzleInspectionPickHeight
    {
        public static bool TryResolveCommonZ(int headIndex, double pickHeight,
            double safeMoveHeight, out double commonZ, out string reason)
        {
            commonZ = 0;
            reason = null;
            if (headIndex != 1 && headIndex != 2)
            {
                reason = "Select Head 1 or Head 2 before moving to inspection pick height.";
                return false;
            }
            if (!Finite(pickHeight) || !Finite(safeMoveHeight) ||
                pickHeight < 0 || safeMoveHeight < 0)
            {
                reason = "Configured pick height or safe move height is invalid.";
                return false;
            }

            commonZ = headIndex == 1 ? pickHeight :
                2 * safeMoveHeight - pickHeight;
            if (!Finite(commonZ) || commonZ < 0)
            {
                reason = "Selected head Pick Height maps to an invalid common-Z position.";
                return false;
            }

            return true;
        }

        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
}
