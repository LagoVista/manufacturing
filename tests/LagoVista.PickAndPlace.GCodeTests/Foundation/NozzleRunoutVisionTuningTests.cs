using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public sealed class NozzleRunoutVisionTuningTests
    {
        private static VisionProfile NozzleProfile() => new VisionProfile
        {
            Key = VisionProfile.VisionProfile_Nozzle,
            TargetImageRadius = 4.2,
            ErrorToleranceMM = 0.08,
            StabilizationCount = 6,
            ZoomLevel = 1,
            HoughCirclesParam2 = 550,
            UseAdaptiveDarkOpening = true,
            ExpectedDarkOpeningDiameterMm = 0.65,
            DarkOpeningTunedPixelsPerMM = 12.5
        };

        [Test]
        public void DraftLeavesProfileUnchangedUntilApply()
        {
            var p = NozzleProfile();
            var edit = NozzleRunoutVisionTuning.FromProfile(p);
            edit.SearchRadiusMm = 6;
            edit.StabilizationCount = 8;
            Assert.That(p.TargetImageRadius, Is.EqualTo(4.2));
            Assert.That(edit.Matches(p), Is.False);
            Assert.That(edit.Validate(out var reason), Is.True, reason);
            edit.ApplyTo(p);
            Assert.That(p.TargetImageRadius, Is.EqualTo(6));
            Assert.That(p.StabilizationCount, Is.EqualTo(8));
            Assert.That(NozzleRunoutVisionTuning.FromProfile(p).Matches(p), Is.True);
        }

        [Test]
        public void RuntimeOpeningLockAndIrrelevantHoughFieldsRemainUntouched()
        {
            var p = NozzleProfile();
            var edit = NozzleRunoutVisionTuning.FromProfile(p);
            edit.SearchRadiusMm = 5;
            edit.Exposure = -10;
            edit.PixelsPerMm = 12.5;
            edit.LightPower = 122;
            edit.LightRed = 220;
            edit.ShowOriginalImage = true;
            edit.ApplyTo(p);
            Assert.Multiple(() =>
            {
                Assert.That(p.HoughCirclesParam2, Is.EqualTo(550));
                Assert.That(p.UseAdaptiveDarkOpening, Is.True);
                Assert.That(p.ExpectedDarkOpeningDiameterMm, Is.EqualTo(0.65));
                Assert.That(p.DarkOpeningTunedPixelsPerMM, Is.EqualTo(12.5));
                Assert.That(p.Exposure, Is.EqualTo(-10));
                Assert.That(p.LightPower, Is.EqualTo(122));
            });
        }

        [Test]
        public void AdaptiveAcquisitionSearchHasReal72PixelFloor()
        {
            var radius = NozzleRunoutVisionTuning.EffectiveSearchRadiusPixels(
                320, 240, 12.5, 4.2);
            Assert.That(radius, Is.EqualTo(72));
            Assert.That(NozzleRunoutVisionTuning.EffectiveSearchRadiusPixels(
                320, 240, 12.5, 5.0), Is.EqualTo(72));
            Assert.That(NozzleRunoutVisionTuning.EffectiveSearchRadiusPixels(
                320, 240, 12.5, 7.0), Is.EqualTo(87.5));
        }

        [Test]
        public void RejectsInvalidScaleRadiusAndStabilization()
        {
            var draft = NozzleRunoutVisionTuning.FromProfile(NozzleProfile());
            draft.SearchRadiusMm = double.NaN;
            Assert.That(draft.Validate(out _), Is.False);
            draft.SearchRadiusMm = 4;
            draft.StabilizationCount = 1;
            Assert.That(draft.Validate(out _), Is.False);
            draft.StabilizationCount = 5;
            draft.PixelsPerMm = -1;
            Assert.That(draft.Validate(out _), Is.False);
        }

        [Test]
        public void OptionalCameraOverridesCanRevertToBaseline()
        {
            var profile = NozzleProfile();
            var baseline = NozzleRunoutVisionTuning.FromProfile(profile);
            var edit = NozzleRunoutVisionTuning.FromProfile(profile);
            edit.Exposure = -8;
            edit.Brightness = 5;
            edit.LightGreen = 140;
            edit.ApplyTo(profile);
            Assert.That(profile.Exposure, Is.EqualTo(-8));
            baseline.ApplyTo(profile);
            Assert.That(profile.Exposure, Is.Null);
            Assert.That(profile.LightGreen, Is.Null);
            Assert.That(baseline.Matches(profile), Is.True);
        }
    }
}
