using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class VisionProfileCatalogTests
    {
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
