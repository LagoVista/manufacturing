// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9f222c742d06decbbf64afa568370549061c2313c0977be58faddbdd64eb296a
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class VacuumViewModel : MachineViewModelBase, IVacuumViewModel
    {
        public VacuumViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
            ReadVacuumCommand = CreatedMachineConnectedCommand(async () => await ReadVacuumAsync(), () => Machine.CurrentMachineToolHead != null);
            VacuumOnCommand = CreatedMachineConnectedCommand(() => Machine.VacuumPump = true, () => Machine.CurrentMachineToolHead != null);
            VacuumOffCommand = CreatedMachineConnectedCommand(() => Machine.VacuumPump = false, () => Machine.CurrentMachineToolHead != null);
        }

        protected override void MachineChanged(IMachine machine)
        {

            Machine.PropertyChanged += (snd, arg) =>
            {
                if(arg.PropertyName == nameof(IMachine.CurrentMachineToolHead))
                {
                    RaiseCanExecuteChanged();
                }
            };

            base.MachineChanged(machine);
        }

        private long? _vacuum;
        public long? Vacuum
        {
            get => _vacuum;
            set => Set(ref _vacuum, value);
        }

        private double? _aboveThreadhold;
        public double? AboveThreshold
        {
            get => _aboveThreadhold;
            set => Set(ref _aboveThreadhold, value);
        }

        private double? _threadhold;
        public double? Threshold
        {
            get => _threadhold;
            set => Set(ref _threadhold, value);
        }

        double? _percentAboveThreadhold;
        public double? PercentAboveThreshold
        {
            get => _percentAboveThreadhold;
            set => Set(ref _percentAboveThreadhold, value);
        }


        public async Task<InvokeResult> CheckNoPartPresent(Component component, int timeoutMS)
        {
            if (Machine.CurrentMachineToolHead == null)
            {
                ToolHead = null;
                return InvokeResult.FromError("No current tool head selected.");
            }

            if (component == null)
            {
                Component = null;
                return InvokeResult.FromError("Component not selected/provided.");
            }

            ToolHead = Machine.CurrentMachineToolHead;
            Component = component;

            Vacuum = null;

            var done = false;
            var timeout = DateTime.Now.AddMilliseconds(timeoutMS);
            var calibrated = VacuumCalibrationProfile.TryFromHead(ToolHead, out var profile);
            Threshold = calibrated ? profile.ThresholdRaw :
                ToolHead.NoPartPickedVacuum * (1 + ToolHead.PercentAboveNoPartPicked / 100.0);

            while (!done && DateTime.Now < timeout)
            {
                var result = await Machine.ReadVacuumAsync();
                if (result.Successful)
                {
                    Vacuum = result.Result;
                    if (calibrated)
                    {
                        AboveThreshold = result.Result - Threshold;
                        PercentAboveThreshold = profile.Normalize(result.Result);
                    }
                    else
                    {
                        AboveThreshold = result.Result - ToolHead.NoPartPickedVacuum;
                        PercentAboveThreshold = ToolHead.NoPartPickedVacuum == 0 ? null :
                            (AboveThreshold / ToolHead.NoPartPickedVacuum) * 100.0;
                    }

                    if (calibrated
                        ? profile.Classify(result.Result) == VacuumPickupClassification.NoPart
                        : Vacuum < Threshold)
                    {
                        return InvokeResult.Success;

                    }
                }

                await Task.Delay(5);
            }

            if (Vacuum != null)
                return InvokeResult.FromError(calibrated
                    ? $"No-part check failed: normalized {profile.Normalize(Vacuum.Value):F1}% at threshold {profile.ThresholdPercent:F1}%."
                    : $"Part was Detected, it should not be. Pressure Detected {Vacuum}, Expected {Threshold} .");


            return InvokeResult.FromError($"Could not read vacuum.");
        }

        public async Task<InvokeResult> CheckPartPresent(Component component, int timeoutMS, ulong? vacuumOverride)
        {
            var sw = Stopwatch.StartNew();
            Machine.DebugWriteLine("-----------------------------");
            Machine.DebugWriteLine($"[CheckPartPresent] - Start");
            if (Machine.CurrentMachineToolHead == null)
            {
                ToolHead = null;
                return InvokeResult.FromError("No current tool head selected.");
            }

            if (component == null)
            {
                Component = null;
                return InvokeResult.FromError("Component not selected/provided.");
            }

            ToolHead = Machine.CurrentMachineToolHead;
            Component = component;

            Vacuum = null;

            var done = false;
            var timeout = DateTime.Now.AddMilliseconds(timeoutMS);
            var calibrated = VacuumCalibrationProfile.TryFromHead(ToolHead, out var profile);
            Threshold = calibrated ? profile.ThresholdRaw :
                ToolHead.NoPartPickedVacuum * (1 + ToolHead.PercentAboveNoPartPicked / 100.0);

            while (!done && DateTime.Now < timeout)
            {
                var result = await Machine.ReadVacuumAsync();
                if (result.Successful)
                {
                    Vacuum = result.Result;
                    if (calibrated)
                    {
                        AboveThreshold = result.Result - Threshold;
                        PercentAboveThreshold = profile.Normalize(result.Result);
                    }
                    else
                    {
                        AboveThreshold = result.Result - ToolHead.NoPartPickedVacuum;
                        PercentAboveThreshold = ToolHead.NoPartPickedVacuum == 0 ? null :
                            (AboveThreshold / ToolHead.NoPartPickedVacuum) * 100.0;
                    }

                    if (calibrated
                        ? profile.Classify(result.Result) == VacuumPickupClassification.PartPresent
                        : Vacuum > Threshold)
                    {
                        Machine.DebugWriteLine($"[CheckPartPresent] Success in {sw.Elapsed.TotalMilliseconds} ms ");
                        Machine.DebugWriteLine("-----------------------------");
                        return InvokeResult.Success;
                    }
                }

                await Task.Delay(5);
            }

            if (Vacuum != null)
            {

                Machine.DebugWriteLine("-----------------------------");
                return InvokeResult.FromError(calibrated
                    ? $"Part-presence check failed: normalized {profile.Normalize(Vacuum.Value):F1}% at threshold {profile.ThresholdPercent:F1}%."
                    : $"Part Not Detected Pressure Detected {Vacuum}, Expected {Threshold}.");
            }


            Machine.DebugWriteLine("-----------------------------");
            return InvokeResult.FromError($"Could not read vacuum.");
        }

        Component _component;
        public Component Component
        {
            get => _component;
            set => Set(ref _component, value);
        }

        MachineToolHead _toolHead;
        public MachineToolHead ToolHead
        {
            get => _toolHead;
            set => Set(ref _toolHead, value);
        }

        public async Task<InvokeResult<long>> ReadVacuumAsync()
        {
            if (Machine.CurrentMachineToolHead != null)
                return await Machine.ReadVacuumAsync();

            return InvokeResult<long>.FromError("No current tool head, could not read vacuum.");
        }

        public RelayCommand ReadVacuumCommand { get; }
        public RelayCommand VacuumOnCommand { get; }
        public RelayCommand VacuumOffCommand { get; }
    }
}
