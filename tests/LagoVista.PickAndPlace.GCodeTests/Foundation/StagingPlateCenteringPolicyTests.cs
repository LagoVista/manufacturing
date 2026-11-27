using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public sealed class StagingPlateCenteringPolicyTests
    {
        [Test]
        public void ExistingPlatesRetainConservativeDefaults()
        {
            var plate = new MachineStagingPlate();
            Assert.That(plate.MaxCenteringCorrectionPerAxisMm,
                Is.EqualTo(StagingPlateCenteringPolicy.DefaultCorrectionPerAxisMm));
            Assert.That(plate.MaximumCenteringMoves,
                Is.EqualTo(StagingPlateCenteringPolicy.DefaultMaximumCorrections));
            Assert.That(StagingPlateCenteringSettings.FromPlate(plate)
                .Matches(plate), Is.True);
        }

        [Test]
        public void DraftDoesNotChangePlateUntilExplicitApply()
        {
            var plate = new MachineStagingPlate();
            var draft = StagingPlateCenteringSettings.FromPlate(plate);
            draft.MaxCorrectionPerAxisMm = 6;
            draft.MaximumCorrectionMoves = 8;
            Assert.That(draft.Matches(plate), Is.False);
            Assert.That(plate.MaxCenteringCorrectionPerAxisMm, Is.EqualTo(3));
            Assert.That(draft.Validate(out var reason), Is.True, reason);
            draft.ApplyTo(plate);
            Assert.That(plate.MaxCenteringCorrectionPerAxisMm, Is.EqualTo(6));
            Assert.That(plate.MaximumCenteringMoves, Is.EqualTo(8));
        }

        [TestCase(2.5, -3.5, 5.0, true)]
        [TestCase(-6.0, 0, 6.0, true)]
        [TestCase(0, -6.01, 6.0, false)]
        [TestCase(6.1, 0, 6.0, false)]
        [TestCase(-3.01, 0, 3.0, false)]
        public void ConfigurableLimitIsEvaluatedOnBothAxes(
            double dx, double dy, double configured, bool allowed)
        {
            Assert.That(StagingPlateCenteringPolicy.TryAuthorizeCorrection(
                dx, dy, configured, 6, out _), Is.EqualTo(allowed));
        }

        [Test]
        public void IndependentHardCapRejectsCorruptOrHugeValues()
        {
            Assert.That(StagingPlateCenteringPolicy.TryAuthorizeCorrection(
                11, 0, 20, 6, out _), Is.False);
            Assert.That(StagingPlateCenteringPolicy.TryValidateSettings(
                double.NaN, 6, out _), Is.False);
            Assert.That(StagingPlateCenteringPolicy.TryValidateSettings(
                double.PositiveInfinity, 6, out _), Is.False);
            Assert.That(StagingPlateCenteringPolicy.TryValidateSettings(
                0, 6, out _), Is.False);
            Assert.That(StagingPlateCenteringPolicy.TryValidateSettings(
                10.01, 6, out _), Is.False);
            Assert.That(StagingPlateCenteringPolicy.TryValidateSettings(
                6, 13, out _), Is.False);
        }

        [Test]
        public void NonfiniteCircleEvidenceIsNeverMotionAuthorization()
        {
            Assert.That(StagingPlateCenteringPolicy.TryAuthorizeCorrection(
                double.NaN, 0, 6, 6, out _), Is.False);
            Assert.That(StagingPlateCenteringPolicy.TryAuthorizeCorrection(
                0, double.NegativeInfinity, 6, 6, out _), Is.False);
        }

        [Test]
        public void AnotherPlateDoesNotInheritSelectedPlateOverride()
        {
            var a = new MachineStagingPlate();
            var b = new MachineStagingPlate();
            var draft = StagingPlateCenteringSettings.FromPlate(a);
            draft.MaxCorrectionPerAxisMm = 7;
            draft.ApplyTo(a);
            Assert.That(a.MaxCenteringCorrectionPerAxisMm, Is.EqualTo(7));
            Assert.That(b.MaxCenteringCorrectionPerAxisMm, Is.EqualTo(3));
        }
    }
}
