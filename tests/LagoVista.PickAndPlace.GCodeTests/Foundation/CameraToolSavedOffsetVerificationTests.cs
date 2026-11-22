using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    [TestFixture]
    public class CameraToolSavedOffsetVerificationTests
    {
        [Test]
        public void SavedOffsetProofHasExplicitTwoTenthsMillimeterTolerance()
        {
            Assert.That(CameraToolPickupCalibrationMath.SavedOffsetPickupToleranceMm,
                Is.EqualTo(0.20));
        }
    }
}
