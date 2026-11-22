using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// An actual observed exposure-tuning sample from the position camera.
    /// Values are produced by the existing production dark/white reference sampler;
    /// they are not confidence estimates or UI-generated measurements.
    /// </summary>
    public sealed class CameraExposureTuneProgress
    {
        public DateTimeOffset ObservedAtUtc { get; set; }
        public int Iteration { get; set; }
        public int SampleFrames { get; set; }
        public double CandidateExposure { get; set; }
        public double DarkLevel { get; set; }
        public double WhiteLevel { get; set; }
        public double DarkRange { get; set; }
        public double WhiteRange { get; set; }
        public double Contrast => WhiteLevel - DarkLevel;
        public bool Preferred { get; set; }
        public bool Usable { get; set; }
        public string Decision { get; set; }
    }
}
