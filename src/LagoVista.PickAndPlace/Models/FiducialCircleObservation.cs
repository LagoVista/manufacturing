using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Immutable-by-convention observation from the existing macfiducial Hough
    /// detector. All coordinates and radii are measured on the processed image.
    /// No invented confidence or simulated image data is included.
    /// </summary>
    public sealed class FiducialCircleObservation
    {
        public Guid TrackId { get; set; }
        public DateTimeOffset ObservedAtUtc { get; set; }
        public DateTimeOffset FrameAtUtc { get; set; }
        public string ProfileKey { get; set; }
        public double PixelsPerMillimeter { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public int FoundCount { get; set; }
        public bool Stabilized { get; set; }
        public bool PositionStable { get; set; }
        public bool Centered { get; set; }
        public double CenterXpx { get; set; }
        public double CenterYpx { get; set; }
        public double RadiusPx { get; set; }
        public double RadiusMm { get; set; }
        public double OffsetXpx { get; set; }
        public double OffsetYpx { get; set; }
        public double OffsetXmm { get; set; }
        public double OffsetYmm { get; set; }
        public double? PositionSpreadMm { get; set; }
        public double SearchRadiusPx { get; set; }
        public double ErrorToleranceMm { get; set; }
        public string RejectionReason { get; set; }
        public bool Qualified { get; set; }
    }
}
