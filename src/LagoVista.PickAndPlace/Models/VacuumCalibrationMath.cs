using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>One timestamped, head-specific raw count from the selected transport.</summary>
    public sealed class VacuumPressureSample
    {
        public int HeadIndex { get; set; }
        public long RawCount { get; set; }
        public DateTimeOffset CapturedAtUtc { get; set; }
    }

    /// <summary>Observed stable raw-count band, not a commanded actuator state.</summary>
    public sealed class VacuumPressureBand
    {
        public int HeadIndex { get; internal set; }
        public int SampleCount { get; internal set; }
        public double MeanRawCount { get; internal set; }
        public long MinimumRawCount { get; internal set; }
        public long MaximumRawCount { get; internal set; }
        public DateTimeOffset FirstSampleUtc { get; internal set; }
        public DateTimeOffset LastSampleUtc { get; internal set; }
    }

    /// <summary>Protocol parser for the observed "prs1:5138 prs2:4206" M170 response.</summary>
    public static class VacuumM170Response
    {
        private static readonly Regex Pattern = new Regex(
            @"^prs1:(?<first>-?[0-9]+)\s+prs2:(?<second>-?[0-9]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool TryParse(string response, out int leftRaw, out int rightRaw)
        {
            leftRaw = 0;
            rightRaw = 0;
            if (string.IsNullOrWhiteSpace(response))
                return false;
            var match = Pattern.Match(response.Trim());
            // Preserve the current machine-side channel wiring:
            // prs2 maps to left, prs1 maps to right, pending physical verification.
            return match.Success &&
                int.TryParse(match.Groups["second"].Value, out leftRaw) &&
                int.TryParse(match.Groups["first"].Value, out rightRaw);
        }
    }

    /// <summary>
    /// Decodes actual Marlin M261 replies; "data:231" represents decimal 231.
    /// Also accepts explicitly hexadecimal data, but never a mismatched address.
    /// </summary>
    public static class VacuumI2CByteResponse
    {
        private static readonly Regex Pattern = new Regex(
            @"^echo:i2c-reply:\s*from:(?<address>[0-9]+)\s+bytes:(?<count>[0-9]+)\s+data:(?<value>(?:0[xX])?[0-9A-Fa-f]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool TryParse(string response, byte expectedAddress, out byte value)
        {
            value = 0;
            if (String.IsNullOrWhiteSpace(response))
                return false;
            var match = Pattern.Match(response.Trim());
            if (!match.Success ||
                !Byte.TryParse(match.Groups["address"].Value, out var address) ||
                address != expectedAddress ||
                match.Groups["count"].Value != "1")
                return false;
            var token = match.Groups["value"].Value;
            var isHex = token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                token.Any(ch => (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f'));
            if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(2);
            return Byte.TryParse(token, isHex
                    ? System.Globalization.NumberStyles.AllowHexSpecifier
                    : System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out value);
        }
    }

    /// <summary>
    /// Reconstructs a polarity-independent classifier using existing
    /// persisted tool-head fields. An open nozzle is 0%, obstructed is 100%.
    /// PercentAboveNoPartPicked specifies the detection threshold on that
    /// open-to-sealed range (50 = midpoint). No server schema change.
    /// </summary>
    public sealed class VacuumCalibrationProfile
    {
        public double IdleRaw { get; private set; }
        public double OpenRaw { get; private set; }
        public double SealedRaw { get; private set; }
        public double NoPartPercent { get; private set; }
        public double ThresholdPercent { get; private set; }
        public double HysteresisPercent { get; private set; }

        public static bool TryFromHead(
            LagoVista.Manufacturing.Models.MachineToolHead head,
            out VacuumCalibrationProfile profile)
        {
            profile = null;
            if (head == null ||
                Double.IsNaN(head.IdleVacuum) || Double.IsInfinity(head.IdleVacuum) ||
                Double.IsNaN(head.NoPartPickedVacuum) || Double.IsInfinity(head.NoPartPickedVacuum) ||
                Double.IsNaN(head.PartPickedVacuum) || Double.IsInfinity(head.PartPickedVacuum))
                return false;

            // Pump-off idle is diagnostic evidence; the pickup scale starts
            // at the measured pump-on/open-nozzle state, not at idle.
            var idleToSealed = head.PartPickedVacuum - head.IdleVacuum;
            var openToSealed = head.PartPickedVacuum - head.NoPartPickedVacuum;
            if (Math.Abs(idleToSealed) < 10.0 || Math.Abs(openToSealed) < 10.0)
                return false;
            var openProgressFromIdle = 100.0 *
                (head.NoPartPickedVacuum - head.IdleVacuum) / idleToSealed;
            var thresholdPercent = head.PercentAboveNoPartPicked;
            var hysteresis = head.VacuumTolerancePercent;
            if (openProgressFromIdle <= 0 || openProgressFromIdle >= 88 ||
                Double.IsNaN(thresholdPercent) || Double.IsInfinity(thresholdPercent) ||
                thresholdPercent <= 0 || thresholdPercent >= 100 ||
                Double.IsNaN(hysteresis) || Double.IsInfinity(hysteresis) ||
                hysteresis < 0 || hysteresis >= 20 ||
                thresholdPercent - hysteresis <= 0 ||
                thresholdPercent + hysteresis >= 100)
                return false;

            profile = new VacuumCalibrationProfile
            {
                IdleRaw = head.IdleVacuum,
                OpenRaw = head.NoPartPickedVacuum,
                SealedRaw = head.PartPickedVacuum,
                NoPartPercent = 0,
                ThresholdPercent = thresholdPercent,
                HysteresisPercent = hysteresis
            };
            return true;
        }

        public double Normalize(double raw) =>
            Math.Max(0, Math.Min(100,
                100.0 * (raw - OpenRaw) / (SealedRaw - OpenRaw)));

        public double ThresholdRaw =>
            OpenRaw + (SealedRaw - OpenRaw) * (ThresholdPercent / 100.0);

        public VacuumPickupClassification Classify(double raw)
        {
            var percent = Normalize(raw);
            if (percent >= ThresholdPercent + HysteresisPercent)
                return VacuumPickupClassification.PartPresent;
            if (percent <= ThresholdPercent - HysteresisPercent)
                return VacuumPickupClassification.NoPart;
            return VacuumPickupClassification.Unknown;
        }

        /// <summary>
        /// Pure conversion of a live sample into a reusable detection result.
        /// Production part checks and other consumers use this same profile.
        /// </summary>
        public VacuumDetectionResult Observe(
            int headIndex, long rawCount, DateTimeOffset capturedAtUtc) =>
            new VacuumDetectionResult
            {
                HeadIndex = headIndex,
                RawCount = rawCount,
                NormalizedPercent = Normalize(rawCount),
                ThresholdPercent = ThresholdPercent,
                HysteresisPercent = HysteresisPercent,
                Classification = Classify(rawCount),
                CapturedAtUtc = capturedAtUtc
            };
    }

    /// <summary>
    /// One live, head-specific, calibrated vacuum observation. NormalizedPercent
    /// is 0 for pump-on/open nozzle and 100 for obstructed nozzle. Raw counts
    /// are included for diagnostics; no new server-persisted fields are needed.
    /// </summary>
    public sealed class VacuumDetectionResult
    {
        public int HeadIndex { get; internal set; }
        public long RawCount { get; internal set; }
        public double NormalizedPercent { get; internal set; }
        public double ThresholdPercent { get; internal set; }
        public double HysteresisPercent { get; internal set; }
        public VacuumPickupClassification Classification { get; internal set; }
        public DateTimeOffset CapturedAtUtc { get; internal set; }
    }

    public enum VacuumPickupClassification
    {
        Unknown,
        NoPart,
        PartPresent
    }

    /// <summary>
    /// Pure sensor validation: sample ordering, freshness and stability must be
    /// established before any pressure is used to prove actuator operation.
    /// </summary>
    public static class VacuumPressureSampling
    {
        /// <summary>
        /// Idle records its empirical noise before a vacuum-response scale
        /// exists. For pump-on and sealed stages, stability is measured
        /// relative to the observed signal change, not the raw count offset.
        /// </summary>
        public static double AllowedRawSpread(VacuumPressureBand idle, double observedMean)
        {
            if (idle == null)
                return Double.MaxValue;
            var idleSpan = (double)idle.MaximumRawCount - idle.MinimumRawCount;
            var response = Math.Abs(observedMean - idle.MeanRawCount);
            return Math.Max(3.0 * idleSpan, Math.Max(40.0, response * 0.08));
        }

        public static double RequiredResponse(VacuumPressureBand idle)
        {
            if (idle == null) throw new ArgumentNullException(nameof(idle));
            return Math.Max(10.0, 5.0 * ((double)idle.MaximumRawCount - idle.MinimumRawCount));
        }

        public static bool TryCaptureBand(
            IReadOnlyList<VacuumPressureSample> readings,
            int expectedHead, DateTimeOffset nowUtc,
            TimeSpan maximumAge, int minimumSamples, double maximumRawSpread,
            out VacuumPressureBand band, out string failure)
        {
            band = null;
            failure = null;
            if ((expectedHead != 1 && expectedHead != 2) ||
                maximumAge <= TimeSpan.Zero || minimumSamples < 3 ||
                maximumRawSpread < 0)
            {
                failure = "Invalid head or sampling limits.";
                return false;
            }
            if (readings == null || readings.Count < minimumSamples)
            {
                failure = "Not enough fresh pressure samples.";
                return false;
            }
            DateTimeOffset? previous = null;
            foreach (var reading in readings)
            {
                if (reading == null || reading.HeadIndex != expectedHead)
                {
                    failure = "Sensor evidence is missing or belongs to the wrong head.";
                    return false;
                }
                if (reading.CapturedAtUtc > nowUtc ||
                    nowUtc - reading.CapturedAtUtc > maximumAge ||
                    (previous.HasValue && reading.CapturedAtUtc <= previous.Value))
                {
                    failure = "Pressure evidence is stale, duplicated or out of order.";
                    return false;
                }
                previous = reading.CapturedAtUtc;
            }
            var min = readings.Min(r => r.RawCount);
            var max = readings.Max(r => r.RawCount);
            if ((double)max - min > maximumRawSpread)
            {
                failure = "Pressure has not settled into a stable band. " +
                    "Observed minimum " + min + ", maximum " + max +
                    ", span " + ((double)max - min).ToString("F1") +
                    ", permitted span " + maximumRawSpread.ToString("F1") + " raw counts.";
                return false;
            }

            band = new VacuumPressureBand
            {
                HeadIndex = expectedHead,
                SampleCount = readings.Count,
                MinimumRawCount = min,
                MaximumRawCount = max,
                MeanRawCount = readings.Average(r => (double)r.RawCount),
                FirstSampleUtc = readings[0].CapturedAtUtc,
                LastSampleUtc = readings[readings.Count - 1].CapturedAtUtc
            };
            return true;
        }
    }

    /// <summary>
    /// Unsaved per-head candidate. Open nozzle = 0%, obstructed = 100%.
    /// Pump-off idle remains diagnostic evidence. No raw polarity or physical
    /// pressure unit is assumed.
    /// </summary>
    public sealed class VacuumCalibrationCandidate
    {
        public int HeadIndex { get; private set; }
        public VacuumPressureBand Idle { get; private set; }
        public VacuumPressureBand NoPart { get; private set; }
        public VacuumPressureBand Sealed { get; private set; }
        public double NoPartPercent { get; private set; }
        public double PartPresentThresholdPercent { get; private set; }
        public double HysteresisPercent { get; private set; }

        public static bool TryCreate(
            VacuumPressureBand idle, VacuumPressureBand noPart,
            VacuumPressureBand sealedBand, double minimumPumpResponseRaw,
            double minimumPartSeparationPercent, double hysteresisPercent,
            out VacuumCalibrationCandidate candidate, out string failure)
        {
            candidate = null;
            failure = null;
            if (idle == null || noPart == null || sealedBand == null ||
                (idle.HeadIndex != 1 && idle.HeadIndex != 2) ||
                idle.HeadIndex != noPart.HeadIndex ||
                idle.HeadIndex != sealedBand.HeadIndex)
            {
                failure = "All three raw pressure bands must belong to one head.";
                return false;
            }
            if (minimumPumpResponseRaw <= 0 || minimumPartSeparationPercent <= 0 ||
                hysteresisPercent <= 0 || hysteresisPercent >= 50)
            {
                failure = "Invalid calibration acceptance limits.";
                return false;
            }
            if (noPart.FirstSampleUtc <= idle.LastSampleUtc ||
                sealedBand.FirstSampleUtc <= noPart.LastSampleUtc)
            {
                failure = "Calibration stages must be independently sampled in order.";
                return false;
            }
            if (Math.Abs(noPart.MeanRawCount - idle.MeanRawCount) < minimumPumpResponseRaw)
            {
                failure = "The open-nozzle reading does not verify pump response.";
                return false;
            }
            if (Math.Abs(sealedBand.MeanRawCount - idle.MeanRawCount) < minimumPumpResponseRaw)
            {
                failure = "The sealed nozzle is not distinguishable from idle.";
                return false;
            }
            // Reject inverted/non-monotonic readings instead of silently
            // clamping an invalid open-nozzle baseline into 0 or 100%.
            // Prove pump-on/open response lies between idle and sealed; this
            // is a validity check, not the part-detection normalization.
            var openProgressFromIdle =
                100.0 * (noPart.MeanRawCount - idle.MeanRawCount) /
                (sealedBand.MeanRawCount - idle.MeanRawCount);
            if (openProgressFromIdle <= 0 || openProgressFromIdle >= 100 ||
                100 - openProgressFromIdle < minimumPartSeparationPercent ||
                100 - openProgressFromIdle <= hysteresisPercent * 2)
            {
                failure = "The open and sealed nozzle values cannot be reliably classified.";
                return false;
            }
            candidate = new VacuumCalibrationCandidate
            {
                HeadIndex = idle.HeadIndex,
                Idle = idle,
                NoPart = noPart,
                Sealed = sealedBand,
                NoPartPercent = 0,
                PartPresentThresholdPercent = 50,
                HysteresisPercent = hysteresisPercent
            };
            return true;
        }

        public double Normalize(long rawCounts) =>
            ClampPercent(100.0 * (rawCounts - NoPart.MeanRawCount) /
                (Sealed.MeanRawCount - NoPart.MeanRawCount));

        public VacuumPickupClassification Classify(long rawCounts)
        {
            var normalized = Normalize(rawCounts);
            if (normalized >= PartPresentThresholdPercent + HysteresisPercent)
                return VacuumPickupClassification.PartPresent;
            if (normalized <= PartPresentThresholdPercent - HysteresisPercent)
                return VacuumPickupClassification.NoPart;
            return VacuumPickupClassification.Unknown;
        }

        public bool IsBackAtIdle(VacuumPressureBand release, double maxDriftPercent)
        {
            return release != null && release.HeadIndex == HeadIndex &&
                release.FirstSampleUtc > Sealed.LastSampleUtc &&
                maxDriftPercent >= 0 &&
                Math.Abs(100.0 * (release.MeanRawCount - Idle.MeanRawCount) /
                    (Sealed.MeanRawCount - Idle.MeanRawCount)) <= maxDriftPercent;
        }

        private static double ClampPercent(double value) =>
            Math.Max(0, Math.Min(100, value));
    }
}
