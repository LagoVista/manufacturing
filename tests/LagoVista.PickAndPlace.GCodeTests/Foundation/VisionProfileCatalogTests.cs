using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using Newtonsoft.Json;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class VisionProfileCatalogTests
    {
        [Test]
        public void FeederLandmarkFiducialAndCustomAreIndependentSelectableProfiles()
        {
            var profiles = VisionProfile.DefaultVisionProfiles;
            var landmark = profiles.Single(p => p.Key == VisionProfile.VisionProfile_FeederLandmark);
            var fiducial = profiles.Single(p => p.Key == VisionProfile.VisionProfile_FeederFiducial);
            var custom = profiles.Single(p => p.Key == VisionProfile.VisionProfile_Custom);
            Assert.That(landmark.Id, Is.EqualTo(VisionProfile.VisionProfile_FeederLandmark));
            Assert.That(landmark.Text, Is.EqualTo(ManufacturingResources.VisionProfile_FeederLandmark));
            Assert.That(landmark.Text, Is.Not.Empty);
            Assert.That(fiducial.Id, Is.EqualTo(VisionProfile.VisionProfile_FeederFiducial));
            Assert.That(fiducial.Text, Is.EqualTo(ManufacturingResources.VisionProfile_FeederFiducial));
            Assert.That(custom.Id, Is.EqualTo(VisionProfile.VisionProfile_Custom));
            Assert.That(custom.Text, Is.EqualTo(ManufacturingResources.VisionProfile_Custom));
            Assert.That(new[] { landmark.Id, fiducial.Id, custom.Id }.Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void LandmarkDimensionsRoundTripWithoutChangingExistingProfileSettings()
        {
            // Illustrative serialization values, not measured feeder dimensions.
            var profile = new VisionProfile
            {
                Id = VisionProfile.VisionProfile_FeederLandmark,
                Key = VisionProfile.VisionProfile_FeederLandmark,
                FeederLandmarkCenterDotDiameterMm = 1.25,
                FeederLandmarkSideDotDiameterMm = 0.75,
                FeederLandmarkCenterToSideSpacingMm = 2.5,
                PrimaryThreshold = 37,
                FindCircles = true,
                PixelsPerMM = 25.0
            };
            var restored = JsonConvert.DeserializeObject<VisionProfile>(JsonConvert.SerializeObject(profile));
            Assert.That(restored.Key, Is.EqualTo(profile.Key));
            Assert.That(restored.FeederLandmarkCenterDotDiameterMm, Is.EqualTo(1.25));
            Assert.That(restored.FeederLandmarkSideDotDiameterMm, Is.EqualTo(0.75));
            Assert.That(restored.FeederLandmarkCenterToSideSpacingMm, Is.EqualTo(2.5));
            Assert.That(restored.PrimaryThreshold, Is.EqualTo(37));
            Assert.That(restored.FindCircles, Is.True);
            Assert.That(restored.PixelsPerMM, Is.EqualTo(25.0));
        }

        [Test]
        public void OlderProfilesDoNotInventLandmarkGeometry()
        {
            var old = JsonConvert.DeserializeObject<VisionProfile>(
                "{\"Key\":\"feederfiducial\",\"FindCircles\":true,\"PrimaryThreshold\":42}");
            Assert.That(old.FeederLandmarkCenterDotDiameterMm, Is.Null);
            Assert.That(old.FeederLandmarkSideDotDiameterMm, Is.Null);
            Assert.That(old.FeederLandmarkCenterToSideSpacingMm, Is.Null);
            Assert.That(old.PrimaryThreshold, Is.EqualTo(42));
            Assert.That(old.FindCircles, Is.True);
        }

        [Test]
        public void CameraLocatorIsAnIndependentSelectableProfile()
        {
            var profiles = VisionProfile.DefaultVisionProfiles;
            var locator = profiles.Single(profile =>
                profile.Key == VisionProfile.VisionProfile_CameraLocator);

            Assert.That(locator.Id, Is.EqualTo(VisionProfile.VisionProfile_CameraLocator));
            Assert.That(locator.Text, Is.EqualTo(ManufacturingResources.VisionProfile_CameraLocator));
            Assert.That(locator.Text, Is.Not.Empty);
            Assert.That(profiles.Count(profile => profile.Id == locator.Id), Is.EqualTo(1));
            Assert.That(locator.Key, Is.Not.EqualTo(VisionProfile.VisionProfile_Nozzle));
        }
    }
}
