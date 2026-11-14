using LagoVista.Manufacturing.Models;
using LagoVista.Core.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederCalibrationFlowPlanTests
    {
        [Test]
        public void OneOriginAndBarcodeThenFourChecksPerInstalledTape()
        {
            var feeder = new StripFeeder
            {
                Rows = new List<StripFeederRow>
                {
                    new StripFeederRow { RowIndex = 3, Component = EntityHeader<Component>.Create("c3", "Part 3") },
                    new StripFeederRow { RowIndex = 1, Component = EntityHeader<Component>.Create("c1", "Part 1") },
                    new StripFeederRow { RowIndex = 2 }
                }
            };
            var plan = FeederCalibrationFlowPlan.Steps(feeder);
            Assert.That(plan.Count, Is.EqualTo(11));
            Assert.That(plan.Take(3).Select(x => x.StepKey), Is.EqualTo(new[]{
                "approach-origin","center-fiducial","barcode"}));
            Assert.That(plan.Skip(3).Select(x => x.RowIndex).Distinct(), Is.EqualTo(new int?[] {1,3}));
            Assert.That(plan.Select(x=>x.Number), Is.EqualTo(Enumerable.Range(1,11)));
        }

        [Test]
        public void NoComponentsProducesOnlyFeederLevelPhases()
        {
            var feeder = new StripFeeder();
            Assert.That(FeederCalibrationFlowPlan.InstalledRows(feeder), Is.Empty);
            Assert.That(FeederCalibrationFlowPlan.Steps(feeder).Count, Is.EqualTo(3));
        }
    }
}
