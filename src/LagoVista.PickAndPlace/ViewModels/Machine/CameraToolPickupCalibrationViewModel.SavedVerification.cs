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
        public async Task SaveAndVerifyAsync()
        {
            if (!CanSaveAndVerify)
            {
                SavedPickupStatus =
                    "No current verified candidate and recorded two-pick history. Re-run the initial calibration.";
                return;
            }
            var candidate = _approvedCandidate;
            var machine = _repo.CurrentMachine;
            if (!VerifyApprovedPickupContext(candidate, machine, out var head, out var reason))
            {
                SavedPickupStatus = "APPROVAL REFUSED: " + reason;
                return;
            }

            // This candidate is single-use. It must not survive a failed write
            // or a failed third pickup and accidentally be applied twice.
            InvalidateSavedPickupCandidate();
            CalibrationRunning = true;
            CalibrationFinished = false;
            _machine = machine;
            _head = head;
            _nozzleIdentity = Convert.ToString(head.CurrentNozzle.Id);
            _pickupZ = head.PickHeight;
            _returnZ = head.PlaceHeight;
            _possibleDiscReturnTarget = null;
            _possibleDiscReturnCandidate = null;
            _possibleDiscHeld = false;
            _discReturnStarted = false;
            _emptyNozzleOpticalLock = false;
            _emptyNozzleCameraId = null;
            _emptyNozzleCameraScale = 0;
            SavedPickupStatus = "Saving approved offset; then independently verifying actual saved-offset pickup.";
            var oldOffset = new Point2D<double>(
                candidate.PreviousOffsetMm.X, candidate.PreviousOffsetMm.Y);
            var saved = false;
            var successfullyReturned = false;
            var atCameraSafeHeight = false;
            try
            {
                var definition = CameraToolSavedOffsetVerificationProcedure.Create();
                CalibrationSteps = new ObservableCollection<FeederCalibrationFlowStep>(
                    definition.Steps.Select((stage, index) => new FeederCalibrationFlowStep
                    {
                        Number = index + 1,
                        StepKey = $"head-{head.HeadIndex}/{stage.Key}",
                        Title = $"Head {head.HeadIndex} - {stage.Title}"
                    }));
                _session = new MachineOperationSession(definition,
                    new MachineOperationTarget
                    {
                        MachineId = Convert.ToString(machine.Settings.Id),
                        CameraId = Convert.ToString(machine.PartInspectionCaptureService?.Camera?.Id)
                    }, DateTimeOffset.UtcNow);
                _session.Start(DateTimeOffset.UtcNow);
                NewHistorySession(_session);

                Begin("readiness", "Confirming saved-offset pickup readiness and independent operator approval.");
                RequireValid();
                Pass("readiness", $"Explicitly approved candidate from two-pick run. " +
                    $"Original ({oldOffset.X:F3},{oldOffset.Y:F3}), " +
                    $"proposed ({candidate.ProposedOffsetMm.X:F3},{candidate.ProposedOffsetMm.Y:F3}) mm. " +
                    "Third pickup MUST use stored offset and no correction jog.",
                    MachineOperationVerificationKind.MachineObserved);

                Begin("save-offset", "Persisting the operator-approved tool-head offset.");
                head.Offset = new Point2D<double>(
                    candidate.ProposedOffsetMm.X, candidate.ProposedOffsetMm.Y);
                var saveResult = await _repo.SaveCurrentMachineAsync();
                if (!saveResult.Successful)
                {
                    head.Offset = oldOffset;
                    throw new CalibrationStopped("Machine offset save did not acknowledge: " +
                        saveResult.ErrorMessage + ". Inspect authoritative machine settings before retry.");
                }
                saved = true;
                Pass("save-offset", $"Machine PUT acknowledged head {head.HeadIndex} " +
                    $"XY ({head.Offset.X:F3}, {head.Offset.Y:F3}) mm.",
                    MachineOperationVerificationKind.MachineObserved, saved: true);

                Begin("read-back", "Reading saved tool-head offset from Manufacturing before motion.");
                var remote = await _restClient.GetAsync<DetailResponse<Machine>>(
                    $"/api/mfg/machine/{machine.Settings.Id}");
                var remoteHead = remote?.Result?.Model?.ToolHeads?
                    .FirstOrDefault(h => h != null && h.HeadIndex == head.HeadIndex);
                if (remote?.Successful != true || remoteHead?.Offset == null ||
                    Math.Abs(remoteHead.Offset.X - candidate.ProposedOffsetMm.X) > 0.00001 ||
                    Math.Abs(remoteHead.Offset.Y - candidate.ProposedOffsetMm.Y) > 0.00001)
                    throw new CalibrationStopped(
                        "Saved machine offset could not be read back as the approved value; no third pickup sent.");
                Pass("read-back",
                    $"Manufacturing read-back: ({remoteHead.Offset.X:F3}, " +
                    $"{remoteHead.Offset.Y:F3}) mm matches the approved candidate.",
                    MachineOperationVerificationKind.MachineObserved);

                Begin("nozzle-reference", "Re-measuring the empty nozzle center for independent pickup.");
                var nozzle = await MeasureEmptyNozzleAsync(head);
                Pass("nozzle-reference", $"Fresh nozzle X={nozzle.OffsetMm.X:F3}, " +
                    $"Y={nozzle.OffsetMm.Y:F3} mm.",
                    MachineOperationVerificationKind.VisionVerified);

                Begin("target", "Re-locating the returned disc using the top camera.");
                var target = await LocateCylinderAsync();
                Pass("target", $"Fresh disc center X={target.X:F3}, Y={target.Y:F3}.",
                    MachineOperationVerificationKind.VisionVerified);

                Begin("approach", "Moving to disc with the saved head offset; no relative jog.");
                await MoveHeadToCylinderAsync(target, null);
                Pass("approach", "Selected head using SAVED XY offset; no candidate jog issued.",
                    MachineOperationVerificationKind.MachineObserved);

                Begin("pick", "Independent third pickup with saved-offset positioning.");
                await PickCylinderAsync(target, null);
                var pickupMotionUtc = DateTimeOffset.UtcNow;
                Pass("pick", "Fresh vacuum-confirmed pickup, normal stored-offset approach.",
                    MachineOperationVerificationKind.MachineObserved);

                Begin("inspect", "Measuring held disc relative to fresh nozzle reference.");
                var disc = await ObserveHeldCylinderAsync(retainFocusForSavedProof: true);
                var observed = CreateObservation(head, nozzle, disc, pickupMotionUtc);
                Pass("inspect", $"Saved-offset disc minus nozzle: " +
                    $"X={observed.PartRelativeToNozzleMm.X:+0.000;-0.000;0.000}, " +
                    $"Y={observed.PartRelativeToNozzleMm.Y:+0.000;-0.000;0.000} mm. " +
                    $"Disc diameter={disc.DiameterMm:F3} mm.",
                    MachineOperationVerificationKind.VisionVerified);

                Begin("verify", "Checking third-pickup residual against 0.200 mm radial tolerance.");
                var passed = CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                    candidate, observed, DateTimeOffset.UtcNow,
                    out var residual, out reason);
                SavedPickupProof = new CameraToolSavedPickupProof
                {
                    DiscOffsetXmm = disc.OffsetMm.X,
                    DiscOffsetYmm = disc.OffsetMm.Y,
                    NozzleOffsetXmm = nozzle.OffsetMm.X,
                    NozzleOffsetYmm = nozzle.OffsetMm.Y,
                    DiscDiameterMm = disc.DiameterMm,
                    ResidualMm = residual,
                    Verified = passed
                };
                ShowSavedPickupRing = true;
                SavedPickupStatus = passed
                    ? $"GREEN: Head {head.HeadIndex} saved-offset pickup is centered " +
                      $"within {residual:F3} mm (limit 0.200 mm). No temporary jog."
                    : $"RED: Head {head.HeadIndex} saved-offset pickup residual " +
                      $"{residual:F3} mm exceeds 0.200 mm. " + reason;
                // Hold the measured image while the machine remains stationary,
                // so the operator can actually see the green/red disc outline.
                await Task.Delay(1500);
                ShowSavedPickupRing = false;
                machine.SendSafeMoveHeight();
                await NeedIdleAsync("safe Z after focused saved-offset proof");
                if (!passed)
                    throw new CalibrationStopped(reason);
                SavedPickupVerified = true;
                Pass("verify", $"GREEN center verified on THIRD pickup using " +
                    $"SAVED offset: residual {residual:F3} mm <= 0.200 mm.",
                    MachineOperationVerificationKind.VisionVerified);

                Begin("return", "Returning the verified disc with the saved offset.");
                await ReturnCylinderAsync(target, null);
                successfullyReturned = true;
                Pass("return", "Verified disc returned; vacuum OFF and safe retract.",
                    MachineOperationVerificationKind.MachineObserved);

                Begin("safe-camera-mode", "Restoring safe camera navigation with the saved offset.");
                machine.SendSafeMoveHeight();
                await NeedIdleAsync("saved-offset verification safe Z");
                await machine.MoveToCameraAsync();
                await NeedIdleAsync("saved-offset verification camera navigation");
                if (machine.CurrentMachineToolHead != null)
                    throw new CalibrationStopped("Camera navigation not restored.");
                atCameraSafeHeight = true;
                Pass("safe-camera-mode",
                    $"Saved offset confirmed in Manufacturing and Head {head.HeadIndex} " +
                    $"third pickup GREEN, residual {residual:F3} mm. Camera mode restored.",
                    MachineOperationVerificationKind.MachineObserved);

                Status = SavedPickupStatus + " Offset saved and read back.";
            }
            catch (Exception ex)
            {
                ShowSavedPickupRing = false;
                var recovery = await RecoverPossibleDiscAsync();
                if (!_possibleDiscHeld && ValidMachine(out _) &&
                    machine.CurrentMachineToolHead != null)
                {
                    try
                    {
                        machine.SendSafeMoveHeight();
                        await NeedIdleAsync("safe Z after third-pickup failure");
                        await machine.MoveToCameraAsync();
                        await NeedIdleAsync("camera mode after third-pickup failure");
                        atCameraSafeHeight = machine.CurrentMachineToolHead == null;
                    }
                    catch (Exception navigationError)
                    {
                        recovery += " Camera-mode recovery failed: " + navigationError.Message;
                    }
                }
                else if (!_possibleDiscHeld && machine.CurrentMachineToolHead == null &&
                         ValidMachine(out _))
                    atCameraSafeHeight = true;

                string rollback;
                if (saved && !_possibleDiscHeld && atCameraSafeHeight &&
                    ValidMachine(out _))
                {
                    head.Offset = oldOffset;
                    try
                    {
                        var restored = await _repo.SaveCurrentMachineAsync();
                        rollback = restored.Successful
                            ? "Original offset restored in Manufacturing after rejected saved-offset verification."
                            : "ROLLBACK SAVE FAILED: " + restored.ErrorMessage +
                              ". Machine offset requires operator inspection.";
                    }
                    catch (Exception restoreError)
                    {
                        rollback = "ROLLBACK FAILED: " + restoreError.Message +
                                   ". Machine offset requires operator inspection.";
                    }
                }
                else if (saved)
                    rollback = "SAVED OFFSET MAY STILL BE ACTIVE; do not operate until " +
                        "the physical disc and machine settings have been inspected.";
                else
                    rollback = "No confirmed saved-offset change.";

                SavedPickupVerified = false;
                Status = "Saved-offset acceptance FAILED: " + ex.Message +
                    " " + recovery + " " + rollback;
                SavedPickupStatus = "RED / NOT VERIFIED. " + Status;
                machine.AddStatusMessage(StatusMessageTypes.Warning, Status);
                var active = CalibrationSteps.FirstOrDefault(s => s.Status == "Running");
                active?.Fail(Status);
                BlockHistoryStep(Status);
            }
            finally
            {
                ShowSavedPickupRing = false;
                lock (_observationGate) _pendingCircle = null;
                _locator.UnregisterCircleLocatedHandler(this);
                _emptyNozzleOpticalLock = false;
                _emptyNozzleCameraId = null;
                _emptyNozzleCameraScale = 0;
                _head = null;
                _machine = null;
                CalibrationRunning = false;
                CalibrationFinished = true;
                await SaveSettledHistoryAsync();
                _session = null;
                RaisePropertyChanged(nameof(CanSaveAndVerify));
            }
        }
    }
}
