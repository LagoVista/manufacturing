using LagoVista.Manufacturing.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StagingPlateServerContractTests
    {
        [Test]
        public void SelectedPlateLimitsRoundTripViaApiJsonModel()
        {
            var plate = new MachineStagingPlate
            {
                MaxCenteringCorrectionPerAxisMm = 8.5,
                MaximumCenteringMoves = 8
            };
            var json = JsonConvert.SerializeObject(plate);
            Assert.Multiple(() =>
            {
                Assert.That(json, Does.Contain("MaxCenteringCorrectionPerAxisMm"));
                Assert.That(json, Does.Contain("MaximumCenteringMoves"));
            });
            var reloaded = JsonConvert.DeserializeObject<MachineStagingPlate>(json);
            Assert.That(reloaded.MaxCenteringCorrectionPerAxisMm, Is.EqualTo(8.5));
            Assert.That(reloaded.MaximumCenteringMoves, Is.EqualTo(8));
            Assert.That(reloaded.TryValidateCenteringLimits(out var reason), Is.True, reason);
        }

        [Test]
        public void LegacyPlateWithoutMotionSettingsInheritsSafeDefaults()
        {
            var plate = JsonConvert.DeserializeObject<MachineStagingPlate>("{}");
            Assert.That(plate.MaxCenteringCorrectionPerAxisMm, Is.EqualTo(3.0));
            Assert.That(plate.MaximumCenteringMoves, Is.EqualTo(6));
            Assert.That(plate.TryValidateCenteringLimits(out var reason), Is.True, reason);
        }

        [Test]
        public void RejectsUnsafePersistedLimitValues()
        {
            var plate = new MachineStagingPlate();
            plate.MaxCenteringCorrectionPerAxisMm = 11;
            Assert.That(plate.TryValidateCenteringLimits(out _), Is.False);
            plate.MaxCenteringCorrectionPerAxisMm = double.NaN;
            Assert.That(plate.TryValidateCenteringLimits(out _), Is.False);
            plate.MaxCenteringCorrectionPerAxisMm = 10;
            plate.MaximumCenteringMoves = 13;
            Assert.That(plate.TryValidateCenteringLimits(out _), Is.False);
        }

        [Test]
        public void StagingProfileSearchOffsetsRoundTripViaApiJson()
        {
            var profile = new VisionProfile
            {
                StagingSearchOffsetXmm = 0.25,
                StagingSearchOffsetYmm = -0.5
            };
            var json = JsonConvert.SerializeObject(profile);
            var reloaded = JsonConvert.DeserializeObject<VisionProfile>(json);
            Assert.That(reloaded.StagingSearchOffsetXmm, Is.EqualTo(0.25));
            Assert.That(reloaded.StagingSearchOffsetYmm, Is.EqualTo(-0.5));
            var old = JsonConvert.DeserializeObject<VisionProfile>("{}");
            Assert.That(old.StagingSearchOffsetXmm, Is.Zero);
            Assert.That(old.StagingSearchOffsetYmm, Is.Zero);
        }
    }
}