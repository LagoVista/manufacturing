using System;
using System.Collections.Generic;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>Board-local XY to camera-navigation machine XY, in mm.
    /// Positive rotation is counterclockwise in the machine XY plane.
    /// No scaling or shear is fitted; additional targets are independent checks.</summary>
    public sealed class BoardRigidTransform
    {
        private readonly double _cos, _sin, _tx, _ty;
        public double RotationDegrees { get; }
        public double BaselineMismatchMm { get; }

        private BoardRigidTransform(double angle, double tx, double ty, double mismatch)
        {
            _cos = Math.Cos(angle);
            _sin = Math.Sin(angle);
            _tx = tx;
            _ty = ty;
            RotationDegrees = angle * 180 / Math.PI;
            BaselineMismatchMm = mismatch;
        }

        public Point2D<double> Rotate(Point2D<double> delta) =>
            new Point2D<double>(_cos * delta.X - _sin * delta.Y,
                _sin * delta.X + _cos * delta.Y);

        public Point2D<double> Apply(Point2D<double> board)
        {
            var p = Rotate(board);
            return new Point2D<double>(p.X + _tx, p.Y + _ty);
        }

        public static double Distance(Point2D<double> a, Point2D<double> b) =>
            Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

        public static bool Finite(double v) => !Double.IsNaN(v) && !Double.IsInfinity(v);
        public static bool Valid(Point2D<double> p) => p != null && Finite(p.X) && Finite(p.Y);

        public static bool InWorkArea(Point2D<double> p, Point2D<double> area) =>
            Valid(p) && Valid(area) && area.X > 0 && area.Y > 0 &&
            p.X >= 0 && p.Y >= 0 && p.X <= area.X && p.Y <= area.Y;

        // Use the farthest nominal pair for angular leverage; never use the
        // remaining fiducials to hide errors by refitting them.
        public static bool TryFit(IReadOnlyList<Point2D<double>> expected,
            IReadOnlyList<Point2D<double>> observed, double tolerance,
            out BoardRigidTransform transform, out string diagnostic)
        {
            transform = null;
            diagnostic = null;
            if (expected == null || observed == null || expected.Count < 2 ||
                expected.Count != observed.Count || !Finite(tolerance) || tolerance <= 0)
            {
                diagnostic = "At least two matched fiducials and a positive centering tolerance are required.";
                return false;
            }
            int first = 0, second = 1;
            double longest = 0;
            for (int i = 0; i < expected.Count; i++)
            {
                if (!Valid(expected[i]) || !Valid(observed[i]))
                {
                    diagnostic = "Invalid or missing fiducial coordinates.";
                    return false;
                }
                for (int j = 0; j < i; j++)
                {
                    var length = Distance(expected[i], expected[j]);
                    if (length <= 2 * tolerance || Distance(observed[i], observed[j]) <= 2 * tolerance)
                    {
                        diagnostic = "Duplicate or indistinguishable fiducials.";
                        return false;
                    }
                    if (length > longest) { longest = length; first = j; second = i; }
                }
            }
            // Reject ill-conditioned baselines rather than magnify the
            // configured optical uncertainty into a large angular error.
            if (longest < 10 * tolerance)
            {
                diagnostic = "Fiducial baseline is too short relative to the centering tolerance.";
                return false;
            }
            var ex = expected[second].X - expected[first].X;
            var ey = expected[second].Y - expected[first].Y;
            var ax = observed[second].X - observed[first].X;
            var ay = observed[second].Y - observed[first].Y;
            var mismatch = Distance(observed[first], observed[second]) - longest;
            // Two optical observations each have a radial tolerance. This is
            // an engineering consistency gate, not a PCB acceptance spec.
            if (Math.Abs(mismatch) > 2 * tolerance)
            {
                diagnostic = $"Baseline length mismatch {mismatch:+0.000;-0.000;0.000} mm exceeds twice the centering tolerance.";
                return false;
            }
            var angle = Math.Atan2(ex * ay - ey * ax, ex * ax + ey * ay);
            var c = Math.Cos(angle);
            var s = Math.Sin(angle);
            var mx = (expected[first].X + expected[second].X) / 2;
            var my = (expected[first].Y + expected[second].Y) / 2;
            var tx = (observed[first].X + observed[second].X) / 2 - (c * mx - s * my);
            var ty = (observed[first].Y + observed[second].Y) / 2 - (s * mx + c * my);
            var candidate = new BoardRigidTransform(angle, tx, ty, mismatch);
            var details = new List<string>();
            var consistent = true;
            for (int i = 0; i < expected.Count; i++)
            {
                var predicted = candidate.Apply(expected[i]);
                var dx = observed[i].X - predicted.X;
                var dy = observed[i].Y - predicted.Y;
                var radial = Distance(observed[i], predicted);
                bool fit = i == first || i == second;
                details.Add($"#{i + 1} {(fit ? "fit" : "independent")} X={dx:+0.000;-0.000;0.000} Y={dy:+0.000;-0.000;0.000} R={radial:F3} mm");
                if (!Finite(radial) || radial > (fit ? tolerance : 2 * tolerance)) consistent = false;
            }
            diagnostic = $"Rotation {candidate.RotationDegrees:F4} deg; baseline {longest:F3} mm; length mismatch {mismatch:+0.000;-0.000;0.000} mm; " +
                String.Join("; ", details) + (expected.Count == 2 ? "; independent pad verification still required." : "");
            if (!consistent) return false;
            transform = candidate;
            return true;
        }
    }
}
