using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class CircleAutoThresholdProfilesTests
    {
        [TestCase("macfiducial")]
        [TestCase("feederfiducial")]
        [TestCase("tapehole")]
        [TestCase("tapeholewhite")]
        [TestCase("tapeholeblack")]
        [TestCase("tapeholeclear")]
        public void AdditionalProfileIsExplicitlySupported(string key)
        {
            Assert.That(CircleAutoThresholdProfiles.IsAdditional(key), Is.True);
            Assert.That(CircleAutoThresholdProfiles.IsSupported(key), Is.True);
        }

        [TestCase("knownlocation")]
        [TestCase("brdfiducial")]
        public void AcceptedProfilesAreNotAdditional(string key)
        {
            Assert.That(CircleAutoThresholdProfiles.IsAdditional(key), Is.False);
            Assert.That(CircleAutoThresholdProfiles.IsSupported(key), Is.True);
        }

        [TestCase("stagingplatehole")]
        [TestCase("calibrationdisc")]
        [TestCase("nozzle")]
        [TestCase("nozzlecalibration")]
        [TestCase("feederbarcode")]
        [TestCase("feederorigin")]
        [TestCase("feederlandmark")]
        [TestCase("partinwhitetape")]
        [TestCase("partinblacktape")]
        [TestCase("partincleartape")]
        [TestCase("squarepart")]
        [TestCase("partinspection")]
        [TestCase("partonboard")]
        public void SpecializedAndUnauditedProfilesRemainExcluded(string key)
        {
            Assert.That(CircleAutoThresholdProfiles.IsSupported(key), Is.False);
        }

        [TestCase(30, 150, true)]
        [TestCase(90, 210, true)]
        [TestCase(150, 30, true)]
        [TestCase(210, 90, true)]
        [TestCase(80, 80, false)]
        [TestCase(80, 95, false)]
        [TestCase(0, 150, false)]
        [TestCase(150, 255, false)]
        public void SmallTargetPhotometricGate(int background, int foreground, bool expected)
        {
            var image = new byte[100, 100, 1];
            for (int y = 0; y < 100; y++)
            for (int x = 0; x < 100; x++)
                image[y, x, 0] = (byte)((x - 50) * (x - 50) + (y - 50) * (y - 50) <= 25
                    ? foreground : background);
            Assert.That(KnownLocationAutoThresholdGate.Assess(image, image,
                (background + foreground) / 2, out _, out _,
                CircleAutoThresholdProfiles.MinimumClassFraction(0.5, 10, 50)), Is.EqualTo(expected));
        }

        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(4, 3)]
        [TestCase(5, 5)]
        public void KernelNormalizationDoesNotRequireProfileMutation(int configured, int expected)
        {
            Assert.That(CircleAutoThresholdProfiles.BlurKernel(configured), Is.EqualTo(expected));
        }
    }
}
