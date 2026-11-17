using System;
using System.Collections.Generic;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// In-session association between a physical installed nozzle and a freshly
    /// tuned dark opening. Camera-owned Nozzle profiles are shared across heads:
    /// a saved diameter has no evidence of which nozzle it describes.
    /// Unknown heads acquire without that stale constraint; explicit profile
    /// tuning for the selected head restores the strict diameter lock.
    /// Nothing is written to the machine or persisted by this class.
    /// </summary>
    public sealed class NozzleOpeningHeadContext
    {
        private sealed class TunedOpening
        {
            public double DiameterMm;
            public double PixelsPerMm;
        }

        private readonly Dictionary<string, TunedOpening> _perNozzle =
            new Dictionary<string, TunedOpening>(StringComparer.Ordinal);
        private string _activeNozzle;
        private VisionProfile _observedProfile;
        private double _observedDiameter;
        private double? _observedScale;
        private bool _initialized;

        /// <summary>
        /// Resolve an expected opening for the selected physical nozzle.
        /// A head switch or a different profile never attributes the shared
        /// profile's existing diameter to the newly selected head.
        /// Only a fresh change of the profile's tuning values while this head
        /// remains selected establishes its in-session association.
        /// </summary>
        /// <summary>
        /// Explicitly associate a proven 24-frame image tune with the nozzle
        /// that was selected for the entire acquisition. Do not infer a new
        /// tune from incidental values left in a shared camera profile.
        /// </summary>
        public void RegisterVerifiedTune(string nozzleIdentity,
            double diameterMm, double pixelsPerMm)
        {
            if (String.IsNullOrWhiteSpace(nozzleIdentity) ||
                Double.IsNaN(diameterMm) || Double.IsInfinity(diameterMm) ||
                diameterMm < DarkCircularOpeningLocator.MinimumOpeningDiameterMm ||
                diameterMm > DarkCircularOpeningLocator.MaximumOpeningDiameterMm ||
                Double.IsNaN(pixelsPerMm) || Double.IsInfinity(pixelsPerMm) ||
                pixelsPerMm <= 0)
                return;

            _perNozzle[nozzleIdentity] = new TunedOpening {
                DiameterMm = diameterMm, PixelsPerMm = pixelsPerMm
            };
        }

        public double Resolve(string nozzleIdentity, VisionProfile profile,
            double currentPixelsPerMm, out bool selectionChanged)
        {
            selectionChanged = !_initialized ||
                !String.Equals(_activeNozzle, nozzleIdentity, StringComparison.Ordinal);
            var profileChanged = !ReferenceEquals(_observedProfile, profile);

            if (selectionChanged || profileChanged)
            {
                _initialized = true;
                _activeNozzle = nozzleIdentity;
                _observedProfile = profile;
                _observedDiameter = profile?.ExpectedDarkOpeningDiameterMm ?? 0;
                _observedScale = profile?.DarkOpeningTunedPixelsPerMM;
            }
            else if (profile != null &&
                (_observedDiameter != profile.ExpectedDarkOpeningDiameterMm ||
                 _observedScale != profile.DarkOpeningTunedPixelsPerMM))
            {
                // This change is a new operator/procedure tune for the head
                // actually selected, not an old value inherited on selection.
                _observedDiameter = profile.ExpectedDarkOpeningDiameterMm;
                _observedScale = profile.DarkOpeningTunedPixelsPerMM;
                if (!String.IsNullOrWhiteSpace(nozzleIdentity))
                {
                    var diameter = DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                        _observedDiameter, _observedScale, currentPixelsPerMm);
                    if (diameter > 0)
                        _perNozzle[nozzleIdentity] = new TunedOpening {
                            DiameterMm = diameter, PixelsPerMm = currentPixelsPerMm
                        };
                    else
                        _perNozzle.Remove(nozzleIdentity);
                }
            }

            if (String.IsNullOrWhiteSpace(nozzleIdentity) ||
                !_perNozzle.TryGetValue(nozzleIdentity, out var tuned))
                return 0;

            return DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                tuned.DiameterMm, tuned.PixelsPerMm, currentPixelsPerMm);
        }
    }
}
