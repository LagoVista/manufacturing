using LagoVista.Core.Models.Drawing;

namespace LagoVista.Manufacturing.Models
{
    /// <summary>
    /// Existing strip-feeder interpolation correction, isolated for regression testing.
    /// Values must be in the same coordinate system and units.
    /// </summary>
    public static class FeederPositionCorrection
    {
        public static Point2D<double> Apply(Point2D<double> actualDelta,
            Point2D<double> expectedDelta, double ratio, Point2D<double> position)
        {
            var errorX = expectedDelta.X - actualDelta.X;
            var errorY = expectedDelta.Y - actualDelta.Y;
            var correction = new Point2D<double>(errorX * ratio, errorY * ratio);
            return position + correction;
        }
    }
}
