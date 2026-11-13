using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Immutable-by-convention snapshot produced once per valid processed
    /// rectangle-detection frame. Absence is meaningful only when FrameId
    /// advances under the same camera and part-in-tape vision profile.
    /// </summary>
    public sealed class PocketRectangleFrameObservation
    {
        public long FrameId { get; set; }
        public long LastRectangleFrameId { get; set; }
        public DateTimeOffset CapturedAtUtc { get; set; }
        public string CameraId { get; set; }
        public string VisionProfileKey { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int RectangleCount { get; set; }
        public int StableCenteredCount { get; set; }
        public int StableCenteredFoundCount { get; set; }
        public double CenteredWidthMm { get; set; }
        public double CenteredHeightMm { get; set; }
        public double CenteredOffsetXmm { get; set; }
        public double CenteredOffsetYmm { get; set; }
    }
}
