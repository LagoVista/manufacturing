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
            Assert.That(candidate.Normalize(2000), Is.EqualTo(0).Within(0.1));
            Assert.That(candidate.PartPresentThresholdPercent, Is.EqualTo(50));
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
            Assert.That(candidate.Normalize(3000), Is.EqualTo(0).Within(0.1));
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
        public void HeadTwoOpenPumpTransientRemainsRejectedAtTheExistingEightPercentThreshold()
        {
            var idle = Capture(2, 2060, 0);
            // The operator's right-head open-nozzle stage showed a roughly
            // 80k-count response but a >10k within-window fluctuation.
            // Do not relax safety limits: reject transient windows regardless
            // of how many bounded attempts the hardware controller makes.
            var unsettled = new long[] {
                -83335, -81900, -80000, -77000, -73800, -72772
            }.Select((raw, index) => new VacuumPressureSample
            {
                HeadIndex = 2,
                RawCount = raw,
                CapturedAtUtc = Now.AddMilliseconds(3000 + index * 150)
            }).ToArray();
            var spread = VacuumPressureSampling.AllowedRawSpread(
                idle, unsettled.Average(sample => (double)sample.RawCount));
            Assert.That(spread, Is.GreaterThan(6300).And.LessThan(6600));
            Assert.That(VacuumPressureSampling.TryCaptureBand(
                unsettled, 2, Now.AddSeconds(5), TimeSpan.FromSeconds(8), 6,
                spread, out _, out var reason), Is.False);
            Assert.That(reason, Does.Contain("span 10563.0"));
            Assert.That(VacuumPressureSampling.UnstableStageMessage(
                2, "pump-on / open nozzle", 5, reason),
                Does.Contain("Head 2 pump-on / open nozzle pressure remained unstable"));
            Assert.That(VacuumPressureSampling.UnstableStageMessage(
                2, "pump-on / open nozzle", 5, reason),
                Does.Not.Contain("baseline remained unstable"));
        }

        [Test]
        public void ARealLaterStableRightOpenPumpWindowPassesWithoutChangingThreshold()
        {
            var idle = Capture(2, 2060, 0);
            var settled = new long[] {
                -79000, -79100, -78800, -78900, -79040, -78930
            }.Select((raw, index) => new VacuumPressureSample
            {
                HeadIndex = 2,
                RawCount = raw,
                CapturedAtUtc = Now.AddMilliseconds(3000 + index * 150)
            }).ToArray();
            var limit = VacuumPressureSampling.AllowedRawSpread(
                idle, settled.Average(sample => (double)sample.RawCount));
            Assert.That(VacuumPressureSampling.TryCaptureBand(
                settled, 2, Now.AddSeconds(5), TimeSpan.FromSeconds(8), 6,
                limit, out var accepted, out var error), Is.True, error);
            Assert.That(accepted.HeadIndex, Is.EqualTo(2));
            Assert.That(accepted.MaximumRawCount - accepted.MinimumRawCount,
                Is.LessThan(limit));
        }

        [Test]
        public void NoiseAwareFifteenSampleBandsProduceDistinctDetectorCalibrationForBothHeads()
        {
            foreach (var head in new[] { 1, 2 })
            {
                var idle = head == 1
                    ? Enumerable.Range(0, 15).Select(i => 3500L + i % 6 * 135).ToArray()
                    : Enumerable.Range(0, 15).Select(i => 900L + i % 7 * 100).ToArray();
                var open = head == 1
                    ? Enumerable.Range(0, 15).Select(i => -152000L + i % 6 * 2000).ToArray()
                    : Enumerable.Range(0, 15).Select(i => -55000L + i % 6 * 3000).ToArray();
                var sealedReadings = head == 1
                    ? Enumerable.Range(0, 15).Select(i => -1000000L + i % 6 * 20000).ToArray()
                    : Enumerable.Range(0, 15).Select(i => -240000L + i % 5 * 5000).ToArray();

                var idleBand = RobustCapture(head, idle, 0);
                var openBand = RobustCapture(head, open, 1);
                var sealedBand = RobustCapture(head, sealedReadings, 2);
                var centerResponse = Math.Abs(openBand.MeanRawCount - idleBand.MeanRawCount);
                var legacyLimit = VacuumPressureSampling.AllowedRawSpread(idleBand, openBand.MeanRawCount);
                if (head == 2)
                    Assert.That(openBand.MaximumRawCount - openBand.MinimumRawCount,
                        Is.GreaterThan(legacyLimit), "Right-head input was rejected by the prior 8% rule");

                Assert.That(centerResponse, Is.GreaterThan(3 *
                    (openBand.NoiseRadiusRaw + idleBand.NoiseRadiusRaw)));
                Assert.That(VacuumCalibrationCandidate.TryCreate(
                    idleBand, openBand, sealedBand,
                    VacuumPressureSampling.RequiredResponse(idleBand), 12, 3,
                    out var candidate, out var error), Is.True, error);
                Assert.That(candidate.ObservedBandGapRaw, Is.GreaterThan(0));
                Assert.That(candidate.Classify((long)openBand.MeanRawCount),
                    Is.EqualTo(VacuumPickupClassification.NoPart));
                Assert.That(candidate.Classify((long)sealedBand.MeanRawCount),
                    Is.EqualTo(VacuumPickupClassification.PartPresent));
            }
        }

        private static VacuumPressureBand RobustCapture(int head, long[] rawCounts, int phase)
        {
            var samples = rawCounts.Select((raw, i) => new VacuumPressureSample
            {
                HeadIndex = head,
                RawCount = raw,
                CapturedAtUtc = Now.AddSeconds(phase * 10).AddMilliseconds(i * 100)
            }).ToArray();
            Assert.That(VacuumPressureSampling.TryCaptureRobustBand(samples, head,
                Now.AddSeconds(phase * 10 + 3), TimeSpan.FromSeconds(8), 15,
                out var band, out var error), Is.True, error);
            return band;
        }

        [Test]
        public void RobustBandTrimsIsolatedBadRawSampleWithoutHidingIt()
        {
            var values = Enumerable.Repeat(-48000L, 15).ToArray();
            values[0] = -900000; // a single wild reading remains visible in raw extrema
            var band = RobustCapture(2, values, 0);
            Assert.That(band.MinimumRawCount, Is.EqualTo(-900000));
            Assert.That(band.MeanRawCount, Is.EqualTo(-48000).Within(0.1));
            Assert.That(band.RobustMinimumRawCount, Is.EqualTo(-48000));
            Assert.That(band.NoiseRadiusRaw, Is.Zero);
        }

        [Test]
        public void NoiseAwareCandidateRejectsOverlappingOpenAndObstructedBands()
        {
            var idle = RobustCapture(2, Enumerable.Range(0, 15)
                .Select(i => 900L + i % 7 * 100).ToArray(), 0);
            var open = RobustCapture(2, Enumerable.Range(0, 15)
                .Select(i => -55000L + i % 6 * 3000).ToArray(), 1);
            var conflicting = RobustCapture(2, Enumerable.Range(0, 15)
                .Select(i => -58000L + i % 6 * 3000).ToArray(), 2);
            Assert.That(VacuumCalibrationCandidate.TryCreate(idle, open, conflicting,
                VacuumPressureSampling.RequiredResponse(idle), 12, 3,
                out _, out var reason), Is.False);
            Assert.That(reason, Does.Contain("overlap").Or.Contain("classified"));
        }

        [Test]
        public void RobustWindowStillRejectsStaleAndWrongHeadEvidence()
        {
            var samples = Enumerable.Range(0, 15)
                .Select(i => new VacuumPressureSample {
                    HeadIndex = 1, RawCount = -45000 + i,
                    CapturedAtUtc = Now.AddMilliseconds(i * 100)
                }).ToArray();
            Assert.That(VacuumPressureSampling.TryCaptureRobustBand(samples, 2,
                Now.AddSeconds(3), TimeSpan.FromSeconds(8), 15,
                out _, out _), Is.False);
            Assert.That(VacuumPressureSampling.TryCaptureRobustBand(samples, 1,
                Now.AddSeconds(20), TimeSpan.FromSeconds(8), 15,
                out _, out _), Is.False);
        }

        [Test]
        public void FiveReadingVoteRequiresAgreementAndFailsUnknownOnOppositeState()
        {
            var head = new LagoVista.Manufacturing.Models.MachineToolHead
            {
                HeadIndex = 2,
                IdleVacuum = 1200,
                NoPartPickedVacuum = -47500,
                PartPickedVacuum = -230000,
                PercentAboveNoPartPicked = 50,
                VacuumTolerancePercent = 3
            };
            Assert.That(VacuumCalibrationProfile.TryFromHead(head, out var profile), Is.True);
            Assert.That(profile.ClassifyWindow(new long[] {
                -230000, -220000, -235000, -230000, -138750
            }), Is.EqualTo(VacuumPickupClassification.PartPresent));
            Assert.That(profile.ClassifyWindow(new long[] {
                -230000, -220000, -235000, -230000, -47500
            }), Is.EqualTo(VacuumPickupClassification.Unknown));
            Assert.That(profile.ClassifyWindow(new long[] {
                -48000, -45000, -47000, -55000, -230000
            }), Is.EqualTo(VacuumPickupClassification.Unknown));
            Assert.That(profile.ClassifyWindow(new long[] { -230000, -220000 }),
                Is.EqualTo(VacuumPickupClassification.Unknown));
        }

        [Test]
        public void RawTwentyFourBitPumpResponseScalesFromMeasuredIdleNoise()
        {
            var ambient = new long[] { 4789, 4289, 4651, 5043, 4595, 4643 }
                .Select((raw, i) => new VacuumPressureSample
                {
                    HeadIndex = 1, RawCount = raw,
                    CapturedAtUtc = Now.AddMilliseconds(i * 100)
                }).ToArray();
            Assert.That(VacuumPressureSampling.TryCaptureBand(ambient, 1,
                Now.AddSeconds(2), TimeSpan.FromSeconds(8), 6,
                VacuumPressureSampling.AllowedRawSpread(null, 0),
                out var idle, out var reason), Is.True, reason);
            Assert.That(idle.MaximumRawCount - idle.MinimumRawCount, Is.EqualTo(754));

            var pump = new long[] { -184289, -186899, -176261, -182871, -187853, -184402 }
                .Select((raw, i) => new VacuumPressureSample
                {
                    HeadIndex = 1, RawCount = raw,
                    CapturedAtUtc = Now.AddMilliseconds(3000 + i * 100)
                }).ToArray();
            var mean = pump.Average(x => (double)x.RawCount);
            Assert.That(VacuumPressureSampling.TryCaptureBand(pump, 1,
                Now.AddSeconds(6), TimeSpan.FromSeconds(8), 6,
                VacuumPressureSampling.AllowedRawSpread(idle, mean),
                out var open, out reason), Is.True, reason);
            Assert.That(Math.Abs(open.MeanRawCount - idle.MeanRawCount),
                Is.GreaterThan(VacuumPressureSampling.RequiredResponse(idle)));
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
        public void StoredNegativeGoingCalibrationClassifiesActualHardwareResponses()
        {
            var head = new LagoVista.Manufacturing.Models.MachineToolHead
            {
                HeadIndex = 1,
                IdleVacuum = 4617,
                NoPartPickedVacuum = -183600,
                PartPickedVacuum = -874853,
                PercentAboveNoPartPicked = 50,
                VacuumTolerancePercent = 3
            };
            Assert.That(VacuumCalibrationProfile.TryFromHead(head, out var profile), Is.True);
            Assert.That(profile.NoPartPercent, Is.EqualTo(0));
            Assert.That(profile.ThresholdPercent, Is.EqualTo(50));
            Assert.That(profile.ThresholdRaw, Is.EqualTo((-183600.0 - 874853.0) / 2).Within(1.0));
            Assert.That(profile.Classify(-183600), Is.EqualTo(VacuumPickupClassification.NoPart));
            Assert.That(profile.Classify(-874853), Is.EqualTo(VacuumPickupClassification.PartPresent));
            Assert.That(profile.Classify(profile.ThresholdRaw), Is.EqualTo(VacuumPickupClassification.Unknown));
            Assert.That(profile.Normalize(head.NoPartPickedVacuum), Is.EqualTo(0).Within(0.01));
            Assert.That(profile.Normalize(head.PartPickedVacuum), Is.EqualTo(100).Within(0.01));
        }

        [Test]
        public void StoredPositiveGoingCalibrationUsesSameClassification()
        {
            var head = new LagoVista.Manufacturing.Models.MachineToolHead
            {
                HeadIndex = 2,
                IdleVacuum = 1000,
                NoPartPickedVacuum = 2500,
                PartPickedVacuum = 6000,
                PercentAboveNoPartPicked = 50,
                VacuumTolerancePercent = 3
            };
            Assert.That(VacuumCalibrationProfile.TryFromHead(head, out var profile), Is.True);
            Assert.That(profile.Normalize(2500), Is.EqualTo(0).Within(0.01));
            Assert.That(profile.Normalize(4250), Is.EqualTo(50).Within(0.01));
            Assert.That(profile.Classify(2500), Is.EqualTo(VacuumPickupClassification.NoPart));
            Assert.That(profile.Classify(6000), Is.EqualTo(VacuumPickupClassification.PartPresent));
            Assert.That(profile.Classify(profile.ThresholdRaw), Is.EqualTo(VacuumPickupClassification.Unknown));
        }

        [Test]
        public void MissingOrNonMonotonicStoredCalibrationDoesNotEnableNewDetector()
        {
            Assert.That(VacuumCalibrationProfile.TryFromHead(
                new LagoVista.Manufacturing.Models.MachineToolHead(), out _), Is.False);
            Assert.That(VacuumCalibrationProfile.TryFromHead(
                new LagoVista.Manufacturing.Models.MachineToolHead
                {
                    IdleVacuum = 1000, NoPartPickedVacuum = 900,
                    PartPickedVacuum = 4000, PercentAboveNoPartPicked = 50,
                    VacuumTolerancePercent = 3
                }, out _), Is.False);
        }

        [Test]
        public void ReusableDetectionObservationIncludesHeadClassificationAndDiagnostics()
        {
            var head = new LagoVista.Manufacturing.Models.MachineToolHead
            {
                HeadIndex = 1,
                IdleVacuum = 4000,
                NoPartPickedVacuum = -200000,
                PartPickedVacuum = -800000,
                PercentAboveNoPartPicked = 50,
                VacuumTolerancePercent = 3
            };
            Assert.That(VacuumCalibrationProfile.TryFromHead(head, out var profile),
                Is.True);

            var open = profile.Observe(head.HeadIndex, -200000, Now);
            Assert.That(open.HeadIndex, Is.EqualTo(1));
            Assert.That(open.RawCount, Is.EqualTo(-200000));
            Assert.That(open.CapturedAtUtc, Is.EqualTo(Now));
            Assert.That(open.NormalizedPercent, Is.EqualTo(0).Within(0.001));
            Assert.That(open.Classification, Is.EqualTo(VacuumPickupClassification.NoPart));
            Assert.That(open.ThresholdPercent, Is.EqualTo(50));
            Assert.That(open.HysteresisPercent, Is.EqualTo(3));

            var ambiguous = profile.Observe(head.HeadIndex, -500000, Now);
            Assert.That(ambiguous.NormalizedPercent, Is.EqualTo(50).Within(0.001));
            Assert.That(ambiguous.Classification, Is.EqualTo(VacuumPickupClassification.Unknown));

            var obstructed = profile.Observe(head.HeadIndex, -800000, Now);
            Assert.That(obstructed.NormalizedPercent, Is.EqualTo(100).Within(0.001));
            Assert.That(obstructed.Classification, Is.EqualTo(VacuumPickupClassification.PartPresent));
        }

        [Test]
        public void ReusableDetectionObservationSupportsPositiveGoingSensor()
        {
            var head = new LagoVista.Manufacturing.Models.MachineToolHead
            {
                HeadIndex = 2,
                IdleVacuum = 1000,
                NoPartPickedVacuum = 2000,
                PartPickedVacuum = 8000,
                PercentAboveNoPartPicked = 50,
                VacuumTolerancePercent = 3
            };
            Assert.That(VacuumCalibrationProfile.TryFromHead(head, out var profile),
                Is.True);
            var observed = profile.Observe(2, 6500, Now);
            Assert.That(observed.HeadIndex, Is.EqualTo(2));
            Assert.That(observed.NormalizedPercent, Is.EqualTo(75).Within(0.001));
            Assert.That(observed.Classification, Is.EqualTo(VacuumPickupClassification.PartPresent));
        }

        [Test]
        public void ProcedureRequiresObservedEvidenceForBothHeadsAndFiducial()
        {
            var definition = VacuumValidationProcedureDefinition.Create();
            Assert.That(definition.Steps.Count, Is.EqualTo(10));
            Assert.That(definition.Steps.Count(s => s.Key.EndsWith("-seal")), Is.EqualTo(2));
            Assert.That(definition.Steps.Single(s => s.Key == "left-seal").Instructions,
                Does.Contain("MachineFiducial"));
            Assert.That(definition.Steps.Single(s => s.Key == "left-seal").Instructions,
                Does.Contain("configured PickHeight"));
            Assert.That(definition.Steps.Single(s => s.Key == "left-seal").Instructions,
                Does.Contain("no fiducial calibration"));
            Assert.That(definition.Steps.Single(s => s.Key == "right-seal").RequiredVerification,
                Is.EqualTo(LagoVista.Manufacturing.Models.Operations.MachineOperationVerificationKind.MachineObserved));
        }
    }
}
