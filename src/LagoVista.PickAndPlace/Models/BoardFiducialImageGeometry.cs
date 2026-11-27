using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>Board search radius limits the target center, not its outside edge.
    /// Sampling may use the available image, but a measured circle must be fully visible.</summary>
    public static class BoardFiducialImageGeometry
    {
        private static bool Positive(double x) => BoardRigidTransform.Finite(x) && x > 0;

        public static bool TrySamplingRadius(int width, int height, double scale,
            double searchRadiusMm, double maximumRadiusMm, out int radius)
        {
            radius = 0;
            if (width < 8 || height < 8 || !Positive(scale) ||
                !Positive(searchRadiusMm) || !Positive(maximumRadiusMm))
                return false;
            var extent = (searchRadiusMm + maximumRadiusMm) * scale;
            if (!Positive(extent) || extent < 3) return false;
            // Never demand pixels outside the sensor. A broad search profile
            // can still detect a smaller target fully contained in the image.
            radius = (int)Math.Min(Math.Ceiling(extent), Math.Min(width, height) / 2 - 1);
            return radius >= 3;
        }

        public static bool IsCandidate(double x, double y, double radius, int width, int height,
            double searchX, double searchY, double searchRadius)
        {
            if (!Positive(radius) || !Positive(searchRadius) ||
                !BoardRigidTransform.Finite(x) || !BoardRigidTransform.Finite(y) ||
                !BoardRigidTransform.Finite(searchX) || !BoardRigidTransform.Finite(searchY))
                return false;
            var dx = x - searchX;
            var dy = y - searchY;
            return dx * dx + dy * dy <= searchRadius * searchRadius &&
                x - radius > 1 && y - radius > 1 &&
                x + radius < width - 2 && y + radius < height - 2;
        }

        // Hough can report both boundaries of a plated ring. They are one
        // physical feature only if tightly concentric and nested.
        public static bool SameRing(double outerX, double outerY, double outerRadius,
            double innerX, double innerY, double innerRadius, double pixelsPerMm)
        {
            if (!Positive(outerRadius) || !Positive(innerRadius) || !Positive(pixelsPerMm) ||
                innerRadius > outerRadius || !BoardRigidTransform.Finite(outerX) ||
                !BoardRigidTransform.Finite(outerY) || !BoardRigidTransform.Finite(innerX) ||
                !BoardRigidTransform.Finite(innerY)) return false;
            var dx = outerX - innerX;
            var dy = outerY - innerY;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            var centerTolerance = Math.Min(outerRadius * 0.12, Math.Max(1.0, 0.10 * pixelsPerMm));
            return distance <= centerTolerance &&
                distance + innerRadius <= outerRadius + 1.0;
        }
    }
}
