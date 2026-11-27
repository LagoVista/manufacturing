using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class CameraExposureQualityTests
    {
        [TestCase(64, 235, 3, 5)]
        [TestCase(35, 214, 4, 8)]
        [TestCase(83, 234, 9, 14)]
        public void AcceptsStableHighContrastScenesWithoutExactLevelMatch(
            double dark, double white, double darkRange, double whiteRange)
        {
            Assert.That(CameraExposureQuality.IsUsable(
                dark, white, darkRange, whiteRange), Is.True);
        }

        [TestCase(75, 170, 2, 3)]
        [TestCase(130, 244, 2, 3)]
        [TestCase(25, 255, 2, 3)]
        [TestCase(60, 235, 11, 5)]
        [TestCase(60, 235, 5, 16)]
        [TestCase(94, 215, 2, 3)]
        public void RejectsFlatClippedOrUnstableScenes(
            double dark, double white, double darkRange, double whiteRange)
        {
            Assert.That(CameraExposureQuality.IsUsable(
                dark, white, darkRange, whiteRange), Is.False);
        }

        [Test]
        public void RejectsInvalidMeasurement()
        {
            Assert.That(CameraExposureQuality.IsUsable(
                double.NaN, 235, 2, 3), Is.False);
            Assert.That(CameraExposureQuality.IsUsable(
                64, double.PositiveInfinity, 2, 3), Is.False);
        }
    }
}
