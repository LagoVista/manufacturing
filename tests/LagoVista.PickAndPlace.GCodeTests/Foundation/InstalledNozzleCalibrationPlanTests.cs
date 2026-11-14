using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class InstalledNozzleCalibrationPlanTests
    {
        private static MachineToolHead Head(int index, string id)
        {
            var head = new MachineToolHead { Name = "Head " + index, HeadIndex = index };
            if (id != null)
                head.CurrentNozzle = EntityHeader<ToolNozzleTip>.Create(id, id, "N" + index);
            return head;
        }

        [Test]
        public void FindsInstalledNozzlesWithoutSelectingNavigationTool()
        {
            var machine = new LagoVista.Manufacturing.Models.Machine { ToolHeads = new ObservableCollection<MachineToolHead>
            {
                Head(2, null), Head(1, "left-nozzle")
            }};
            var installed = InstalledNozzleCalibrationPlan.InstalledHeads(machine);
            Assert.That(installed.Count, Is.EqualTo(1));
            Assert.That(installed[0].HeadIndex, Is.EqualTo(1));
            Assert.That(installed[0].CurrentNozzle.Text, Is.EqualTo("N1"));
        }

        [Test]
        public void DiscoversBothHeadsInStableOrder()
        {
            var machine = new LagoVista.Manufacturing.Models.Machine { ToolHeads = new ObservableCollection<MachineToolHead>
            {
                Head(2,"right-nozzle"), Head(1,"left-nozzle")
            }};
            Assert.That(InstalledNozzleCalibrationPlan.InstalledHeads(machine)
                .Select(x => x.HeadIndex), Is.EqualTo(new[]{1,2}));
        }

        [Test]
        public void PrefersActiveHeadOnlyWhenItHasAnInstalledNozzle()
        {
            var left = Head(1,"left-nozzle");
            var right = Head(2,"right-nozzle");
            var installed = new[]{left,right};
            Assert.That(InstalledNozzleCalibrationPlan.ChooseFirst(installed,right),Is.SameAs(right));
            Assert.That(InstalledNozzleCalibrationPlan.ChooseFirst(installed,null),Is.SameAs(left));
            Assert.That(InstalledNozzleCalibrationPlan.ChooseFirst(new[]{left},
                Head(2,null)),Is.SameAs(left));
        }

        [Test]
        public void NoInstalledNozzlesProducesNoCalibrationTarget()
        {
            var machine = new LagoVista.Manufacturing.Models.Machine {ToolHeads = new ObservableCollection<MachineToolHead>
            {
                Head(1,null),Head(2,null)
            }};
            Assert.That(InstalledNozzleCalibrationPlan.InstalledHeads(machine),Is.Empty);
            Assert.That(InstalledNozzleCalibrationPlan.ChooseFirst(
                InstalledNozzleCalibrationPlan.InstalledHeads(machine),null),Is.Null);
        }
    }
}
