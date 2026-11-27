using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class DarkOpeningScaleGuardTests
    {
        [Test]
        public void ExistingNozzleProfileWithoutScaleProofReacquiresRatherThanRejects()
        {
            // Prior version persisted 0.14 mm at 50 px/mm, but did not
            // record that scale. A later 12.5 px/mm calibration must not
            // enforce a physically incorrect diameter.
            Assert.That(DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                0.14, null, 12.5), Is.Zero);
        }

        [Test]
        public void MatchingScaleKeepsVerifiedDiameterGate()
        {
            Assert.That(DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                0.56, 12.5, 12.5), Is.EqualTo(0.56));
        }

        [Test]
        public void ChangedScaleDiscardsStaleDiameterUntilReTuned()
        {
            Assert.That(DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                0.14, 50, 12.5), Is.Zero);
            Assert.That(DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                0.56, 12.5, 50), Is.Zero);
        }

        [Test]
        public void NonFiniteAndMissingInputsNeverProduceAFalseLockedDiameter()
        {
            Assert.That(DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                double.NaN, 50, 50), Is.Zero);
            Assert.That(DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                0.56, double.PositiveInfinity, 50), Is.Zero);
            Assert.That(DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                0.56, 50, 0), Is.Zero);
        }

        [Test]
        public void SmallSerializationRoundingDoesNotInvalidateAnOtherwiseCurrentTune()
        {
            Assert.That(DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                0.56, 12.50, 12.51), Is.EqualTo(0.56));
        }
    }
}
