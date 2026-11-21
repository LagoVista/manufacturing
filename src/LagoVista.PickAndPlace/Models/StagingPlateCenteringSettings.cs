using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Draft, per-staging-plate motion policy. Motion limits are NOT vision
    /// profile parameters. They are saved independently after explicit
    /// operator review, rather than changed by the live image editor.
    /// </summary>
    public sealed class StagingPlateCenteringSettings : ModelBase
    {
        private double _maxCorrectionPerAxisMm = StagingPlateCenteringPolicy.DefaultCorrectionPerAxisMm;
        private int _maximumCorrectionMoves = StagingPlateCenteringPolicy.DefaultMaximumCorrections;

        public double MaxCorrectionPerAxisMm
        {
            get => _maxCorrectionPerAxisMm;
            set => Set(ref _maxCorrectionPerAxisMm, value);
        }

        public int MaximumCorrectionMoves
        {
            get => _maximumCorrectionMoves;
            set => Set(ref _maximumCorrectionMoves, value);
        }

        public bool Validate(out string reason) =>
            StagingPlateCenteringPolicy.TryValidateSettings(
                MaxCorrectionPerAxisMm, MaximumCorrectionMoves, out reason);

        public bool Matches(MachineStagingPlate plate) =>
            plate != null &&
            plate.MaxCenteringCorrectionPerAxisMm == MaxCorrectionPerAxisMm &&
            plate.MaximumCenteringMoves == MaximumCorrectionMoves;

        public static StagingPlateCenteringSettings FromPlate(MachineStagingPlate plate)
        {
            if (plate == null) throw new ArgumentNullException(nameof(plate));
            return new StagingPlateCenteringSettings
            {
                MaxCorrectionPerAxisMm = plate.MaxCenteringCorrectionPerAxisMm,
                MaximumCorrectionMoves = plate.MaximumCenteringMoves
            };
        }

        public void ApplyTo(MachineStagingPlate plate)
        {
            if (plate == null) throw new ArgumentNullException(nameof(plate));
            if (!Validate(out var reason)) throw new InvalidOperationException(reason);
            plate.MaxCenteringCorrectionPerAxisMm = MaxCorrectionPerAxisMm;
            plate.MaximumCenteringMoves = MaximumCorrectionMoves;
        }
    }
}
