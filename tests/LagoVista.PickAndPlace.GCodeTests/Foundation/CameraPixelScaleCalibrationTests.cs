using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Globalization;
using System.Threading;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class CameraPixelScaleCalibrationTests
    {
        [Test]
        public void TwoHundredPixelMeasurementProducesExpectedScale()
        {
            Assert.That(CameraPixelScaleCalibration.TryCalculate(2.1, out var scale), Is.True);
            Assert.That(scale, Is.EqualTo(95.2));
        }

        [Test]
        public void TextMeasurementAcceptsDecimalPointWithoutRewritingIntermediateInput()
        {
            // The TextBox is bound to a string. "2." remains in that string
            // until the operator types the next digit rather than becoming 2.
            var measurement = "2.";
            Assert.That(CameraPixelScaleCalibration.TryCalculateText(measurement, out _), Is.True);
            Assert.That(measurement, Is.EqualTo("2."));
            measurement += "2";
            Assert.That(CameraPixelScaleCalibration.TryCalculateText(measurement, out var result), Is.True);
            Assert.That(measurement, Is.EqualTo("2.2"));
            Assert.That(result, Is.EqualTo(90.9));
        }

        [Test]
        public void TextMeasurementSupportsLocalCommaOrInvariantDot()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                Assert.That(CameraPixelScaleCalibration.TryCalculateText("2,2", out var commaScale), Is.True);
                Assert.That(CameraPixelScaleCalibration.TryCalculateText("2.2", out var dotScale), Is.True);
                Assert.That(commaScale, Is.EqualTo(90.9));
                Assert.That(dotScale, Is.EqualTo(90.9));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Test]
        public void TextMeasurementRejectsIncompleteOrInvalidMeasurements()
        {
            Assert.That(CameraPixelScaleCalibration.TryCalculateText("", out _), Is.False);
            Assert.That(CameraPixelScaleCalibration.TryCalculateText("not a number", out _), Is.False);
            Assert.That(CameraPixelScaleCalibration.TryCalculateText("0", out _), Is.False);
        }

        [Test]
        public void NonPositiveOrNonFiniteMeasurementsAreRejected()
        {
            Assert.That(CameraPixelScaleCalibration.TryCalculate(null, out _), Is.False);
            Assert.That(CameraPixelScaleCalibration.TryCalculate(0, out _), Is.False);
            Assert.That(CameraPixelScaleCalibration.TryCalculate(-2.0, out _), Is.False);
            Assert.That(CameraPixelScaleCalibration.TryCalculate(double.NaN, out _), Is.False);
            Assert.That(CameraPixelScaleCalibration.TryCalculate(double.PositiveInfinity, out _), Is.False);
        }

        [Test]
        public void ProfileCalculationDoesNotModifyCameraBaseline()
        {
            var camera = new MachineCamera { PixelsPerMM = 50 };
            var profile = new VisionProfile();
            camera.CurrentVisionProfile = profile;

            CameraPixelScaleCalibration.ApplyToProfile(profile, 95.2);

            Assert.That(profile.PixelsPerMM, Is.EqualTo(95.2));
            Assert.That(camera.PixelsPerMM, Is.EqualTo(50));
            Assert.That(camera.EffectivePixelsPerMM, Is.EqualTo(95.2));

            profile.PixelsPerMM = null;
            Assert.That(camera.EffectivePixelsPerMM, Is.EqualTo(50));
        }

        [Test]
        public void CameraCalibrationRetainsProfilesWhenNotClearing()
        {
            var camera = new MachineCamera { PixelsPerMM = 50 };
            var feederOrigin = new VisionProfile { PixelsPerMM = 50 };
            var stagingPlate = new VisionProfile { PixelsPerMM = 95.2 };
            camera.VisionProfiles.Add(feederOrigin);
            camera.VisionProfiles.Add(stagingPlate);
            camera.CurrentVisionProfile = feederOrigin;

            CameraPixelScaleCalibration.ApplyToCamera(camera, 90.9, false);

            Assert.That(camera.PixelsPerMM, Is.EqualTo(90.9));
            Assert.That(feederOrigin.PixelsPerMM, Is.EqualTo(50));
            Assert.That(stagingPlate.PixelsPerMM, Is.EqualTo(95.2));
            Assert.That(camera.EffectivePixelsPerMM, Is.EqualTo(50));
        }

        [Test]
        public void CameraCalibrationClearsOnlySelectedCameraProfileOverrides()
        {
            var camera = new MachineCamera { PixelsPerMM = 50 };
            var feederOrigin = new VisionProfile { PixelsPerMM = 50 };
            var stagingPlate = new VisionProfile { PixelsPerMM = 95.2 };
            camera.VisionProfiles.Add(feederOrigin);
            camera.VisionProfiles.Add(stagingPlate);
            camera.CurrentVisionProfile = feederOrigin;

            var otherCamera = new MachineCamera { PixelsPerMM = 50 };
            var otherProfile = new VisionProfile { PixelsPerMM = 13.3 };
            otherCamera.VisionProfiles.Add(otherProfile);

            CameraPixelScaleCalibration.ApplyToCamera(camera, 90.9, true);

            Assert.That(camera.PixelsPerMM, Is.EqualTo(90.9));
            Assert.That(feederOrigin.PixelsPerMM, Is.Null);
            Assert.That(stagingPlate.PixelsPerMM, Is.Null);
            Assert.That(camera.EffectivePixelsPerMM, Is.EqualTo(90.9));
            Assert.That(otherCamera.PixelsPerMM, Is.EqualTo(50));
            Assert.That(otherProfile.PixelsPerMM, Is.EqualTo(13.3));
        }
    }
}
