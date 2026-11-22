using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System;
using System.Threading.Tasks;
namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public sealed partial class CameraToolPickupCalibrationViewModel
    {
        private CameraToolPickupCandidate _approvedCandidate;
        private IMachine _approvedMachine;
        private DateTimeOffset _approvedAt;
        private string _savedPickupStatus;
        private CameraToolSavedPickupProof _savedPickupProof;
        private bool _savedPickupVerified;
        private bool _showSavedPickupRing;
        public string SavedPickupStatus { get => _savedPickupStatus; private set => Set(ref _savedPickupStatus, value); }
        public CameraToolSavedPickupProof SavedPickupProof { get => _savedPickupProof; private set => Set(ref _savedPickupProof, value); }
        public bool SavedPickupVerified { get => _savedPickupVerified; private set => Set(ref _savedPickupVerified, value); }
        public bool ShowSavedPickupRing { get => _showSavedPickupRing; private set => Set(ref _showSavedPickupRing, value); }
        public bool CanSaveAndVerify => _approvedCandidate != null && !CalibrationRunning;
        private void InvalidateSavedPickupCandidate() { _approvedCandidate = null; SavedPickupVerified=false; }
        private void PrepareSavedPickupCandidate(CameraToolPickupCandidate candidate) { _approvedCandidate=candidate; }
        public Task SaveAndVerifyAsync() { throw new NotSupportedException("Diagnostic isolation; no machine motion or save enabled."); }
    }
}
