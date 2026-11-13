using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    // Pure guard for a suggested camera correction. The second Play button
    // remains the authority for applying the actual jog.
    public static class FeederAutoCornerJogMath
    {
        public const double MaxCorrectionMm = 10.0;

        public static bool TryTarget(Point2D<double> current,
            double correctionX, double correctionY, Point2D<double> workArea,
            out Point2D<double> target)
        {
            target = null;
            if (current == null || workArea == null ||
                !Finite(current.X) || !Finite(current.Y) ||
                !Finite(workArea.X) || !Finite(workArea.Y) ||
                !Finite(correctionX) || !Finite(correctionY) ||
                Math.Abs(correctionX) > MaxCorrectionMm ||
                Math.Abs(correctionY) > MaxCorrectionMm)
                return false;

            var x = current.X + correctionX;
            var y = current.Y + correctionY;
            if (!Finite(x) || !Finite(y) || x < 0 || y < 0 ||
                x > workArea.X || y > workArea.Y)
                return false;

            target = new Point2D<double>(x, y);
            return true;
        }

        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
}
