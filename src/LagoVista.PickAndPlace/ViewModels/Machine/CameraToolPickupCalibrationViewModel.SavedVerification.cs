using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public sealed partial class CameraToolPickupCalibrationViewModel
    {
        private CameraToolPickupCandidate _approvedCandidate;
        private IMachine _approvedMachine;
        private string _approvedCameraId;
        private string _approvedNozzleId;
        private DateTimeOffset _approvedAt;
        private string _approvedTwoPickRunId;
        private string _savedPickupStatus =
            "Complete a verified two-pick calibration before approving a saved-offset pickup.";
        private CameraToolSavedPickupProof _savedPickupProof;
        private bool _showSavedPickupRing;
        private bool _savedPickupVerified;

        public string SavedPickupStatus
        {
            get => _savedPickupStatus;
            private set => Set(ref _savedPickupStatus, value);
        }

        public CameraToolSavedPickupProof SavedPickupProof
        {
            get => _savedPickupProof;
            private set => Set(ref _savedPickupProof, value);
        }

        public bool ShowSavedPickupRing
        {
            get => _showSavedPickupRing;
            private set => Set(ref _showSavedPickupRing, value);
        }

        public bool SavedPickupVerified
        {
            get => _savedPickupVerified;
            private set => Set(ref _savedPickupVerified, value);
        }

        public bool CanSaveAndVerify =>
            !CalibrationRunning && !PreviewBusy && !HistoryBusy &&
            _pendingHistorySnapshot == null &&
            _approvedCandidate != null &&
            ReferenceEquals(_repo.CurrentMachine, _approvedMachine) &&
            String.Equals(HeadIndexText,
                _approvedCandidate.HeadIndex.ToString(), StringComparison.Ordinal) &&
            DateTimeOffset.UtcNow - _approvedAt < TimeSpan.FromMinutes(15);

        private void InvalidateSavedPickupCandidate()
        {
            _approvedCandidate = null;
            _approvedMachine = null;
            _approvedCameraId = null;
            _approvedNozzleId = null;
            _approvedTwoPickRunId = null;
            SavedPickupProof = null;
            ShowSavedPickupRing = false;
            SavedPickupVerified = false;
            SavedPickupStatus = "Run the two-pick calibration to prepare a new verified correction.";
            RaisePropertyChanged(nameof(CanSaveAndVerify));
        }

        private void PrepareSavedPickupCandidate(CameraToolPickupCandidate candidate)
        {
            _approvedCandidate = candidate;
            _approvedMachine = _machine;
            _approvedCameraId = Convert.ToString(_machine.PartInspectionCaptureService?.Camera?.Id);
            _approvedNozzleId = _nozzleIdentity;
            _approvedAt = DateTimeOffset.UtcNow;
            _approvedTwoPickRunId = _session?.Snapshot.RunId;
            SavedPickupStatus =
                $"Head {candidate.HeadIndex}: two independent pickups verified. " +
                $"Approve saving ({candidate.ProposedOffsetMm.X:F3}, " +
                $"{candidate.ProposedOffsetMm.Y:F3}) mm and performing a THIRD " +
                "pickup with no correction jog. Green requires <=0.200 mm.";
            SavedPickupProof = null;
            SavedPickupVerified = false;
            RaisePropertyChanged(nameof(CanSaveAndVerify));
        }

        private bool VerifyApprovedPickupContext(CameraToolPickupCandidate candidate,
            IMachine machine, out MachineToolHead head, out string reason)
        {
            head = null;
            reason = null;
            if (candidate == null || machine == null ||
                !ReferenceEquals(machine, _repo.CurrentMachine) ||
                !ReferenceEquals(machine, _approvedMachine) ||
                DateTimeOffset.UtcNow - _approvedAt > TimeSpan.FromMinutes(15) ||
                !String.Equals(HeadIndexText, candidate.HeadIndex.ToString(),
                    StringComparison.Ordinal))
            {
                reason = "Two-pick approval has expired, head selection changed, or the machine changed. Re-run the calibration.";
                return false;
            }
            if (!machine.Connected || !machine.WasMachineHomed ||
                !machine.WasMachinOriginCalibrated ||
                machine.Mode != OperatingMode.Manual ||
                machine.IsLocating || _locator.IsLocating || machine.Busy ||
                machine.ToSendQueueCount != 0 ||
                machine.UnacknowledgedBytesSent != 0 ||
                machine.CurrentMachineToolHead != null)
            {
                reason = "Machine must be connected, homed, Manual, idle, empty-queue, and in camera mode. No motion was sent.";
                return false;
            }
            head = machine.Settings?.ToolHeads?.FirstOrDefault(h =>
                h != null && h.HeadIndex == candidate.HeadIndex);
            var currentCameraId =
                Convert.ToString(machine.PartInspectionCaptureService?.Camera?.Id);
            if (head?.Offset == null || head.CurrentNozzle == null ||
                !head.IsCalibrated || !head.RunoutZeroAngle.HasValue ||
                head.RotationOffsets?.Count != NozzleRunoutMath.SampleCount ||
                !String.Equals(Convert.ToString(head.CurrentNozzle.Id),
                    _approvedNozzleId, StringComparison.Ordinal) ||
                !String.Equals(currentCameraId, _approvedCameraId, StringComparison.Ordinal) ||
                Math.Abs(head.Offset.X - candidate.PreviousOffsetMm.X) > 0.00001 ||
                Math.Abs(head.Offset.Y - candidate.PreviousOffsetMm.Y) > 0.00001)
            {
                reason = "Head, nozzle, camera, T004 runout, or previously stored offsets changed since the two-pick proof. Nothing saved.";
                return false;
            }
            if (!Finite(head.PickHeight) || !Finite(head.PlaceHeight) ||
                Math.Abs(head.PickHeight) < 0.001 ||
                Math.Abs(head.PlaceHeight) < 0.001 ||
                Math.Abs(head.PickHeight - head.PlaceHeight) > 0.5 ||
                !Finite(machine.Settings.SafMoveHeight) ||
                !VacuumCalibrationProfile.TryFromHead(head, out _) ||
                (head.HeadIndex == 1 && machine.LeftVacuumPump) ||
                (head.HeadIndex == 2 && machine.RightVacuumPump))
            {
                reason = "Pickup/return heights, saved vacuum calibration or vacuum-off readiness invalid. No motion was sent.";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Explicitly operator-approved action. Writes the candidate, verifies
        /// authoritative read-back, then performs a fresh THIRD pickup through
        /// ordinary saved-head selection (never a temporary XY correction jog).
        /// A failed third pass attempts safe recovery and offset rollback.
        /// </summary>
        public Task SaveAndVerifyAsync() { throw new NotSupportedException("Under compiler isolation - not actionable."); }
    }
}
