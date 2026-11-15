using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Bottom-camera observation of the picked calibration plate. The XY
    /// measurement is plate center MINUS the T004-calibrated nozzle axis, in
    /// machine coordinates, NOT relative to the bottom camera image center.
    /// </summary>
    public sealed class CameraToolPickupObservation
    {
        public int HeadIndex { get; set; }
        public string NozzleId { get; set; }
        public string FixtureId { get; set; }
        public Point2D<double> PartRelativeToNozzleMm { get; set; }
        public bool VacuumConfirmed { get; set; }
        public bool StableVision { get; set; }
        public bool NozzleCalibrated { get; set; }
        public DateTimeOffset MotionFinishedUtc { get; set; }
        public DateTimeOffset CapturedUtc { get; set; }
    }

    /// <summary>Unsaved proposal for the same installed head/nozzle/fixture.</summary>
    public sealed class CameraToolPickupCandidate
    {
        public int HeadIndex { get; internal set; }
        public string NozzleId { get; internal set; }
        public string FixtureId { get; internal set; }
        public Point2D<double> PreviousOffsetMm { get; internal set; }
        public Point2D<double> ProposedOffsetMm { get; internal set; }
        public Point2D<double> SecondPickupJogMm { get; internal set; }
        public DateTimeOffset FirstCaptureUtc { get; internal set; }
    }

    /// <summary>
    /// Pure camera-to-tool calibration geometry; never moves hardware,
    /// changes G92, changes a nozzle's runout, or persists an offset.
    /// A second pickup applies a relative jog while the original tool offset
    /// stays active, then commits the candidate only after independent proof.
    /// </summary>
    public static class CameraToolPickupCalibrationMath
    {
        public const double MaximumCorrectionPerAxisMm = 2.0;
        public const double VerificationToleranceMm = 0.12;
        public const double TravelClearanceMm = 5.0;
        public static readonly TimeSpan MaximumVisionAge = TimeSpan.FromSeconds(3);

        public static bool TryPropose(Point2D<double> existingOffset,
            CameraToolPickupObservation first, DateTimeOffset now,
            out CameraToolPickupCandidate candidate, out string reason)
        {
            candidate = null;
            if (!Valid(existingOffset))
            {
                reason = "The existing tool-head offset is invalid.";
                return false;
            }
            if (!ValidObservation(first, now, out reason))
                return false;
            var error = first.PartRelativeToNozzleMm;
            if (Math.Abs(error.X) > MaximumCorrectionPerAxisMm ||
                Math.Abs(error.Y) > MaximumCorrectionPerAxisMm)
            {
                reason = "Pickup error exceeds the guarded XY correction limit.";
                return false;
            }
            // Actual nozzle error relative to top-camera plate center is the
            // opposite of (picked part center minus nozzle center).
            // Machine selects a head by moving +Offset and then applying G92.
            // Hence corrected Offset = old Offset + observed part-minus-nozzle.
            var proposed = new Point2D<double>(
                existingOffset.X + error.X, existingOffset.Y + error.Y);
            if (!Valid(proposed))
            {
                reason = "Proposed offset is invalid.";
                return false;
            }
            candidate = new CameraToolPickupCandidate
            {
                HeadIndex = first.HeadIndex,
                NozzleId = first.NozzleId,
                FixtureId = first.FixtureId,
                PreviousOffsetMm = Copy(existingOffset),
                ProposedOffsetMm = proposed,
                SecondPickupJogMm = Copy(error),
                FirstCaptureUtc = first.CapturedUtc
            };
            reason = null;
            return true;
        }

        public static bool TryPlanSecondPickup(CameraToolPickupCandidate candidate,
            Point2D<double> logicalControllerXY, Point2D<double> workArea,
            out Point2D<double> relativeJog, out string reason)
        {
            relativeJog = null;
            if (candidate == null || !Valid(logicalControllerXY) ||
                !Valid(workArea) || !Valid(candidate.PreviousOffsetMm) ||
                !Valid(candidate.SecondPickupJogMm) ||
                workArea.X <= 2 * TravelClearanceMm ||
                workArea.Y <= 2 * TravelClearanceMm)
            {
                reason = "Missing or invalid movement context.";
                return false;
            }
            var correction = candidate.SecondPickupJogMm;
            if (Math.Abs(correction.X) > MaximumCorrectionPerAxisMm ||
                Math.Abs(correction.Y) > MaximumCorrectionPerAxisMm)
            {
                reason = "Correction exceeds the guarded XY limit.";
                return false;
            }
            // With a selected tool, G92 reports logical XY, but physical
            // gantry XY = reported XY + CURRENT (still old) tool offset.
            var physicalX = logicalControllerXY.X + candidate.PreviousOffsetMm.X;
            var physicalY = logicalControllerXY.Y + candidate.PreviousOffsetMm.Y;
            if (!Safe(physicalX, physicalY, workArea) ||
                !Safe(physicalX + correction.X, physicalY + correction.Y, workArea))
            {
                reason = "Second pickup could approach a physical XY travel limit.";
                return false;
            }
            relativeJog = Copy(correction);
            reason = null;
            return true;
        }

        public static bool TryVerify(CameraToolPickupCandidate candidate,
            CameraToolPickupObservation second, DateTimeOffset now,
            out double residualMm, out string reason)
        {
            residualMm = Double.NaN;
            if (candidate == null)
            {
                reason = "No first-pass proposal exists.";
                return false;
            }
            if (!ValidObservation(second, now, out reason))
                return false;
            if (second.HeadIndex != candidate.HeadIndex ||
                !String.Equals(second.NozzleId, candidate.NozzleId, StringComparison.Ordinal) ||
                !String.Equals(second.FixtureId, candidate.FixtureId, StringComparison.Ordinal))
            {
                reason = "Head, installed nozzle, or fixture changed between passes.";
                return false;
            }
            if (second.MotionFinishedUtc <= candidate.FirstCaptureUtc ||
                second.CapturedUtc <= candidate.FirstCaptureUtc)
            {
                reason = "Second pass has no independently fresh pickup.";
                return false;
            }
            var error = second.PartRelativeToNozzleMm;
            residualMm = Math.Sqrt(error.X * error.X + error.Y * error.Y);
            if (residualMm > VerificationToleranceMm)
            {
                reason = "Second pickup did not verify the XY correction.";
                return false;
            }
            reason = null;
            return true;
        }

        private static bool ValidObservation(CameraToolPickupObservation obs,
            DateTimeOffset now, out string reason)
        {
            if (obs == null || obs.HeadIndex <= 0 ||
                String.IsNullOrWhiteSpace(obs.NozzleId) ||
                String.IsNullOrWhiteSpace(obs.FixtureId) ||
                !Valid(obs.PartRelativeToNozzleMm))
            {
                reason = "Invalid or unidentified pickup observation.";
                return false;
            }
            if (!obs.NozzleCalibrated || !obs.VacuumConfirmed || !obs.StableVision)
            {
                reason = "Nozzle-runout, vacuum, or stable camera proof is missing.";
                return false;
            }
            if (obs.MotionFinishedUtc == default(DateTimeOffset) ||
                obs.CapturedUtc < obs.MotionFinishedUtc ||
                obs.CapturedUtc > now.AddMilliseconds(200) ||
                now - obs.CapturedUtc > MaximumVisionAge)
            {
                reason = "Optical observation was stale or predates pickup motion.";
                return false;
            }
            reason = null;
            return true;
        }

        private static Point2D<double> Copy(Point2D<double> p) =>
            new Point2D<double>(p.X, p.Y);
        private static bool Valid(Point2D<double> p) =>
            p != null && !Double.IsNaN(p.X) && !Double.IsInfinity(p.X) &&
            !Double.IsNaN(p.Y) && !Double.IsInfinity(p.Y);
        private static bool Safe(double x, double y, Point2D<double> area) =>
            x >= TravelClearanceMm && y >= TravelClearanceMm &&
            x <= area.X - TravelClearanceMm &&
            y <= area.Y - TravelClearanceMm;
    }
}
