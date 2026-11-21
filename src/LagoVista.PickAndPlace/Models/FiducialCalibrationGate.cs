using System;
using LagoVista.Manufacturing.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Deterministic evidence gate for machine-origin movement/commit.
    /// Device/profile identity, machine mode, and actual physical idleness
    /// are independently checked by the real procedure around this gate.
    /// </summary>
    public static class FiducialCalibrationGate
    {
        public const int MaximumFrameAgeMilliseconds = 2000;

        public static bool TryQualify(FiducialCircleObservation observation,
            string expectedProfile, double expectedPixelsPerMm,
            DateTimeOffset lastMoveCompletedUtc, DateTimeOffset nowUtc,
            out string rejection)
        {
            if (observation == null)
            {
                rejection = "No circle observed";
                return false;
            }
            if (observation.CameraType != CameraTypes.Position)
            {
                rejection = "Circle came from a different camera";
                return false;
            }
            if (observation.ProfileKey != expectedProfile)
            {
                rejection = "Vision profile changed";
                return false;
            }
            if (!IsFinitePositive(expectedPixelsPerMm) ||
                !IsFinitePositive(observation.PixelsPerMillimeter) ||
                Math.Abs(observation.PixelsPerMillimeter - expectedPixelsPerMm) > 0.0001)
            {
                rejection = "Camera pixel scale changed";
                return false;
            }
            if (observation.FrameWidth <= 0 || observation.FrameHeight <= 0 ||
                !IsFinitePositive(observation.RadiusPx) ||
                !IsFinite(observation.CenterXpx) || !IsFinite(observation.CenterYpx) ||
                !IsFinite(observation.OffsetXpx) || !IsFinite(observation.OffsetYpx))
            {
                rejection = "Invalid circle dimensions or coordinates";
                return false;
            }
            if (observation.FrameAtUtc <= lastMoveCompletedUtc ||
                observation.ObservedAtUtc <= lastMoveCompletedUtc)
            {
                rejection = "Observation predates last completed move";
                return false;
            }
            if (observation.FrameAtUtc > nowUtc || observation.ObservedAtUtc > nowUtc ||
                (nowUtc - observation.FrameAtUtc).TotalMilliseconds > MaximumFrameAgeMilliseconds ||
                (nowUtc - observation.ObservedAtUtc).TotalMilliseconds > MaximumFrameAgeMilliseconds)
            {
                rejection = "Stale camera frame";
                return false;
            }
            if (!observation.Stabilized)
            {
                rejection = "Insufficient consecutive detections";
                return false;
            }
            if (!observation.PositionStable)
            {
                rejection = "Circle position is not stable";
                return false;
            }

            rejection = null;
            return true;
        }

        public static bool CanCommitOrigin(FiducialCircleObservation observation,
            string expectedProfile, double expectedPixelsPerMm,
            DateTimeOffset lastMoveCompletedUtc, DateTimeOffset nowUtc,
            out string reason)
        {
            if (!TryQualify(observation, expectedProfile, expectedPixelsPerMm,
                lastMoveCompletedUtc, nowUtc, out reason))
                return false;

            if (!observation.Centered ||
                !IsFinitePositive(observation.ErrorToleranceMm) ||
                Math.Abs(observation.OffsetXpx / observation.PixelsPerMillimeter) >= observation.ErrorToleranceMm ||
                Math.Abs(observation.OffsetYpx / observation.PixelsPerMillimeter) >= observation.ErrorToleranceMm)
            {
                reason = "Circle is stable but outside centering tolerance";
                return false;
            }

            reason = null;
            return true;
        }

        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
        private static bool IsFinitePositive(double value) =>
            value > 0 && IsFinite(value);
    }
}
