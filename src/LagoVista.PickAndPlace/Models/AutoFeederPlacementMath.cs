using System;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;

namespace LagoVista.PickAndPlace.Models
{
    public static class AutoFeederPlacementMath
    {
        public static bool ContainsSlot(int start, int count, int slot) =>
            slot >= 1 && slot <= 254 && count > 0 && slot >= start && (long)slot - start < count;

        public static bool TryOrigin(Point2D<double> first, int start, int count, int slot, double pitch,
            FeederRotations rotation, out Point2D<double> origin)
        {
            origin = null;
            if (!ContainsSlot(start, count, slot) || !BoardRigidTransform.Valid(first) ||
                !BoardRigidTransform.Finite(pitch) || pitch <= 0 ||
                (rotation != FeederRotations.Zero && rotation != FeederRotations.OneEighty)) return false;
            var x = first.X + (slot - start) * pitch * (rotation == FeederRotations.OneEighty ? -1 : 1);
            if (!BoardRigidTransform.Finite(x)) return false;
            origin = new Point2D<double>(x, first.Y);
            return true;
        }

        public static bool TryBackfillFirstSlot(Point2D<double> oldOrigin, Point2D<double> measuredOffset,
            Point2D<double> nominalOffset, int slot, int firstSlot, Point2D<double> workArea, out Point2D<double> origin)
        {
            origin = null;
            if (slot != firstSlot || slot < 1 || slot > 254 || !BoardRigidTransform.Valid(oldOrigin) ||
                !BoardRigidTransform.Valid(measuredOffset) || !BoardRigidTransform.Valid(nominalOffset)) return false;
            var candidate = oldOrigin + measuredOffset - nominalOffset;
            if (!BoardRigidTransform.InWorkArea(candidate, workArea)) return false;
            origin = candidate;
            return true;
        }

        public static bool TryMeasure(Point2D<double> position, Point2D<double> approach,
            FiducialCircleObservation observation, double scale, double radius, double min, double max,
            Point2D<double> workArea, out Point2D<double> target, out double residual)
        {
            target = null;
            residual = double.NaN;
            if (!BoardRigidTransform.InWorkArea(position, workArea) || !BoardRigidTransform.InWorkArea(approach, workArea) ||
                observation == null || !BoardRigidTransform.Finite(scale) || scale <= 0 ||
                !BoardRigidTransform.Finite(radius) || radius <= 0 ||
                !BoardRigidTransform.Finite(observation.RadiusMm) || observation.RadiusMm < min || observation.RadiusMm > max ||
                BoardRigidTransform.Distance(position, approach) > radius) return false;
            var dx = observation.OffsetXpx / scale;
            var dy = -observation.OffsetYpx / scale;
            if (!FeederAutoCornerJogMath.TryTarget(position, dx, dy, workArea, out var candidate) ||
                BoardRigidTransform.Distance(candidate, approach) > radius) return false;
            residual = Math.Sqrt(dx * dx + dy * dy);
            target = candidate;
            return true;
        }
    }
}
