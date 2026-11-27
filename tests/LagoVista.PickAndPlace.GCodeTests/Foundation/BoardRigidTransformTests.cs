using System;
using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class BoardRigidTransformTests
    {
        private static Point2D<double> P(double x, double y) => new Point2D<double>(x, y);

        [TestCase(0, 50, 0)]
        [TestCase(0, 0, 50)]
        [TestCase(17, 50, 0)]
        [TestCase(-22, 0, 50)]
        [TestCase(179, -50, 1)]
        [TestCase(-179, 50, -1)]
        public void RigidTransformAppliesTranslationAndRotation(double angle, double x, double y)
        {
            var a = angle * Math.PI / 180;
            Point2D<double> Actual(Point2D<double> p) => P(100 + Math.Cos(a) * p.X - Math.Sin(a) * p.Y,
                120 + Math.Sin(a) * p.X + Math.Cos(a) * p.Y);
            var expected = new[] { P(4, 7), P(4 + x, 7 + y) };
            var observed = new[] { Actual(expected[0]), Actual(expected[1]) };
            Assert.That(BoardRigidTransform.TryFit(expected, observed, 0.25, out var fit, out var reason), Is.True, reason);
            Assert.That(fit.RotationDegrees, Is.EqualTo(angle).Within(1e-8));
            var independent = P(20, 40);
            Assert.That(BoardRigidTransform.Distance(fit.Apply(independent), Actual(independent)), Is.LessThan(1e-8));
            Assert.That(reason, Does.Contain("independent pad verification still required"));
        }

        [Test]
        public void ThirdPointIsIndependentAndShearIsRejected()
        {
            var e = new[] { P(0, 0), P(100, 0), P(50, 30) };
            var a = new[] { P(10, 20), P(110, 20), P(61, 50) };
            Assert.That(BoardRigidTransform.TryFit(e, a, 0.25, out var fit, out var reason), Is.False);
            Assert.That(fit, Is.Null);
            Assert.That(reason, Does.Contain("independent"));
        }

        [Test]
        public void BaselineMismatchDoesNotGetFittedAsScale()
        {
            Assert.That(BoardRigidTransform.TryFit(new[] { P(0, 0), P(50, 0) },
                new[] { P(10, 10), P(61, 10) }, 0.25, out var fit, out var reason), Is.False);
            Assert.That(fit, Is.Null);
            Assert.That(reason, Does.Contain("Baseline length mismatch"));
        }

        [Test]
        public void MissingDuplicateShortOrNonfiniteInputIsRejected()
        {
            var e = new[] { P(0, 0), P(50, 0) };
            Assert.That(BoardRigidTransform.TryFit(e, new[] { P(10, 10), (Point2D<double>)null },
                0.25, out _, out _), Is.False);
            Assert.That(BoardRigidTransform.TryFit(e, new[] { P(10, 10), P(10, 10) },
                0.25, out _, out _), Is.False);
            Assert.That(BoardRigidTransform.TryFit(new[] { P(0, 0), P(1, 0) }, new[] { P(10, 10), P(11, 10) },
                0.25, out _, out _), Is.False);
            Assert.That(BoardRigidTransform.TryFit(e, new[] { P(10, 10), P(double.NaN, 10) },
                0.25, out _, out _), Is.False);
        }

        [Test]
        public void SmallBaselineMismatchDoesNotChangeDistances()
        {
            Assert.That(BoardRigidTransform.TryFit(new[] { P(0, 0), P(50, 0) },
                new[] { P(10, 10), P(60.2, 10) }, 0.25, out var fit, out var reason), Is.True, reason);
            Assert.That(BoardRigidTransform.Distance(fit.Apply(P(0, 0)), fit.Apply(P(50, 0))), Is.EqualTo(50).Within(1e-8));
        }
    }
}
