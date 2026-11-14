using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class NozzleRunoutReadinessPolicyTests
    {
        private static Machine Settings(bool left, bool right) =>
            new Machine
            {
                StagingPlates = new ObservableCollection<MachineStagingPlate>
                {
                    new MachineStagingPlate{ IsCalibrated=left },
                    new MachineStagingPlate{ IsCalibrated=right }
                }
            };

        [Test]
        public void AcceptsExactlyTheTwoDocumentedCalibrationPrerequisites()
        {
            Assert.That(NozzleRunoutReadinessPolicy.TryValidate(
                Settings(true,true),true,out var reason),Is.True,reason);
        }

        [Test]
        public void RequiresMachineFiducialButNotCameraSetup()
        {
            Assert.That(NozzleRunoutReadinessPolicy.TryValidate(
                Settings(true,true),false,out var reason),Is.False);
            Assert.That(reason,Does.Contain("fiducial"));
        }

        [Test]
        public void RequiresBothCalibratedStagingPlates()
        {
            Assert.That(NozzleRunoutReadinessPolicy.TryValidate(
                Settings(true,false),true,out var reason),Is.False);
            Assert.That(reason,Does.Contain("both staging plates"));
        }

        [Test]
        public void RejectsMissingMachineConfiguration()
        {
            Assert.That(NozzleRunoutReadinessPolicy.TryValidate(
                null,true,out _),Is.False);
        }
    }
}
