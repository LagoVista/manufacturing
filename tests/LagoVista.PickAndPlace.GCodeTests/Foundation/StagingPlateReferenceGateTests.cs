using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StagingPlateReferenceGateTests
    {
        private static MachineStagingPlate Plate()
        {
            return new MachineStagingPlate
            {
                HoleSpacing = 15,
                FiducialRow = EntityHeader.Create("F", "F", "F"),
                FiducialColumn = EntityHeader.Create("13", "13", "13"),
                ReferenceHoleRow1 = EntityHeader.Create("C", "C", "C"),
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow2 = EntityHeader.Create("C", "C", "C"),
                ReferenceHoleColumn2 = EntityHeader.Create("13", "13", "13")
            };
        }

        [Test]
        public void TwoValidReferenceHolesCanBeMeasuredIndependently()
        {
            var p = Plate();
            Assert.That(StagingPlateReferenceGate.ValidateConfiguration(p, out _), Is.True);
            Assert.That(StagingPlateReferenceGate.ValidateTwoHoleMeasurements(p,
                new Point2D<double>(40, 60), new Point2D<double>(115, 60), out var reason),
                Is.True, reason);
        }

        [Test]
        public void SmallPhysicalPlateRotationDoesNotInvalidateValidSpan()
        {
            var p = Plate();
            Assert.That(StagingPlateReferenceGate.ValidateTwoHoleMeasurements(p,
                new Point2D<double>(40, 60), new Point2D<double>(114.7, 64.5),
                out var reason), Is.True, reason);
        }

        [Test]
        public void MissingOrSameReferenceHolesCannotRun()
        {
            var p = Plate();
            p.ReferenceHoleColumn2 = p.ReferenceHoleColumn1;
            Assert.That(StagingPlateReferenceGate.ValidateConfiguration(p, out _), Is.False);
            p = Plate();
            p.ReferenceHoleRow2 = null;
            Assert.That(StagingPlateReferenceGate.ValidateConfiguration(p, out _), Is.False);
        }

        [Test]
        public void WrongHoleSpacingAndReversedHoleOrderFail()
        {
            var p = Plate();
            Assert.That(StagingPlateReferenceGate.ValidateTwoHoleMeasurements(p,
                new Point2D<double>(40, 60), new Point2D<double>(122, 60), out _), Is.False);
            Assert.That(StagingPlateReferenceGate.ValidateTwoHoleMeasurements(p,
                new Point2D<double>(115, 60), new Point2D<double>(40, 60), out _), Is.False);
        }

        [Test]
        public void TravelViolationExplainsExactTargetAndHowFarBeyondLimit()
        {
            var target = new Point2D<double>(302.125, 150.75);
            var area = new Point2D<double>(300, 200);
            Assert.That(StagingPlateReferenceGate.WithinWorkArea(target, area), Is.False);
            var reason = StagingPlateReferenceGate.DescribeTravelViolation(
                target, area, "Reference hole 2");
            Assert.Multiple(() =>
            {
                Assert.That(reason, Does.Contain("Reference hole 2"));
                Assert.That(reason, Does.Contain("302.125"));
                Assert.That(reason, Does.Contain("300.000"));
                Assert.That(reason, Does.Contain("2.125"));
                Assert.That(reason, Does.Contain("No move issued"));
            });
        }

        [Test]
        public void NegativeTargetMustNotBeDescribedAsFixableByIncreasingUpperLimit()
        {
            var reason = StagingPlateReferenceGate.DescribeTravelViolation(
                new Point2D<double>(-4.5, 20), new Point2D<double>(400, 300),
                "Reference hole 2");
            Assert.That(reason, Does.Contain("negative coordinate"));
            Assert.That(reason, Does.Contain("4.500"));
            Assert.That(StagingPlateReferenceGate.WithinWorkArea(
                new Point2D<double>(-4.5, 20), new Point2D<double>(400, 300)), Is.False);
        }

        [Test]
        public void ValidatesPhysicalMachineXYLimits()
        {
            var area = new Point2D<double>(300, 200);
            Assert.That(StagingPlateReferenceGate.WithinWorkArea(
                new Point2D<double>(100, 150), area), Is.True);
            Assert.That(StagingPlateReferenceGate.WithinWorkArea(
                new Point2D<double>(-0.1, 20), area), Is.False);
            Assert.That(StagingPlateReferenceGate.WithinWorkArea(
                new Point2D<double>(double.NaN, 20), area), Is.False);
        }
    }
}
