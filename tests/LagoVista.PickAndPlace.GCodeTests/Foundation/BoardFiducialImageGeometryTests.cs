using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class BoardFiducialImageGeometryTests
    {
        [TestCase(340, 292, 145)]
        [TestCase(292, 340, 145)]
        [TestCase(640, 480, 239)]
        public void BroadProfileUsesAvailableFrameInsteadOfFailing(int width, int height, int expected)
        {
            Assert.That(BoardFiducialImageGeometry.TrySamplingRadius(width, height, 100, 3, 2, out var radius), Is.True);
            Assert.That(radius, Is.EqualTo(expected));
        }

        [Test]
        public void SmallerSearchUsesLocalRegionAndInvalidScaleFails()
        {
            Assert.That(BoardFiducialImageGeometry.TrySamplingRadius(640, 480, 20, 2, 1, out var radius), Is.True);
            Assert.That(radius, Is.EqualTo(60));
            Assert.That(BoardFiducialImageGeometry.TrySamplingRadius(640, 480, 0, 2, 1, out _), Is.False);
        }

        [Test]
        public void SearchLimitsCenterButCannotAcceptClippedTarget()
        {
            Assert.That(BoardFiducialImageGeometry.IsCandidate(156, 124, 62, 340, 292, 155, 164, 60), Is.True);
            Assert.That(BoardFiducialImageGeometry.IsCandidate(156, 124, 62, 340, 292, 155, 164, 30), Is.False);
            Assert.That(BoardFiducialImageGeometry.IsCandidate(25, 124, 62, 340, 292, 155, 164, 150), Is.False);
        }

        [Test]
        public void InnerAndOuterRingAreOneButNearbySeparatePadsAreNot()
        {
            Assert.That(BoardFiducialImageGeometry.SameRing(156, 124, 62, 157, 123, 28, 40), Is.True);
            Assert.That(BoardFiducialImageGeometry.SameRing(156, 124, 62, 172, 124, 28, 40), Is.False);
            Assert.That(BoardFiducialImageGeometry.SameRing(156, 124, 62, 156, 124, 70, 40), Is.False);
        }
    }
}
