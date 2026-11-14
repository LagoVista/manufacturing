using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class NozzleOpticalJogMathTests
    {
        private static Point2D<double> XY(double x, double y) =>
            new Point2D<double>(x, y);

        [Test]
        public void FixedBottomCameraRequiresOppositeOfObservedOffsetOnBothAxes()
        {
            Assert.That(NozzleOpticalJogMath.TryPlan(
                XY(165.1, 128.24), XY(-0.25, -0.57), XY(22.7, 65),
                XY(330, 355), out var correction, out var reason), Is.True, reason);
            Assert.That(correction.X, Is.EqualTo(+0.25).Within(0.001));
            Assert.That(correction.Y, Is.EqualTo(+0.57).Within(0.001));
        }

        [Test]
        public void OppositeQuadrantAlsoCentersTowardCameraOrigin()
        {
            Assert.That(NozzleOpticalJogMath.TryPlan(
                XY(165.1, 128.24), XY(0.45, 0.20), XY(22.7, 65),
                XY(330, 355), out var correction, out var reason), Is.True, reason);
            Assert.That(correction.X, Is.EqualTo(-0.45).Within(0.001));
            Assert.That(correction.Y, Is.EqualTo(-0.20).Within(0.001));
        }

        [Test]
        public void RejectsLargeAutomaticCorrection()
        {
            Assert.That(NozzleOpticalJogMath.TryPlan(
                XY(165, 128), XY(-2.0, -0.5), XY(22.7, 65),
                XY(330, 355), out var jog, out var reason), Is.False);
            Assert.That(jog, Is.Null);
            Assert.That(reason, Does.Contain("too far"));
        }

        [Test]
        public void ChecksPhysicalGantryNotOnlyReportedLogicalXY()
        {
            Assert.That(NozzleOpticalJogMath.TryPlan(
                XY(301, 120), XY(-0.25, -0.57), XY(25, 65),
                XY(330, 355), out var jog, out var reason), Is.False);
            Assert.That(jog, Is.Null);
            Assert.That(reason, Does.Contain("physical XY"));
        }

        [Test]
        public void RejectsJogWhichWouldCrossPhysicalClearance()
        {
            Assert.That(NozzleOpticalJogMath.TryPlan(
                XY(6, 100), XY(1.5, 0), XY(0, 0),
                XY(330, 355), out var jog, out _), Is.False);
            Assert.That(jog, Is.Null);
        }

        [Test]
        public void RejectsNonFiniteInputsAndMissingHeadOffset()
        {
            Assert.That(NozzleOpticalJogMath.TryPlan(
                XY(100, 100), XY(double.NaN, 0), XY(22, 65),
                XY(330, 355), out _, out _), Is.False);
            Assert.That(NozzleOpticalJogMath.TryPlan(
                XY(100, 100), XY(0.2, 0.3), null,
                XY(330, 355), out _, out _), Is.False);
        }

        [Test]
        public void CenteredTargetRequiresNoXYMovement()
        {
            Assert.That(NozzleOpticalJogMath.TryPlan(
                XY(165, 128), XY(0, 0), XY(22.7, 65),
                XY(330, 355), out var jog, out var reason), Is.True, reason);
            Assert.That(jog.X, Is.EqualTo(0));
            Assert.That(jog.Y, Is.EqualTo(0));
        }
    }
}
