using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public sealed class NozzleRunoutSingleHeadSelectionTests
    {
        [Test]
        public void OperatorSelectsExactlyOneInstalledHead()
        {
            var left = new MachineToolHead { HeadIndex = 1, Name = "Left" };
            var right = new MachineToolHead { HeadIndex = 2, Name = "Right" };
            var installed = new List<MachineToolHead> { left, right };

            var selected = InstalledNozzleCalibrationPlan.SelectOne(installed, right);
            Assert.That(selected.Count, Is.EqualTo(1));
            Assert.That(selected[0], Is.SameAs(right));
        }

        [Test]
        public void AStaleSelectionCannotFallBackToAnUnrelatedInstalledHead()
        {
            var installed = new List<MachineToolHead>
            {
                new MachineToolHead { HeadIndex = 1 },
                new MachineToolHead { HeadIndex = 2 }
            };
            var stale = new MachineToolHead { HeadIndex = 1 };
            Assert.That(InstalledNozzleCalibrationPlan.SelectOne(installed, stale),
                Is.Empty);
        }

        [Test]
        public void NullSelectionNeverTriggersAllHeads()
        {
            var installed = new List<MachineToolHead>
            {
                new MachineToolHead { HeadIndex = 1 },
                new MachineToolHead { HeadIndex = 2 }
            };
            Assert.That(InstalledNozzleCalibrationPlan.SelectOne(installed, null),
                Is.Empty);
        }

        [Test]
        public void EmptyMachineCannotStartRunout()
        {
            Assert.That(InstalledNozzleCalibrationPlan.SelectOne(
                new List<MachineToolHead>(), new MachineToolHead()), Is.Empty);
        }
    }
}
