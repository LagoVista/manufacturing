using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public sealed class NozzleRunoutSequentialHeadPlanTests
    {
        private static MachineToolHead InstalledHead(int number, string nozzleId)
        {
            return new MachineToolHead
            {
                HeadIndex = number,
                Name = "Head " + number,
                CurrentNozzle = EntityHeader<ToolNozzleTip>.Create(
                    nozzleId, nozzleId, "Nozzle " + number)
            };
        }

        private static LagoVista.Manufacturing.Models.Machine Machine(
            params MachineToolHead[] heads) =>
            new LagoVista.Manufacturing.Models.Machine
            {
                ToolHeads = new ObservableCollection<MachineToolHead>(heads)
            };

        [Test]
        public void OneStartPlansBothHeadsInPhysicalOrderEvenIfSettingsAreReversed()
        {
            var headTwo = InstalledHead(2, "right");
            var headOne = InstalledHead(1, "left");
            var machine = Machine(headTwo, headOne);

            var ok = InstalledNozzleCalibrationPlan.TryCreateSequentialPlan(
                machine, out var ordered, out var reason);

            Assert.That(ok, Is.True, reason);
            Assert.That(ordered.Select(h => h.HeadIndex), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(ordered[0], Is.SameAs(headOne));
            Assert.That(ordered[1], Is.SameAs(headTwo));
        }

        [Test]
        public void OneInstalledHeadIsTheEntirePlan()
        {
            var headTwo = InstalledHead(2, "right");
            var machine = Machine(headTwo, new MachineToolHead { HeadIndex = 1 });
            Assert.That(InstalledNozzleCalibrationPlan.TryCreateSequentialPlan(
                machine, out var ordered, out var reason), Is.True, reason);
            Assert.That(ordered.Count, Is.EqualTo(1));
            Assert.That(ordered[0], Is.SameAs(headTwo));
        }

        [Test]
        public void NoInstalledNozzlesRejectsStartWithoutArbitraryFallback()
        {
            var machine = Machine(new MachineToolHead { HeadIndex = 1 },
                                  new MachineToolHead { HeadIndex = 2 });
            Assert.That(InstalledNozzleCalibrationPlan.TryCreateSequentialPlan(
                machine, out var plan, out var reason), Is.False);
            Assert.That(plan, Is.Empty);
            Assert.That(reason, Does.Contain("No installed nozzle"));
        }

        [Test]
        public void DuplicateHeadIndexIsRejectedBeforeMotion()
        {
            var machine = Machine(InstalledHead(1, "left"),
                                  InstalledHead(1, "duplicate"));
            Assert.That(InstalledNozzleCalibrationPlan.TryCreateSequentialPlan(
                machine, out _, out var reason), Is.False);
            Assert.That(reason, Does.Contain("same HeadIndex"));
        }

        [Test]
        public void ReplacingSecondHeadNozzleMidSequenceInvalidatesOriginalPlan()
        {
            var first = InstalledHead(1, "left");
            var second = InstalledHead(2, "right");
            var machine = Machine(first, second);
            Assert.That(InstalledNozzleCalibrationPlan.MatchesPlannedInstallation(
                machine, second, "right"), Is.True);

            second.CurrentNozzle = EntityHeader<ToolNozzleTip>.Create(
                "replacement", "replacement", "Replacement");
            Assert.That(InstalledNozzleCalibrationPlan.MatchesPlannedInstallation(
                machine, second, "right"), Is.False);
        }

        [Test]
        public void ReplacingHeadObjectWithSameIndexMustInvalidateOriginalPlan()
        {
            var head = InstalledHead(2, "right");
            var machine = Machine(InstalledHead(1, "left"), head);
            machine.ToolHeads[1] = InstalledHead(2, "right");
            Assert.That(InstalledNozzleCalibrationPlan.MatchesPlannedInstallation(
                machine, head, "right"), Is.False);
        }

        [Test]
        public void CompletedHeadOneIsNotInvalidatedWhenHeadTwoIsPrepared()
        {
            var first = InstalledHead(1, "left");
            var second = InstalledHead(2, "right");
            var samples = Enumerable.Range(0, 12).Select(i => new RotationOffset
            {
                Angle = i * 30,
                Offset = new Point2D<double>(0.01, -0.02)
            }).ToList();

            first.CompleteRunoutCalibration("left", 90, samples);
            second.InvalidateRunoutCalibration();

            Assert.That(first.IsCalibrated, Is.True);
            Assert.That(first.RunoutZeroAngle, Is.EqualTo(90));
            Assert.That(first.RotationOffsets.Count, Is.EqualTo(12));
            Assert.That(second.IsCalibrated, Is.False);
        }

        [Test]
        public void EachHeadProcedureIncludesItsOwnVerifiedSafeHeightStage()
        {
            var steps = NozzleRunoutProcedureDefinition.Create(true).Steps;
            Assert.That(steps.Select(s => s.Key), Does.Contain("safe-height"));
            Assert.That(steps.Count(s => s.Key.StartsWith("angle-")), Is.EqualTo(12));
        }
    }
}
