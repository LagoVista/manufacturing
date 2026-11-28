using System;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Staged, operator-editable values consumed by the production macfiducial
    /// Hough detector. Draft values do not affect the detector until Apply.
    /// </summary>
    public sealed class FiducialDetectorTuning : ModelBase
    {
        private double _dp;
        private double _minDistanceMm;
        private double _edgeThreshold;
        private double _accumulatorThreshold;
        private double _minimumRadiusMm;
        private double _maximumRadiusMm;
        private double _searchRadiusMm;
        private double _centerToleranceMm;
        private int _stabilizationCount;
        private bool _autoThreshold;
        private byte _primaryThreshold;

        public double Dp { get => _dp; set => Set(ref _dp, value); }
        public double MinDistanceMm { get => _minDistanceMm; set => Set(ref _minDistanceMm, value); }
        public double EdgeThreshold { get => _edgeThreshold; set => Set(ref _edgeThreshold, value); }
        public double AccumulatorThreshold { get => _accumulatorThreshold; set => Set(ref _accumulatorThreshold, value); }
        public double MinimumRadiusMm { get => _minimumRadiusMm; set => Set(ref _minimumRadiusMm, value); }
        public double MaximumRadiusMm { get => _maximumRadiusMm; set => Set(ref _maximumRadiusMm, value); }
        public double SearchRadiusMm { get => _searchRadiusMm; set => Set(ref _searchRadiusMm, value); }
        public double CenterToleranceMm { get => _centerToleranceMm; set => Set(ref _centerToleranceMm, value); }
        public int StabilizationCount { get => _stabilizationCount; set => Set(ref _stabilizationCount, value); }

        public bool AutoThreshold { get => _autoThreshold; set => Set(ref _autoThreshold, value); }
        public byte PrimaryThreshold { get => _primaryThreshold; set => Set(ref _primaryThreshold, value); }

        public static FiducialDetectorTuning FromProfile(VisionProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return new FiducialDetectorTuning
            {
                Dp = profile.HoughCirclesDP,
                MinDistanceMm = profile.HoughCirclesMinDistance,
                EdgeThreshold = profile.HoughCirclesParam1,
                AccumulatorThreshold = profile.HoughCirclesParam2,
                MinimumRadiusMm = profile.HoughCirclesMinRadius,
                MaximumRadiusMm = profile.HoughCirclesMaxRadius,
                SearchRadiusMm = profile.TargetImageRadius,
                CenterToleranceMm = profile.ErrorToleranceMM,
                StabilizationCount = profile.StabilizationCount,
                AutoThreshold = profile.AutoThreshold,
                PrimaryThreshold = profile.PrimaryThreshold
            };
        }

        public bool Validate(out string reason)
        {
            if (PrimaryThreshold > 100)
                reason = "Manual threshold must be 0-100.";
            else if (!Between(Dp, 0.1, 10))
                reason = "Hough DP must be between 0.1 and 10.";
            else if (!Between(MinDistanceMm, 0.01, 100))
                reason = "Minimum circle separation must be 0.01-100 mm.";
            else if (!Between(EdgeThreshold, 1, 5000))
                reason = "Edge threshold must be between 1 and 5000.";
            else if (!Between(AccumulatorThreshold, 1, 5000))
                reason = "Accumulator threshold must be between 1 and 5000.";
            else if (!Between(MinimumRadiusMm, 0.01, 100))
                reason = "Minimum radius must be 0.01-100 mm.";
            else if (!Between(MaximumRadiusMm, 0.01, 100) ||
                     MaximumRadiusMm <= MinimumRadiusMm)
                reason = "Maximum radius must exceed minimum radius and be at most 100 mm.";
            else if (!Between(SearchRadiusMm, 0.01, 100))
                reason = "Search radius must be 0.01-100 mm.";
            else if (!Between(CenterToleranceMm, 0.01, 1))
                reason = "Centering tolerance must be 0.01-1.0 mm.";
            else if (StabilizationCount < 2 || StabilizationCount > 30)
                reason = "Stable detection count must be 2-30 frames.";
            else
            {
                reason = null;
                return true;
            }
            return false;
        }

        public bool Matches(VisionProfile profile)
        {
            if (profile == null) return false;
            return Dp == profile.HoughCirclesDP &&
                   MinDistanceMm == profile.HoughCirclesMinDistance &&
                   EdgeThreshold == profile.HoughCirclesParam1 &&
                   AccumulatorThreshold == profile.HoughCirclesParam2 &&
                   MinimumRadiusMm == profile.HoughCirclesMinRadius &&
                   MaximumRadiusMm == profile.HoughCirclesMaxRadius &&
                   SearchRadiusMm == profile.TargetImageRadius &&
                   CenterToleranceMm == profile.ErrorToleranceMM &&
                   StabilizationCount == profile.StabilizationCount &&
                   AutoThreshold == profile.AutoThreshold &&
                   PrimaryThreshold == profile.PrimaryThreshold;
        }

        public void ApplyTo(VisionProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!Validate(out var reason))
                throw new InvalidOperationException(reason);

            profile.HoughCirclesDP = Dp;
            profile.HoughCirclesMinDistance = MinDistanceMm;
            profile.HoughCirclesParam1 = EdgeThreshold;
            profile.HoughCirclesParam2 = AccumulatorThreshold;
            profile.HoughCirclesMinRadius = MinimumRadiusMm;
            profile.HoughCirclesMaxRadius = MaximumRadiusMm;
            profile.TargetImageRadius = SearchRadiusMm;
            profile.ErrorToleranceMM = CenterToleranceMm;
            profile.StabilizationCount = StabilizationCount;
            profile.PrimaryThreshold = PrimaryThreshold;
            profile.AutoThreshold = AutoThreshold;
        }

        private static bool Between(double value, double min, double max) =>
            !double.IsInfinity(value) && !double.IsNaN(value) &&
            value >= min && value <= max;
    }
}
