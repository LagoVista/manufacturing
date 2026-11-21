using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Pure mechanical guard for a supervised fine XY jog. The actual machine
    /// additionally enforces Manual mode, safe Z and acknowledged serial idle.
    /// </summary>
    public static class FiducialManualJogPolicy
    {
        public static bool TryValidate(double currentX, double currentY,
            double deltaX, double deltaY, double workAreaX, double workAreaY,
            out string reason)
        {
            if (!Finite(currentX) || !Finite(currentY) ||
                !Finite(deltaX) || !Finite(deltaY) ||
                !Finite(workAreaX) || !Finite(workAreaY) ||
                workAreaX <= 0 || workAreaY <= 0)
            {
                reason = "Invalid machine coordinate or work-area boundary.";
                return false;
            }
            if ((deltaX == 0 && deltaY == 0) ||
                (deltaX != 0 && deltaY != 0) ||
                (deltaX != 0 && !AllowedStep(deltaX)) ||
                (deltaY != 0 && !AllowedStep(deltaY)))
            {
                reason = "Jog must be one axis at a time, exactly 0.1 or 0.5 mm.";
                return false;
            }

            var x = currentX + deltaX;
            var y = currentY + deltaY;
            if (x < 0 || y < 0 || x > workAreaX || y > workAreaY)
            {
                reason = "Jog destination is outside configured XY travel.";
                return false;
            }
            reason = null;
            return true;
        }

        private static bool AllowedStep(double mm) =>
            Math.Abs(Math.Abs(mm) - 0.1) < 0.000001 ||
            Math.Abs(Math.Abs(mm) - 0.5) < 0.000001;

        private static bool Finite(double x) =>
            !double.IsInfinity(x) && !double.IsNaN(x);
    }
}
