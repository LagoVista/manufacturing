using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class KnownLocationCylinderCenteringMathTests
    {
        private static Point2D<double> XY(double x, double y) => new Point2D<double>(x, y);

        [Test]
        public void ExistingCenteredSevenMmCylinderNeedsNoJog()
        {
            Assert.That(KnownLocationCylinderCenteringMath.TryPlan(
                XY(119.3, 132.6), XY(119.3, 132.6),
                XY(-0.5, +0.5), 7.55 * 18, 18, true,
                XY(330, 355), out var jog, out var measured,
                out var diameter, out var reason), Is.True, reason);
            Assert.That(jog.X, Is.Zero);
            Assert.That(jog.Y, Is.Zero);
            Assert.That(measured.X, Is.EqualTo(119.3 - 0.5 / 18).Within(0.00001));
            Assert.That(measured.Y, Is.EqualTo(132.6 - 0.5 / 18).Within(0.00001));
            Assert.That(diameter, Is.EqualTo(7.55).Within(0.00001));
        }

        [Test]
        public void MovingPositionCameraJogUsesCirclePixelOffsetsAndYFlip()
        {
            Assert.That(KnownLocationCylinderCenteringMath.TryPlan(
                XY(119.3, 132.6), XY(119.3, 132.6),
                XY(+6, -3), 7 * 20, 20, true,
                XY(330, 355), out var jog, out var measured,
                out _, out var reason), Is.True, reason);
            Assert.That(jog.X, Is.EqualTo(+0.3).Within(0.00001));
            Assert.That(jog.Y, Is.EqualTo(+0.15).Within(0.00001));
            Assert.That(measured.X, Is.EqualTo(119.6).Within(0.00001));
            Assert.That(measured.Y, Is.EqualTo(132.75).Within(0.00001));
        }

        [Test]
        public void WrongCircleDiameterDoesNotAuthorizeMotion()
        {
            Assert.That(KnownLocationCylinderCenteringMath.TryPlan(
                XY(119.3, 132.6), XY(119.3, 132.6),
                XY(+6, -3), 2 * 20, 20, true, XY(330, 355),
                out var jog, out _, out _, out var reason), Is.False);
            Assert.That(jog, Is.Null);
            Assert.That(reason, Does.Contain("diameter"));
        }

        [Test]
        public void UnstableOrFarAwayCircleDoesNotAuthorizeMotion()
        {
            Assert.That(KnownLocationCylinderCenteringMath.TryPlan(
                XY(119.3, 132.6), XY(119.3, 132.6),
                XY(+6, -3), 7 * 20, 20, false, XY(330, 355),
                out _, out _, out _, out var reason), Is.False);
            Assert.That(reason, Does.Contain("stable"));

            Assert.That(KnownLocationCylinderCenteringMath.TryPlan(
                XY(119.3, 132.6), XY(119.3, 132.6),
                XY(+25, -3), 7 * 20, 20, true, XY(330, 355),
                out _, out _, out _, out reason), Is.False);
            Assert.That(reason, Does.Contain("too far"));
        }

        [Test]
        public void TravelLimitsAndBadCameraScaleRejectJog()
        {
            Assert.That(KnownLocationCylinderCenteringMath.TryPlan(
                XY(329, 130), XY(329, 130),
                XY(+8, 0), 7 * 20, 20, true,
                XY(330, 355), out _, out _, out _, out _), Is.False);
            Assert.That(KnownLocationCylinderCenteringMath.TryPlan(
                XY(119.3, 132.6), XY(119.3, 132.6),
                XY(+8, 0), 7 * 20, 0, true,
                XY(330, 355), out _, out _, out _, out _), Is.False);
        }
    }
}
