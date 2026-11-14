using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class NozzleRunoutMotionPreflightTests
    {
        private static Point2D<double> XY(double x, double y) =>
            new Point2D<double>(x, y);

        [Test]
        public void AllowsShortWellBoundedMovement()
        {
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(100, 100), XY(22.7, 65), true, XY(145, 120),
                XY(400, 400), out var reason), Is.True, reason);
        }

        [Test]
        public void LumenJrConfiguredHeadAndCameraApproachIsInsideTheMachineEnvelope()
        {
            // Grounded machine configuration: fiducial 164.8/192, N14
            // Head 1 offset +22.7/+65, bottom camera 165.1/128.24,
            // work area 330x355 mm. Current XY is an assumed fiducial
            // starting location; runtime checks the actual M114 reading.
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(164.8,192),XY(22.7,65),true,XY(165.1,128.24),
                XY(330,355),out var reason),Is.True,reason);
        }

        [Test]
        public void RejectsCameraAtTravelMaximum()
        {
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(330, 330), XY(0, 0), false, XY(400, 400),
                XY(400, 400), out var reason), Is.False);
            Assert.That(reason, Does.Contain("travel limit"));
        }

        [Test]
        public void RejectsHeadSelectionIfCameraPlusOffsetExceedsLimit()
        {
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(120, 140), XY(22.7, 65), true, XY(390, 340),
                XY(400, 400), out var reason), Is.False);
            Assert.That(reason, Does.Contain("Selecting the tool head"));
        }

        [Test]
        public void AllowsStartingAtTopRightBecauseHeadSelectionOccursAtCamera()
        {
            // This previously failed: +22.7/+65 was applied at X316/Y354.
            // Travel to the known bottom-camera XY FIRST, then change head.
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(316.4,354.19),XY(22.7,65),true,XY(165.1,128.24),
                XY(330,355),out var reason),Is.True,reason);
        }

        [Test]
        public void RejectsCameraLocationWhoseNewHeadOffsetWouldExceedLimit()
        {
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(120, 140), XY(22.7, 65), true, XY(305, 280),
                XY(330, 355), out var reason), Is.False);
            Assert.That(reason, Does.Contain("Selecting the tool head"));
        }

        [Test]
        public void AllowsFullInBoundsTravelWithoutOperatorPositioning()
        {
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(30, 30), XY(22.7, 65), true, XY(330, 340),
                XY(400, 400), out var reason), Is.False,
                "Head offset at 330/330 exceeds bounds; choose an in-bounds camera target.");
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(30, 30), XY(22.7, 65), true, XY(255, 270),
                XY(400, 400), out reason), Is.True, reason);
        }

        [Test]
        public void RejectsUnsafeReturnFromPreviouslySelectedHead()
        {
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(18, 90), XY(22.7, 65), true, XY(165, 128),
                XY(330, 355), out var reason, XY(25, 65)), Is.False);
            Assert.That(reason, Does.Contain("previous head"));
        }

        [Test]
        public void ValidatesBothMovesDuringHeadToHeadSwitch()
        {
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(120, 120), XY(22.7, 65), true, XY(165, 128),
                XY(330, 355), out var reason, XY(28, 45)), Is.True, reason);
        }

        [Test]
        public void RejectsNonFiniteInputsAndCurrentOutsideArea()
        {
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(double.NaN, 30), XY(0, 0), false, XY(90, 90),
                XY(400, 400), out _), Is.False);
            Assert.That(NozzleRunoutMotionPreflight.TryValidate(
                XY(420, 30), XY(0, 0), false, XY(90, 90),
                XY(400, 400), out _), Is.False);
        }
    }
}
