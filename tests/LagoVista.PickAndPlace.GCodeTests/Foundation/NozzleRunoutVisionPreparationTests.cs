using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class NozzleRunoutVisionPreparationTests
    {
        [Test]
        public void ConfiguresDisabledNozzleCircleDetectorWithoutOperatorSetup()
        {
            var profile = new VisionProfile
            {
                Key = VisionProfile.VisionProfile_Nozzle,
                FindCircles = false,
                PerformShapeDetection = false
            };
            Assert.That(NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                profile, out var reason), Is.True, reason);
            Assert.That(profile.FindCircles, Is.True);
            Assert.That(profile.PerformShapeDetection, Is.True);
            Assert.That(profile.LightOn, Is.True);
        }

        [Test]
        public void DoesNotModifyUnrelatedVisionProfile()
        {
            var profile = new VisionProfile
            { Key=VisionProfile.VisionProfile_Defauilt };
            Assert.That(NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                profile, out _),Is.False);
            Assert.That(profile.FindCircles,Is.False);
        }

        [Test]
        public void FailsForMissingNozzleProfile()
        {
            Assert.That(NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                null, out _),Is.False);
        }
    }
}
