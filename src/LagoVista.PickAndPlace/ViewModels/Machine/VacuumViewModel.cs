using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
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
            Threshold = Machine.CurrentMachineToolHead.NoPartPickedVacuum *( Machine.CurrentMachineToolHead.PercentAboveNoPartPicked / 100.0) + Machine.CurrentMachineToolHead.NoPartPickedVacuum;

            while (!done && DateTime.Now < timeout)
            {
                var result = await Machine.ReadVacuumAsync();
                if (result.Successful)
                {
                    Vacuum = result.Result;
                    AboveThreshold = result.Result - Machine.CurrentMachineToolHead.NoPartPickedVacuum;
                    PercentAboveThreshold = (AboveThreshold / Machine.CurrentMachineToolHead.NoPartPickedVacuum) * 100.0;

                    if (Vacuum < Threshold)
                    {
                        return InvokeResult.Success;

                    }
                }

                await Task.Delay(5);
            }

            if (Vacuum != null)
                return InvokeResult.FromError($"Part was Detected, it should not be. Pressure Detected {Vacuum}, Expected {Threshold} .");


            return InvokeResult.FromError($"Could not read vacuum.");
        }

        public async Task<InvokeResult> CheckPartPresent(Component component, int timeoutMS, ulong? vacuumOverride)
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
            Threshold = Machine.CurrentMachineToolHead.NoPartPickedVacuum * (Machine.CurrentMachineToolHead.PercentAboveNoPartPicked / 100.0) + Machine.CurrentMachineToolHead.NoPartPickedVacuum;

            while (!done && DateTime.Now < timeout)
            {
                var result = await Machine.ReadVacuumAsync();
                if (result.Successful)
                {
                    Vacuum = result.Result;
                    AboveThreshold = result.Result - Machine.CurrentMachineToolHead.NoPartPickedVacuum;
                    PercentAboveThreshold = (AboveThreshold / Machine.CurrentMachineToolHead.NoPartPickedVacuum) * 100.0;

                    if (Vacuum > Threshold)
                    {
                        return InvokeResult.Success;

                    }
                }

                await Task.Delay(5);
            }

            if (Vacuum != null)
                return InvokeResult.FromError($"Part Not Detected Pressure Detected {Vacuum}, Expected {Threshold}.");

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
