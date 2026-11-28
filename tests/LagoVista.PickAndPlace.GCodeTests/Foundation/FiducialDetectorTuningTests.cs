using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FiducialDetectorTuningTests
    {
        private static VisionProfile Baseline() => new VisionProfile
        {
            HoughCirclesDP = 2.0,
            HoughCirclesMinDistance = 4.5,
            HoughCirclesParam1 = 30,
            HoughCirclesParam2 = 550,
            HoughCirclesMinRadius = 1.0,
            HoughCirclesMaxRadius = 10.0,
            TargetImageRadius = 12,
            ErrorToleranceMM = 0.1,
            StabilizationCount = 5
        };

        [Test]
        public void LoadDraftDoesNotMutateLiveProfile()
        {
            var profile = Baseline();
            var draft = FiducialDetectorTuning.FromProfile(profile);
            draft.EdgeThreshold = 80;
            Assert.That(profile.HoughCirclesParam1, Is.EqualTo(30));
            Assert.That(draft.Matches(profile), Is.False);
        }

        [Test]
        public void ApplyOnlyWritesActualHoughAndCenteringSettings()
        {
            var profile = Baseline();
            var draft = FiducialDetectorTuning.FromProfile(profile);
            draft.EdgeThreshold = 75;
            draft.MinimumRadiusMm = 1.4;
            draft.SearchRadiusMm = 15;
            draft.StabilizationCount = 7;
            Assert.That(draft.Validate(out var reason), Is.True, reason);
            draft.ApplyTo(profile);
            Assert.Multiple(() =>
            {
                Assert.That(profile.HoughCirclesParam1, Is.EqualTo(75));
                Assert.That(profile.HoughCirclesMinRadius, Is.EqualTo(1.4).Within(0.01));
                Assert.That(profile.TargetImageRadius, Is.EqualTo(15));
                Assert.That(profile.StabilizationCount, Is.EqualTo(7));
                Assert.That(profile.HoughCirclesParam2, Is.EqualTo(550));
                Assert.That(draft.Matches(profile), Is.True);
            });
        }

        [Test]
        public void SavingOrRevertingUsesIndependentSnapshot()
        {
            var profile = Baseline();
            var original = FiducialDetectorTuning.FromProfile(profile);
            var preview = FiducialDetectorTuning.FromProfile(profile);
            preview.AccumulatorThreshold = 420;
            preview.ApplyTo(profile);
            Assert.That(original.Matches(profile), Is.False);
            original.ApplyTo(profile);
            Assert.That(original.Matches(profile), Is.True);
        }

        [Test]
        public void RejectsInvalidMinMaxAndNaN()
        {
            var draft = FiducialDetectorTuning.FromProfile(Baseline());
            draft.MinimumRadiusMm = 11;
            Assert.That(draft.Validate(out _), Is.False);
            draft = FiducialDetectorTuning.FromProfile(Baseline());
            draft.Dp = double.NaN;
            Assert.That(draft.Validate(out _), Is.False);
        }

        [Test]
        public void RejectsZeroToleranceOrUnreasonableStabilization()
        {
            var draft = FiducialDetectorTuning.FromProfile(Baseline());
            draft.CenterToleranceMm = 0;
            Assert.That(draft.Validate(out _), Is.False);
            draft = FiducialDetectorTuning.FromProfile(Baseline());
            draft.StabilizationCount = 99;
            Assert.That(draft.Validate(out _), Is.False);
        }

        [Test]
        public void AutomaticThresholdIsStagedAndRevertsWithManualValueIntact()
        {
            var profile = Baseline();
            profile.PrimaryThreshold = 37;
            var saved = FiducialDetectorTuning.FromProfile(profile);
            var draft = FiducialDetectorTuning.FromProfile(profile);
            draft.AutoThreshold = true;
            Assert.That(profile.AutoThreshold, Is.False);
            Assert.That(draft.Matches(profile), Is.False);
            draft.ApplyTo(profile);
            Assert.That(profile.AutoThreshold, Is.True);
            Assert.That(profile.PrimaryThreshold, Is.EqualTo(37));
            Assert.That(draft.Matches(profile), Is.True);
            Assert.That(saved.Matches(profile), Is.False);
            saved.ApplyTo(profile);
            Assert.That(profile.AutoThreshold, Is.False);
            Assert.That(profile.PrimaryThreshold, Is.EqualTo(37));
        }

        [Test]
        public void ManualThresholdDraftCanBeEditedAndRestored()
        {
            var profile = Baseline();
            profile.AutoThreshold = true;
            profile.PrimaryThreshold = 37;
            var saved = FiducialDetectorTuning.FromProfile(profile);
            var draft = FiducialDetectorTuning.FromProfile(profile);
            draft.AutoThreshold = false;
            draft.PrimaryThreshold = 52;
            draft.ApplyTo(profile);
            Assert.That(profile.AutoThreshold, Is.False);
            Assert.That(profile.PrimaryThreshold, Is.EqualTo(52));
            saved.ApplyTo(profile);
            Assert.That(profile.AutoThreshold, Is.True);
            Assert.That(profile.PrimaryThreshold, Is.EqualTo(37));
            draft.PrimaryThreshold = 101;
            Assert.That(draft.Validate(out _), Is.False);
        }

        [Test]
        public void ExistingLargeAccumulatorThresholdRemainsSupported()
        {
            var draft = FiducialDetectorTuning.FromProfile(Baseline());
            Assert.That(draft.AccumulatorThreshold, Is.EqualTo(550));
            Assert.That(draft.Validate(out var reason), Is.True, reason);
        }
    }
}
