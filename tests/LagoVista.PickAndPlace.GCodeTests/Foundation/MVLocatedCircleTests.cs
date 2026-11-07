using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Drawing;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class MVLocatedCircleTests
    {
        private static MVLocatedCircle CreateStagingHole()
        {
            return new MVLocatedCircle(CameraTypes.Position,
                new Point2D<int>(100, 100), 50, 0.1, 5, requirePositionStability: true);
        }

        [Test]
        public void ConsistentMeasurementsBecomeStableAndCentered()
        {
            var hole = CreateStagingHole();
            var positions = new[] { 100f, 101f, 99f, 100.5f, 100f };
            for (var i = 0; i < positions.Length; i++)
                hole.Add(new PointF(positions[i], 100), 65);

            Assert.That(hole.Stabilized, Is.True);
            Assert.That(hole.PositionStable, Is.True);
            Assert.That(hole.Centered, Is.True);
        }

        [Test]
        public void AlternatingHoughCandidatesMustNotCountAsCentered()
        {
            var hole = CreateStagingHole();
            for (var i = 0; i < 9; i++)
                hole.Add(new PointF(i % 2 == 0 ? 100 : 112, 100), 65);

            Assert.That(hole.Stabilized, Is.True);
            Assert.That(hole.PositionStable, Is.False);
            Assert.That(hole.Centered, Is.False);
        }

        [Test]
        public void TrackRecoversAfterAJumpLeavesItsWindow()
        {
            var hole = CreateStagingHole();
            for (var i = 0; i < 5; i++)
                hole.Add(new PointF(100, 100), 65);

            Assert.That(hole.PositionStable, Is.True);

            hole.Add(new PointF(112, 100), 65);
            Assert.That(hole.PositionStable, Is.False);

            for (var i = 0; i < 9; i++)
                hole.Add(new PointF(100, 100), 65);

            Assert.That(hole.PositionStable, Is.True);
            Assert.That(hole.Centered, Is.True);
        }

        [Test]
        public void NonStagingCircleKeepsOriginalCenteredBehavior()
        {
            var circle = new MVLocatedCircle(CameraTypes.Position,
                new Point2D<int>(100, 100), 50, 0.1, 5);
            circle.Add(new PointF(100, 100), 65);

            Assert.That(circle.Stabilized, Is.False);
            Assert.That(circle.PositionStable, Is.False);
            Assert.That(circle.Centered, Is.True);
        }

        [Test]
        public void ResetClearsPositionStability()
        {
            var hole = CreateStagingHole();
            for (var i = 0; i < 5; i++)
                hole.Add(new PointF(100, 100), 65);
            Assert.That(hole.PositionStable, Is.True);

            hole.Reset();

            Assert.That(hole.Stabilized, Is.False);
            Assert.That(hole.PositionStable, Is.False);
        }
    }
}
