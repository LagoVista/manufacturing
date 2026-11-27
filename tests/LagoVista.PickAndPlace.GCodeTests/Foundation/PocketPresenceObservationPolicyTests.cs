using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class PocketPresenceObservationPolicyTests
    {
        private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 11, 13, 23, 0, 0, TimeSpan.Zero);
        private const string Camera = "position-camera";
        private const string Profile = "partinblacktape";

        private static PocketRectangleFrameObservation Frame(long id, long lastCandidate, int candidates = 0,
            int centered = 0, int stableCount = 0) => new PocketRectangleFrameObservation
            {
                FrameId = id,
                LastRectangleFrameId = lastCandidate,
                CapturedAtUtc = Start.AddMilliseconds(900),
                CameraId = Camera,
                VisionProfileKey = Profile,
                Width = 480,
                Height = 480,
                RectangleCount = candidates,
                StableCenteredCount = centered,
                StableCenteredFoundCount = stableCount
            };

        [Test]
        public void StableCenteredRectangleConfirmsPresent()
        {
            var frame = Frame(6, 6, candidates: 1, centered: 1, stableCount: 5);
            Assert.That(PocketPresenceObservationPolicy.Evaluate(frame, 0, Start,
                Start.AddSeconds(1), Camera, Profile), Is.EqualTo(PocketPresenceDecision.Present));
        }

        [Test]
        public void RepeatedValidEmptyFramesConfirmEmpty()
        {
            var frame = Frame(15, 0);
            Assert.That(PocketPresenceObservationPolicy.Evaluate(frame, 0, Start,
                Start.AddSeconds(1), Camera, Profile), Is.EqualTo(PocketPresenceDecision.Empty));
        }

        [Test]
        public void ASingleMissedRectanglePreventsEmptyClassification()
        {
            var frame = Frame(24, 11);
            Assert.That(PocketPresenceObservationPolicy.Evaluate(frame, 0, Start,
                Start.AddSeconds(1), Camera, Profile), Is.EqualTo(PocketPresenceDecision.AwaitingEvidence));
        }

        [Test]
        public void RedOrUnstableRectangleIsNotPresentOrEmpty()
        {
            var frame = Frame(25, 25, candidates: 1, centered: 0);
            Assert.That(PocketPresenceObservationPolicy.Evaluate(frame, 0, Start,
                Start.AddSeconds(1), Camera, Profile), Is.EqualTo(PocketPresenceDecision.AwaitingEvidence));
        }

        [Test]
        public void MissingFreshFramesCannotProveEmpty()
        {
            Assert.That(PocketPresenceObservationPolicy.Evaluate(Frame(40, 0), 40, Start,
                Start.AddSeconds(1), Camera, Profile), Is.EqualTo(PocketPresenceDecision.AwaitingEvidence));
        }

        [Test]
        public void EmptyNeedsMinimumTimeAndFrameCount()
        {
            Assert.That(PocketPresenceObservationPolicy.Evaluate(Frame(10, 0), 0, Start,
                Start.AddSeconds(1), Camera, Profile), Is.EqualTo(PocketPresenceDecision.AwaitingEvidence));
            Assert.That(PocketPresenceObservationPolicy.Evaluate(Frame(15, 0), 0, Start,
                Start.AddMilliseconds(650), Camera, Profile), Is.EqualTo(PocketPresenceDecision.AwaitingEvidence));
        }

        [Test]
        public void ChangedProfileOrCameraFailsClosed()
        {
            var frame = Frame(20, 0);
            Assert.That(PocketPresenceObservationPolicy.Evaluate(frame, 0, Start,
                Start.AddSeconds(1), Camera, "partinwhitetape"), Is.EqualTo(PocketPresenceDecision.CameraUnavailable));
            Assert.That(PocketPresenceObservationPolicy.Evaluate(frame, 0, Start,
                Start.AddSeconds(1), "other-camera", Profile), Is.EqualTo(PocketPresenceDecision.CameraUnavailable));
        }

        [Test]
        public void StaleCameraTimestampFailsClosed()
        {
            var frame = Frame(30, 0);
            Assert.That(PocketPresenceObservationPolicy.Evaluate(frame, 0, Start,
                Start.AddSeconds(5), Camera, Profile), Is.EqualTo(PocketPresenceDecision.CameraUnavailable));
        }

        [Test]
        public void MultipleContoursFromOneComponentMayStillConfirmPresent()
        {
            var frame = Frame(10, 10, candidates: 2, centered: 2, stableCount: 5);
            Assert.That(PocketPresenceObservationPolicy.Evaluate(frame, 0, Start,
                Start.AddSeconds(1), Camera, Profile), Is.EqualTo(PocketPresenceDecision.Present));
        }
    }
}
