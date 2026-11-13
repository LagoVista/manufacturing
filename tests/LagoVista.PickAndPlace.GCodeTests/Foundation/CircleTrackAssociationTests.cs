using Emgu.CV.Structure;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class CircleTrackAssociationTests
    {
        private static MVLocatedCircle ExistingTrack(float x, float y, float radius)
        {
            var track = new MVLocatedCircle(CameraTypes.Position,
                new Point2D<int>(540, 540), 90.9, 0.1, 5);
            track.Add(new PointF(x, y), radius);
            return track;
        }

        [Test]
        public void RadiusChangesDoNotCreateAnotherTrackAtTheSamePhysicalHole()
        {
            // Both radii are in the configured 0.7–1.0 mm Hough band at
            // 90.9 px/mm. Their 13-pixel difference previously split a track.
            var original = ExistingTrack(540, 540, 75);
            var tracks = new ObservableCollection<MVLocatedCircle> { original };
            var nextFrame = new CircleF(new PointF(542, 539), 88);

            Assert.That(tracks.FindPrevious(nextFrame, 10), Is.SameAs(original));
        }

        [Test]
        public void NearbyButDifferentTapeHolesRemainSeparateTracks()
        {
            var original = ExistingTrack(540, 540, 75);
            var tracks = new ObservableCollection<MVLocatedCircle> { original };

            // Four-millimeter pitch corresponds to approximately 364 pixels.
            Assert.That(tracks.FindPrevious(
                new CircleF(new PointF(540, 904), 78), 10), Is.Null);
        }

        [Test]
        public void LargeCenterJumpCannotReuseAnOldTrack()
        {
            var original = ExistingTrack(540, 540, 75);
            var tracks = new List<MVLocatedCircle> { original };

            Assert.That(tracks.FindPrevious(
                new CircleF(new PointF(551, 540), 75), 10), Is.Null);
        }
    }
}
