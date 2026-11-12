using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class MVLocatedCornerTests
    {
        [Test]
        public void StableWhiteCornerUsesTheSamePixelToJogSignsAsCircleCentering()
        {
            var corner = new MVLocatedCorner { Camera = CameraTypes.Position };
            for (var i = 0; i < 7; i++)
                corner.Add(280 + (i % 2), 260 - (i % 2),
                    480, 480, pixelsPerMm: 100, toleranceMm: 0.15,
                    stabilizationCount: 5);

            Assert.That(corner.Stabilized, Is.True);
            Assert.That(corner.PositionStable, Is.True);
            Assert.That(corner.OffsetMM.X, Is.EqualTo(0.41).Within(0.02));
            Assert.That(corner.OffsetMM.Y, Is.EqualTo(-0.19).Within(0.02));
            Assert.That(corner.Summary, Does.Contain("STABLE"));
            Assert.That(corner.Summary, Does.Contain("CAMERA SCALE"));
        }

        [Test]
        public void CalibratedCameraScaleDeterminesWhiteCornerJogOffset()
        {
            var corner = new MVLocatedCorner { Camera = CameraTypes.Position };
            for (var i = 0; i < 5; i++)
                corner.Add(340, 190, 480, 480, pixelsPerMm: 50,
                    toleranceMm: 0.15, stabilizationCount: 5);

            Assert.That(corner.PositionStable, Is.True);
            Assert.That(corner.OffsetMM.X, Is.EqualTo(2.0).Within(0.001));
            Assert.That(corner.OffsetMM.Y, Is.EqualTo(1.0).Within(0.001));
            Assert.That(corner.Summary, Does.Contain("CAMERA SCALE"));
        }

        [Test]
        public void ChangedCameraScaleRequiresReacquiringStableCorner()
        {
            var corner = new MVLocatedCorner();
            for (var i = 0; i < 5; i++)
                corner.Add(340, 190, 480, 480, 50, 0.15, 5);

            Assert.That(corner.PositionStable, Is.True);

            corner.Add(340, 190, 480, 480, 100, 0.15, 5);

            Assert.That(corner.Stabilized, Is.False);
            Assert.That(corner.FoundCount, Is.EqualTo(1));
            Assert.That(corner.OffsetMM.X, Is.EqualTo(1.0).Within(0.001));
        }

        [Test]
        public void ANewCornerLocationMustClearPreviousStableHistory()
        {
            var corner = new MVLocatedCorner();
            for (var i = 0; i < 7; i++)
                corner.Add(270, 260, 480, 480, 100, 0.15, 5);
            Assert.That(corner.PositionStable, Is.True);

            corner.Add(375, 350, 480, 480, 100, 0.15, 5);
            Assert.That(corner.Stabilized, Is.False);
            Assert.That(corner.PositionStable, Is.False);
            Assert.That(corner.FoundCount, Is.EqualTo(1));
            Assert.That(corner.Summary, Does.Contain("ACQUIRING"));
        }

        [Test]
        public void UnstableCornerCannotAuthorizeJog()
        {
            var corner = new MVLocatedCorner();
            for (var i = 0; i < 7; i++)
                corner.Add(270 + (i % 2) * 20, 260, 480, 480,
                    100, 0.15, 5);
            Assert.That(corner.Stabilized, Is.True);
            Assert.That(corner.PositionStable, Is.False);
        }
    }
}
