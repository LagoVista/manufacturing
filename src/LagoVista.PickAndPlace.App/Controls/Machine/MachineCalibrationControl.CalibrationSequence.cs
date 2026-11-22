using LagoVista.Core.IOC;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.ViewModels.Machine;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// One operator click chains the existing independent calibration windows.
    /// Every procedure still owns hardware motion, persistence, and safety gates.
    /// A failed stage remains visible for diagnosis; no later stage is started.
    /// Vacuum calibration is explicitly excluded from this sequence.
    /// </summary>
    public partial class MachineCalibrationControl
    {
        private bool _allCalibrationsRunning;

        private void SetCalibrationSequenceStatus(string message)
        {
            AllCalibrationSequenceStatus.Text = message;
        }

        private static bool StepsPassed(System.Collections.Generic.IEnumerable<string> states)
        {
            var steps = states.ToList();
            return steps.Count > 0 && steps.All(state => state == "Passed");
        }

        private bool VerifySequenceMachine(IMachine original, out string reason)
        {
            var machine = ViewModel?.Machine;
            if (machine == null || !ReferenceEquals(machine, original) ||
                !machine.Connected || !machine.WasMachineHomed ||
                machine.Mode != OperatingMode.Manual ||
                machine.IsLocating || machine.Busy ||
                machine.ToSendQueueCount > 0 ||
                machine.UnacknowledgedBytesSent > 0)
            {
                reason = "Machine identity, homing, Manual mode, controller idle, " +
                    "or command-queue readiness changed. No further calibration motion sent.";
                return false;
            }
            reason = null;
            return true;
        }

        /// <summary>
        /// Show the actual existing operation dialog, start it exactly once on
        /// Loaded, and close it automatically only after verified success.
        /// ShowDialog keeps the owner UI blocked but the camera/progress UI live.
        /// An incomplete or failed run leaves its window open for investigation.
        /// </summary>
        private bool RunSequenceWindow(Window window, string stageName,
            Func<Task> execute, Func<bool> verified, Func<string> failure)
        {
            var started = false;
            var accepted = false;
            window.Owner = Window.GetWindow(this);
            window.Loaded += async (sender, args) =>
            {
                if (started)
                    return;
                started = true;
                try
                {
                    await execute();
                    accepted = verified();
                    if (accepted)
                    {
                        window.Close();
                    }
                    else
                    {
                        SetCalibrationSequenceStatus(stageName + " stopped: " +
                            (failure() ?? "The required verified completion evidence was missing.") +
                            " Close this window after reviewing the failure; later stages will not run.");
                    }
                }
                catch (Exception ex)
                {
                    SetCalibrationSequenceStatus(stageName + " stopped: " + ex.Message +
                        " Review machine state and close this window; later stages will not run.");
                }
            };
            window.ShowDialog();
            return accepted;
        }

        private void RunAllCalibrations_Click(object sender, RoutedEventArgs e)
        {
            if (_allCalibrationsRunning)
                return;

            var machine = ViewModel?.Machine;
            if (machine == null || !VerifySequenceMachine(machine, out var readiness))
            {
                SetCalibrationSequenceStatus(readiness ??
                    "No connected machine was available.");
                return;
            }
            if (_vacuumWindow != null)
            {
                SetCalibrationSequenceStatus(
                    "Close the separate vacuum-calibration window before starting this sequence.");
                return;
            }

            var settings = ViewModel.MachineConfiguration;
            var plates = settings?.StagingPlates?.ToList();
            var installed = settings?.ToolHeads?
                .Where(head => head != null && head.CurrentNozzle != null)
                .OrderBy(head => head.HeadIndex).ToList();
            if (plates == null || plates.Count == 0)
            {
                SetCalibrationSequenceStatus(
                    "No staging plates are configured. The full sequence cannot be verified.");
                return;
            }
            if (installed == null || installed.Count == 0 ||
                installed.Any(head => head.HeadIndex != 1 && head.HeadIndex != 2))
            {
                SetCalibrationSequenceStatus(
                    "The full sequence requires one or two supported installed nozzle heads.");
                return;
            }

            // T008 requires the actual active position-camera configuration.
            var positionCamera = settings.Cameras?.FirstOrDefault(
                camera => camera.CameraType?.Value == CameraTypes.Position);
            if (positionCamera == null ||
                !ReferenceEquals(machine.PositionImageCaptureService?.Camera, positionCamera))
            {
                SetCalibrationSequenceStatus(
                    "The active position camera must match a configured camera. No motion started.");
                return;
            }

            _allCalibrationsRunning = true;
            AllCalibrationSequenceButton.IsEnabled = false;
            var count = 3 + plates.Count + installed.Count;
            var step = 0;
            try
            {
                ViewModel.SelectedMachineCamera = positionCamera;
                string reason;
                bool Next(string label, Func<bool> run)
                {
                    step++;
                    if (!VerifySequenceMachine(machine, out reason))
                    {
                        SetCalibrationSequenceStatus("Stopped before " + label + ": " + reason);
                        return false;
                    }
                    SetCalibrationSequenceStatus("Running " + step + " of " + count + ": " +
                        label + ". Supervise the machine; the window closes on verified success.");
                    if (!run())
                        return false;
                    if (!VerifySequenceMachine(machine, out reason))
                    {
                        SetCalibrationSequenceStatus(label + " completed, but stopped before " +
                            "the next stage: " + reason);
                        return false;
                    }
                    SetCalibrationSequenceStatus("Verified " + step + " of " + count + ": " + label);
                    return true;
                }

                if (!Next("Camera exposure", () =>
                    RunSequenceWindow(new CameraExposureCalibrationWindow(ViewModel),
                        "Camera exposure",
                        () => ViewModel.RunExposureCalibrationAsync(),
                        () => StepsPassed(ViewModel.ExposureSteps.Select(s => s.Status)),
                        () => ViewModel.ExposureCalibrationStatus)))
                    return;

                var fiducial = new MachineFiducialCalibrationWindow();
                var fiducialVm = fiducial.DataContext as MachineFiducialCalibrationViewModel;
                if (fiducialVm == null ||
                    !Next("Machine fiducial", () =>
                        RunSequenceWindow(fiducial, "Machine fiducial",
                            () => fiducialVm.RunAsync(),
                            () => machine.WasMachinOriginCalibrated &&
                                StepsPassed(fiducialVm.Steps.Select(s => s.Status)),
                            () => fiducialVm.Status)))
                    return;

                foreach (var plate in plates)
                {
                    var dialog = new StagingPlateCalibrationWindow(plate);
                    var vm = dialog.DataContext as StagingPlateCalibrationViewModel;
                    if (vm == null ||
                        !Next("Staging plate: " + plate.Name, () =>
                            RunSequenceWindow(dialog, "Staging plate: " + plate.Name,
                                () => vm.RunAsync(),
                                () => plate.IsCalibrated &&
                                    ReferenceEquals(vm.SelectedPlate, plate) &&
                                    StepsPassed(vm.Steps.Select(s => s.Status)),
                                () => vm.Status)))
                        return;
                }

                var nozzleVm = SLWIOC.Get<INozzleTipCalibrationViewModel>();
                var nozzleDialog = new NozzleRunoutCalibrationWindow
                {
                    DataContext = nozzleVm
                };
                if (!Next("Installed nozzle runout", () =>
                    RunSequenceWindow(nozzleDialog, "Installed nozzle runout",
                        () => nozzleVm.StartAsync(),
                        () => nozzleVm.RunoutFinished &&
                            !nozzleVm.RunoutRunning &&
                            nozzleVm.CompletedRunoutSummaries.Count == installed.Count &&
                            installed.All(head => head.IsCalibrated) &&
                            StepsPassed(nozzleVm.RunoutSteps.Select(s => s.Status)),
                        () => nozzleVm.Status)))
                    return;

                foreach (var head in installed)
                {
                    var vm = SLWIOC.Get<ICameraToolPickupCalibrationViewModel>();
                    vm.HeadIndexText = head.HeadIndex.ToString();
                    vm.SaveVerifiedOffset = false;
                    var dialog = new CameraToolPickupCalibrationWindow
                    {
                        DataContext = vm
                    };
                    if (!Next("Camera-to-tool XY: Head " + head.HeadIndex, () =>
                        RunSequenceWindow(dialog,
                            "Camera-to-tool XY: Head " + head.HeadIndex,
                            () => vm.StartAsync(),
                            () => vm.CalibrationFinished && !vm.CalibrationRunning &&
                                !vm.SaveVerifiedOffset &&
                                vm.CalibrationSteps.Count == 16 &&
                                vm.CalibrationSteps.All(s => s.StepKey.EndsWith(
                                    "/commit-offset", StringComparison.Ordinal)
                                    ? s.Status == "Unsaved" : s.Status == "Passed"),
                            () => vm.Status)))
                        return;
                }

                SetCalibrationSequenceStatus(
                    "All " + count + " calibration stages VERIFIED. Vacuum calibration was not " +
                    "run. Camera-to-tool XY candidates remain UNSAVED for supervised hardware acceptance.");
            }
            catch (Exception ex)
            {
                SetCalibrationSequenceStatus(
                    "Sequence stopped: " + ex.Message + ". No further stages started.");
            }
            finally
            {
                _allCalibrationsRunning = false;
                AllCalibrationSequenceButton.IsEnabled = true;
            }
        }
    }
}
