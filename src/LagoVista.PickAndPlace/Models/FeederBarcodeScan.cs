using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Combines complete 10-pixel-band barcode measurements taken at overlapping
    /// machine positions. The leading 5-unit bar establishes optical scale;
    /// camera pixels/mm is never used to classify a symbol.
    /// </summary>
    public sealed class FeederBarcodeScan
    {
        private sealed class MeasuredBar
        {
            public double CenterMm { get; set; }
            public double WidthUnits { get; set; }
        }

        private const int TotalBars = 11;
        private const double StartBarWidthMm = 2.5; // physical scale only for combining machine coordinates
        private const double MaximumMarkerLengthMm = 20.0;
        private const double DuplicateCenterToleranceMm = 0.28;
        private const double DuplicateWidthToleranceUnits = 0.32;
        private const double WidthToleranceUnits = 0.20;
        // Machine positions are a coarse registration hint. Require two
        // matching bars before accepting a later frame, and refine its offset.
        private const int MinimumMatchingBars = 2;
        private const double MaximumRegistrationCorrectionMm = 1.5;
        private readonly List<MeasuredBar> _bars = new List<MeasuredBar>();
        private double? _startCenterMm;
        private double _pixelsPerUnit;
        private bool _overlapConflict;

        public int CaptureCount { get; private set; }
        public bool HasVerifiedStart => _startCenterMm.HasValue;
        public double PixelsPerUnit => _pixelsPerUnit;
        // Available as soon as the start width is measured, before its
        // world-coordinate anchor has been assigned.
        public double PixelsPerMmFromStart => _pixelsPerUnit > 0
            ? _pixelsPerUnit / FeederBarcodeSignal.BaseWidthMm : 0;
        public int ObservedBars => _bars.Count;
        public int LastOverlapCount { get; private set; }
        public double LastRegistrationCorrectionMm { get; private set; }
        public bool IsComplete { get; private set; }
        public int? FeederId { get; private set; }
        public string Status { get; private set; } = "Frame the first bar, then capture.";

        /// <summary>
        /// Supply the fixed-band signal and the measured position of the
        /// position camera on the barcode's positive axis: X for horizontal
        /// feeders, -Y for vertical feeders. Signal pixels increase along
        /// that same axis in the displayed (mirrored as configured) image.
        /// </summary>
        public void AddCapture(byte[] signal, double machineAxisMm, double nominalPixelsPerMm)
        {
            if (IsComplete)
                return;
            if (signal == null || signal.Length < 20)
                throw new ArgumentException("Barcode capture must contain a complete signal.", nameof(signal));
            if (double.IsNaN(machineAxisMm) || double.IsInfinity(machineAxisMm))
                throw new ArgumentOutOfRangeException(nameof(machineAxisMm));

            CaptureCount++;
            IReadOnlyList<FeederBarcodeBar> detected;
            if (!HasVerifiedStart)
            {
                var calibration = FeederBarcodeSignal.ReadFromStart(signal, nominalPixelsPerMm);
                if (!calibration.HasVerifiedStart)
                {
                    Status = calibration.Status;
                    return;
                }

                _pixelsPerUnit = calibration.PixelsPerUnit;
                detected = calibration.Bars;
                var first = detected[0];
                _startCenterMm = ToWorldCenter(machineAxisMm, signal.Length,
                    (first.StartPixel + first.EndPixel) / 2.0);
            }
            else
            {
                // Later frames may not contain START at all. Use the same
                // XOR/median measurements, but no physical-width gate.
                detected = FeederBarcodeSignal.FindBars(signal, nominalPixelsPerMm,
                    new double[0], usePhysicalWidthFilter: false);
            }

            var incoming = new List<MeasuredBar>();
            foreach (var bar in detected)
            {
                var center = ToWorldCenter(machineAxisMm, signal.Length,
                    (bar.StartPixel + bar.EndPixel) / 2.0);
                // Do not admit unrelated machine/background features beyond
                // the 20 mm prototype marker.
                if (center < _startCenterMm.Value - StartBarWidthMm / 2 - 0.25 -
                        MaximumRegistrationCorrectionMm ||
                    center > _startCenterMm.Value - StartBarWidthMm / 2 +
                        MaximumMarkerLengthMm + MaximumRegistrationCorrectionMm)
                    continue;

                incoming.Add(new MeasuredBar
                {
                    CenterMm = center,
                    WidthUnits = (bar.EndPixel - bar.StartPixel) / _pixelsPerUnit
                });
            }

            if (_bars.Count == 0)
            {
                _bars.AddRange(incoming);
                Validate();
                return;
            }

            // A camera move may be slightly different from its requested
            // machine distance, and the optical start scale may not equal the
            // actual machine mm scale. Register overlapping bar centers before
            // deduplicating. One repeated width is not sufficient because the
            // 0/1/2/3 pattern deliberately repeats.
            if (!TryRegisterOverlap(incoming, out var correctionMm, out var overlapCount))
            {
                Status = $"OVERLAP NOT VERIFIED: no two matching bars at capture {CaptureCount}. " +
                    $"Retained {_bars.Count}/{TotalBars} unique bars; restart scan with smaller steps.";
                return;
            }

            LastRegistrationCorrectionMm = correctionMm;
            LastOverlapCount = overlapCount;
            var additions = new List<MeasuredBar>();
            foreach (var bar in incoming)
            {
                bar.CenterMm += correctionMm;
                if (bar.CenterMm < _startCenterMm.Value - StartBarWidthMm / 2 - 0.25 ||
                    bar.CenterMm > _startCenterMm.Value - StartBarWidthMm / 2 + MaximumMarkerLengthMm)
                    continue;

                var previous = _bars.FirstOrDefault(candidate =>
                    Math.Abs(candidate.CenterMm - bar.CenterMm) < DuplicateCenterToleranceMm);
                if (previous != null)
                {
                    if (Math.Abs(previous.WidthUnits - bar.WidthUnits) > DuplicateWidthToleranceUnits)
                    {
                        _overlapConflict = true;
                        break;
                    }
                    continue;
                }

                additions.Add(bar);
            }

            if (!_overlapConflict)
                _bars.AddRange(additions);

            Validate();
            Status += $" (overlap {overlapCount}, registration {correctionMm:+0.00;-0.00;0.00} mm)";
        }

        private bool TryRegisterOverlap(IReadOnlyList<MeasuredBar> incoming,
            out double bestCorrectionMm, out int bestMatchCount)
        {
            bestCorrectionMm = 0;
            bestMatchCount = 0;
            var bestError = double.MaxValue;

            foreach (var sample in incoming)
            {
                foreach (var known in _bars)
                {
                    if (Math.Abs(sample.WidthUnits - known.WidthUnits) >
                        DuplicateWidthToleranceUnits)
                        continue;

                    var correction = known.CenterMm - sample.CenterMm;
                    if (Math.Abs(correction) > MaximumRegistrationCorrectionMm)
                        continue;

                    var used = new HashSet<MeasuredBar>();
                    var matches = 0;
                    var error = 0.0;
                    foreach (var candidate in incoming)
                    {
                        var match = _bars
                            .Where(item => !used.Contains(item) &&
                                Math.Abs(item.WidthUnits - candidate.WidthUnits) <=
                                    DuplicateWidthToleranceUnits &&
                                Math.Abs(item.CenterMm - candidate.CenterMm - correction) <
                                    DuplicateCenterToleranceMm)
                            .OrderBy(item => Math.Abs(item.CenterMm -
                                candidate.CenterMm - correction))
                            .FirstOrDefault();
                        if (match == null)
                            continue;

                        used.Add(match);
                        matches++;
                        error += Math.Abs(match.CenterMm - candidate.CenterMm - correction);
                    }

                    // Prefer the strongest actual overlap, breaking ties by
                    // residual and smallest correction. The best correction is
                    // applied only after two independent matches are found.
                    var score = error + Math.Abs(correction) * 0.001;
                    if (matches > bestMatchCount ||
                        (matches == bestMatchCount && score < bestError))
                    {
                        bestMatchCount = matches;
                        bestCorrectionMm = correction;
                        bestError = score;
                    }
                }
            }

            return bestMatchCount >= MinimumMatchingBars;
        }

        private double ToWorldCenter(double machineAxisMm, int framePixels, double pixelCenter)
        {
            return machineAxisMm + (pixelCenter - framePixels / 2.0) / PixelsPerMmFromStart;
        }

        private static int? ClassifyData(double units)
        {
            var best = -1;
            var error = double.MaxValue;
            for (var i = 0; i < FeederBarcodeSignal.DataBarUnits.Length; i++)
            {
                var diff = Math.Abs(units - FeederBarcodeSignal.DataBarUnits[i]);
                if (diff < error)
                {
                    error = diff;
                    best = i;
                }
            }

            return error < FeederBarcodeSignal.DataWidthToleranceUnits ? (int?)best : null;
        }

        /// <summary>
        /// Two-bit CRC of the 16 identity bits, MSB-first, polynomial
        /// x^2+x+1, initial register 3, no reflection and no final xor.
        /// This defines the check-symbol convention for the 11-bar marker.
        /// A two-bit check only detects errors; it is not an authentication code.
        /// </summary>
        public static int CalculateCheckSymbol(int feederId)
        {
            if (feederId < 0 || feederId > 0xFFFF)
                throw new ArgumentOutOfRangeException(nameof(feederId));

            var crc = 3;
            for (var bitIndex = 15; bitIndex >= 0; bitIndex--)
            {
                var bit = (feederId >> bitIndex) & 1;
                var feedback = ((crc >> 1) & 1) ^ bit;
                crc = (crc << 1) & 3;
                if (feedback != 0)
                    crc ^= 3;
            }
            return crc;
        }

        private void Validate()
        {
            IsComplete = false;
            FeederId = null;
            if (_overlapConflict)
            {
                Status = "OVERLAP MISMATCH: repeated bar widths disagree; retry the scan.";
                return;
            }

            var ordered = _bars.OrderBy(bar => bar.CenterMm).ToList();
            if (ordered.Count < TotalBars)
            {
                Status = $"READING: {ordered.Count}/{TotalBars} complete bars across {CaptureCount} capture(s).";
                return;
            }
            if (ordered.Count != TotalBars)
            {
                Status = $"BAR COUNT MISMATCH: found {ordered.Count}, expected {TotalBars}.";
                return;
            }
            if (Math.Abs(ordered[0].WidthUnits - FeederBarcodeSignal.StartBarUnits) >= WidthToleranceUnits)
            {
                Status = "START NOT VERIFIED: coordinate alignment lost.";
                return;
            }

            // Verify spacing so a missed bar cannot silently be replaced by
            // an unrelated dark run. The printed sheet uses one-unit gaps.
            for (var i = 1; i < TotalBars; i++)
            {
                var deltaMm = ordered[i].CenterMm - ordered[i - 1].CenterMm;
                var halfWidthsMm = (ordered[i].WidthUnits + ordered[i - 1].WidthUnits) *
                    FeederBarcodeSignal.BaseWidthMm / 2.0;
                var gapMm = deltaMm - halfWidthsMm;
                if (gapMm < 0.20 || gapMm > 0.80)
                {
                    Status = $"GAP MISMATCH: between bars {i} and {i + 1} ({gapMm:F2} mm).";
                    return;
                }
            }

            var candidateId = 0;
            for (var i = 1; i <= 8; i++)
            {
                var value = ClassifyData(ordered[i].WidthUnits);
                if (!value.HasValue)
                {
                    Status = $"DATA BAR {i}: {ordered[i].WidthUnits:F2}x is not a valid width.";
                    return;
                }
                candidateId = (candidateId << 2) | value.Value;
            }

            var check = ClassifyData(ordered[9].WidthUnits);
            if (!check.HasValue)
            {
                Status = $"CHECK BAR: {ordered[9].WidthUnits:F2}x is not a valid width.";
                return;
            }
            if (Math.Abs(ordered[10].WidthUnits - FeederBarcodeSignal.EndBarUnits) >= WidthToleranceUnits)
            {
                Status = $"END BAR: expected 0.5x, measured {ordered[10].WidthUnits:F2}x.";
                return;
            }

            var expectedCheck = CalculateCheckSymbol(candidateId);
            if (check.Value != expectedCheck)
            {
                Status = $"CHECK FAILED: ID {candidateId:X4}, check {check.Value}, expected {expectedCheck}.";
                return;
            }

            IsComplete = true;
            FeederId = candidateId;
            Status = $"BARCODE VERIFIED: ID {candidateId:X4}, 11 bars, CRC-2 OK, {CaptureCount} capture(s).";
        }
    }
}
