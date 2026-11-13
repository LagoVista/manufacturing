using System;

namespace LagoVista.PickAndPlace.Models
{
    public enum PocketPresenceDecision
    {
        AwaitingEvidence,
        Present,
        Empty,
        Ambiguous,
        CameraUnavailable
    }

    /// <summary>
    /// Conservative vision-only pocket inspection. Stable centered rectangles
    /// identify the part under the configured part-in-tape profile. Absence
    /// requires many NEW processed frames, no candidate in ANY of those frames,
    /// and a minimum observation duration. Missing frames are never empty.
    /// </summary>
    public static class PocketPresenceObservationPolicy
    {
        public const int MinimumEmptyFrames = 15;
        public static readonly TimeSpan MinimumEmptyDuration = TimeSpan.FromMilliseconds(850);
        public static readonly TimeSpan MaximumFrameAge = TimeSpan.FromSeconds(2);

        public static PocketPresenceDecision Evaluate(
            PocketRectangleFrameObservation frame, long frameIdAtArrival,
            DateTimeOffset observationStartUtc, DateTimeOffset now,
            string expectedCameraId, string expectedProfileKey)
        {
            if (frame == null || frame.FrameId <= frameIdAtArrival)
                return PocketPresenceDecision.AwaitingEvidence;

            if (frame.CapturedAtUtc < observationStartUtc ||
                now - frame.CapturedAtUtc > MaximumFrameAge ||
                frame.Width <= 0 || frame.Height <= 0 ||
                !String.Equals(frame.CameraId, expectedCameraId, StringComparison.Ordinal) ||
                !String.Equals(frame.VisionProfileKey, expectedProfileKey, StringComparison.Ordinal))
                return PocketPresenceDecision.CameraUnavailable;

            if (frame.StableCenteredCount > 1)
                return PocketPresenceDecision.Ambiguous;
            if (frame.StableCenteredCount == 1 &&
                frame.StableCenteredFoundCount >= 5)
                return PocketPresenceDecision.Present;

            if (frame.FrameId - frameIdAtArrival >= MinimumEmptyFrames &&
                now - observationStartUtc >= MinimumEmptyDuration &&
                frame.RectangleCount == 0 &&
                frame.LastRectangleFrameId <= frameIdAtArrival)
                return PocketPresenceDecision.Empty;

            return PocketPresenceDecision.AwaitingEvidence;
        }
    }
}
