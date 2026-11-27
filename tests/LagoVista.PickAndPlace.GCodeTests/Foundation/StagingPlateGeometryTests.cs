using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StagingPlateGeometryTests
    {
        private static Point2D<double> P(double x, double y) => new Point2D<double>(x,y);
        [Test]
        public void SkewAndIndependentHoldout()
        {
            Assert.That(StagingPlateGeometry.TryFit(new[] { P(0,0), P(300,150), P(300,0) },
                new[] { P(20,30), P(320,183), P(320,33) }, out var fit), Is.True);
            Assert.That(fit.ControllerYPerPlateX, Is.EqualTo(.01).Within(1e-9));
            Assert.That(fit.AxisAngleDegrees, Is.GreaterThan(90));
            Assert.That(StagingPlateGeometry.Distance(fit.Predict(P(0,150)), P(20,181)), Is.EqualTo(1).Within(1e-9));
        }
        [Test]
        public void AxisScaleAndTranslation()
        {
            Assert.That(StagingPlateGeometry.TryFit(new[] { P(0,0), P(100,0), P(0,100) },
                new[] { P(5,9), P(105.5,9), P(5,108) }, out var fit), Is.True);
            Assert.That(fit.PhysicalXScale, Is.EqualTo(100.0/100.5).Within(1e-9));
            Assert.That(fit.PhysicalYScale, Is.EqualTo(100.0/99).Within(1e-9));
            Assert.That(fit.AxisAngleDegrees, Is.EqualTo(90).Within(1e-9));
        }
        [Test]
        public void RejectCollinearAndReversed()
        {
            Assert.That(StagingPlateGeometry.TryFit(new[] { P(0,0), P(1,1), P(2,2) },
                new[] { P(0,0), P(1,1), P(2,2) }, out _), Is.False);
            Assert.That(StagingPlateGeometry.TryFit(new[] { P(0,0), P(1,0), P(0,1) },
                new[] { P(0,0), P(-1,0), P(0,1) }, out _), Is.False);
        }
    }
}
