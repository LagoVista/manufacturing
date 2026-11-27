using System;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Models
{
    // Diagnostic-only map from nominal plate mm to controller coordinates.
    public sealed class StagingPlateGeometry
    {
        private Point2D<double> _p, _q;
        private double _a, _b, _c, _d;
        private double Det => _a * _d - _b * _c;
        // Physical scale and axis angle use inverse columns, assuming a square plate.
        public double PhysicalXScale => Math.Sqrt(_d * _d + _c * _c) / Det;
        public double PhysicalYScale => Math.Sqrt(_b * _b + _a * _a) / Det;
        public double AxisAngleDegrees => Math.Acos(Math.Max(-1, Math.Min(1,
            (-_d * _b - _c * _a) /
            (Math.Sqrt(_d * _d + _c * _c) * Math.Sqrt(_b * _b + _a * _a))))) * 180 / Math.PI;
        public double PlateXDirectionDegrees => Math.Atan2(_c, _a) * 180 / Math.PI;
        public double ControllerYPerPlateX => _c;
        public double ControllerXPerPlateY => _b;
        public Point2D<double> Predict(Point2D<double> p) =>
            new Point2D<double>(_q.X + _a * (p.X - _p.X) + _b * (p.Y - _p.Y),
                _q.Y + _c * (p.X - _p.X) + _d * (p.Y - _p.Y));
        public static double Distance(Point2D<double> a, Point2D<double> b) =>
            Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        public static bool TryFit(Point2D<double>[] p, Point2D<double>[] q, out StagingPlateGeometry fit)
        {
            fit = null;
            if (p == null || q == null || p.Length != 3 || q.Length != 3) return false;
            for (var i = 0; i < 3; i++)
                if (!Finite(p[i]) || !Finite(q[i])) return false;
            var x = p[1].X-p[0].X; var y = p[1].Y-p[0].Y;
            var X = p[2].X-p[0].X; var Y = p[2].Y-p[0].Y;
            var det = x*Y-X*y;
            if (Math.Abs(det) < 1e-6 || double.IsInfinity(det)) return false;
            var u = q[1].X-q[0].X; var v = q[1].Y-q[0].Y;
            var U = q[2].X-q[0].X; var V = q[2].Y-q[0].Y;
            var f = new StagingPlateGeometry {
                _p = p[0], _q = q[0], _a = (u*Y-U*y)/det, _b = (x*U-X*u)/det,
                _c = (v*Y-V*y)/det, _d = (x*V-X*v)/det
            };
            if (double.IsNaN(f.Det) || double.IsInfinity(f.Det) || f.Det <= 1e-6) return false;
            fit = f;
            return true;
        }
        private static bool Finite(Point2D<double> p) => p != null &&
            !double.IsNaN(p.X) && !double.IsInfinity(p.X) &&
            !double.IsNaN(p.Y) && !double.IsInfinity(p.Y);
    }
}
