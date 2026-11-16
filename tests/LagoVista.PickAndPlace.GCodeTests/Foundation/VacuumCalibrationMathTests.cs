using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class VacuumCalibrationMathTests
    {
        private static readonly DateTimeOffset Now =
            new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero);

        private static VacuumPressureBand Capture(int head, long raw, int phase)
        {
            var samples = Enumerable.Range(0, 5).Select(i =>
                new VacuumPressureSample
                {
                    HeadIndex = head,
                    RawCount = raw + i % 2,
                    CapturedAtUtc = Now.AddMilliseconds(phase * 1000 + i * 20)
                }).ToArray();
            Assert.That(VacuumPressureSampling.TryCaptureBand(samples, head,
                Now.AddSeconds(10), TimeSpan.FromSeconds(12), 5, 2,
                out var band, out var error), Is.True, error);
            return band;
        }

        [Test]
        public void ParsesExampleM170ResponseAndRejectsMalformedData()
        {
            Assert.That(VacuumM170Response.TryParse("prs1:5138 prs2:4206",
                out var left, out var right), Is.True);
            Assert.That(left, Is.EqualTo(4206));
            Assert.That(right, Is.EqualTo(5138));
            Assert.That(VacuumM170Response.TryParse("prs1:bad prs2:4206",
                out _, out _), Is.False);
        }

        [Test]
        public void I2CResponseDecodesDecimalAndRejectsUnrelatedOrInvalidReadings()
        {
            Assert.That(VacuumI2CByteResponse.TryParse(
                "echo:i2c-reply: from:109 bytes:1 data:231", 109, out var value), Is.True);
            Assert.That(value, Is.EqualTo(231));
            Assert.That(VacuumI2CByteResponse.TryParse(
                "echo:i2c-reply: from:109 bytes:1 data:0xE7", 109, out value), Is.True);
            Assert.That(value, Is.EqualTo(231));
            Assert.That(VacuumI2CByteResponse.TryParse(
                "echo:i2c-reply: from:108 bytes:1 data:231", 109, out _), Is.False);
            Assert.That(VacuumI2CByteResponse.TryParse(
                "echo:i2c-reply: from:109 bytes:1 data:999", 109, out _), Is.False);
            Assert.That(VacuumI2CByteResponse.TryParse(
                "echo:i2c-reply: from:109 bytes:2 data:231", 109, out _), Is.False);
        }

        [Test]
        public void NormalizesRisingPressureAndSeparatesNoPartFromHeld()
        {
            Assert.That(VacuumCalibrationCandidate.TryCreate(
                Capture(1, 1000, 0), Capture(1, 2000, 1), Capture(1, 4000, 2),
                50, 10, 4, out var candidate, out var error), Is.True, error);
            Assert.That(candidate.Normalize(1000), Is.EqualTo(0).Within(0.1));
            Assert.That(candidate.Normalize(4000), Is.EqualTo(100).Within(0.1));
            Assert.That(candidate.Classify(2000), Is.EqualTo(VacuumPickupClassification.NoPart));
            Assert.That(candidate.Classify(4000), Is.EqualTo(VacuumPickupClassification.PartPresent));
            Assert.That(candidate.Classify(3000), Is.EqualTo(VacuumPickupClassification.Unknown));
            Assert.That(candidate.IsBackAtIdle(Capture(1, 1001, 3), 1), Is.True);
            Assert.That(candidate.IsBackAtIdle(Capture(1, 900, 3), 1), Is.False);
        }

        [Test]
        public void ReversedRawSensorPolarityNormalizesTheSameWay()
        {
            Assert.That(VacuumCalibrationCandidate.TryCreate(
                Capture(2, 4000, 0), Capture(2, 3000, 1), Capture(2, 1000, 2),
                50, 10, 4, out var candidate, out var error), Is.True, error);
            Assert.That(candidate.Normalize(4000), Is.EqualTo(0).Within(0.1));
            Assert.That(candidate.Normalize(1000), Is.EqualTo(100).Within(0.1));
            Assert.That(candidate.Classify(3000), Is.EqualTo(VacuumPickupClassification.NoPart));
            Assert.That(candidate.Classify(1000), Is.EqualTo(VacuumPickupClassification.PartPresent));
        }

        [Test]
        public void NonMonotonicPumpResponseDoesNotProduceCalibration()
        {
            Assert.That(VacuumCalibrationCandidate.TryCreate(
                Capture(1, 1000, 0), Capture(1, 900, 1), Capture(1, 4000, 2),
                50, 10, 4, out _, out _), Is.False);
        }

        [Test]
        public void StaleDuplicateAndCrossHeadSamplesFailClosed()
        {
            var samples = Enumerable.Range(0, 5).Select(i =>
                new VacuumPressureSample { HeadIndex = 1, RawCount = 100,
                    CapturedAtUtc = Now.AddMilliseconds(i * 20) }).ToArray();
            Assert.That(VacuumPressureSampling.TryCaptureBand(samples, 1,
                Now.AddSeconds(20), TimeSpan.FromSeconds(1), 5, 1,
                out _, out _), Is.False);
            Assert.That(VacuumPressureSampling.TryCaptureBand(samples, 2,
                Now.AddSeconds(1), TimeSpan.FromSeconds(2), 5, 1,
                out _, out _), Is.False);
            foreach (var s in samples) s.CapturedAtUtc = Now;
            Assert.That(VacuumPressureSampling.TryCaptureBand(samples, 1,
                Now.AddSeconds(1), TimeSpan.FromSeconds(2), 5, 1,
                out _, out _), Is.False);
        }

        [Test]
        public void UnstableBandExplainsMeasuredMinimumMaximumAndAllowedSpan()
        {
            var readings = new long[] { 4676, 4650, 4771, 4922, 4595, 4643 }
                .Select((raw, index) => new VacuumPressureSample
                {
                    HeadIndex = 1,
                    RawCount = raw,
                    CapturedAtUtc = Now.AddMilliseconds(index * 150)
                }).ToArray();
            var mean = readings.Average(r => (double)r.RawCount);
            var limit = Math.Max(40, Math.Abs(mean) * 0.04);
            Assert.That(VacuumPressureSampling.TryCaptureBand(
                readings, 1, Now.AddSeconds(2), TimeSpan.FromSeconds(8),
                6, limit, out _, out var failure), Is.False);
            Assert.That(failure, Does.Contain("minimum 4595"));
            Assert.That(failure, Does.Contain("maximum 4922"));
            Assert.That(failure, Does.Contain("span 327.0"));
            Assert.That(failure, Does.Contain("188.4"));
        }

        [Test]
        public void UnstableOrNonseparatingBandsCannotCalibrate()
        {
            var samples = Enumerable.Range(0, 5).Select(i =>
                new VacuumPressureSample { HeadIndex = 1, RawCount = i * 100,
                    CapturedAtUtc = Now.AddMilliseconds(i * 20) }).ToArray();
            Assert.That(VacuumPressureSampling.TryCaptureBand(samples, 1,
                Now.AddSeconds(1), TimeSpan.FromSeconds(2), 5, 10,
                out _, out _), Is.False);
            Assert.That(VacuumCalibrationCandidate.TryCreate(
                Capture(1, 1000, 0), Capture(1, 2000, 1), Capture(1, 2020, 2),
                50, 10, 4, out _, out _), Is.False);
        }

        [Test]
        public void ProcedureRequiresObservedEvidenceForBothHeadsAndFiducial()
        {
            var definition = VacuumValidationProcedureDefinition.Create();
            Assert.That(definition.Steps.Count, Is.EqualTo(10));
            Assert.That(definition.Steps.Count(s => s.Key.EndsWith("-seal")), Is.EqualTo(2));
            Assert.That(definition.Steps.Single(s => s.Key == "left-seal").Instructions,
                Does.Contain("MachineFiducial"));
            Assert.That(definition.Steps.Single(s => s.Key == "right-seal").RequiredVerification,
                Is.EqualTo(LagoVista.Manufacturing.Models.Operations.MachineOperationVerificationKind.MachineObserved));
        }
    }
}
