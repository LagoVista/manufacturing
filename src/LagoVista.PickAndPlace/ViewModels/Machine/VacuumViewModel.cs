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

        private double? _errorValue;
        public double? ErrorValue
        {
            get => _errorValue;
            set => Set(ref _errorValue, value);
        }


        double? _percentError;
        public double? PercentError
        {
            get => _percentError;
            set => Set(ref _percentError, value);
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

            long? lastVacuum = null;

            var done = false;
            var timeout = DateTime.Now.AddMilliseconds(timeoutMS);
            while (!done && DateTime.Now < timeout)
            {
                var result = await Machine.ReadVacuumAsync();
                if (result.Successful)
                {
                    lastVacuum = result.Result;

                    var err = Machine.CurrentMachineToolHead.PartPickedVacuum - result.Result;
                    var delta = Math.Abs(err);
                    var percentError = delta / Machine.CurrentMachineToolHead.PartPickedVacuum * 100;
                    if (percentError > Machine.CurrentMachineToolHead.VacuumTolerancePercent)
                    {
                        Vacuum = lastVacuum;
                        ErrorValue = err;
                        return InvokeResult.Success;

                    }
                }

                await Task.Delay(5);
            }

            if (Vacuum != null)
                return InvokeResult.FromError($"Part was Detected, it should not be. Pressure Detected {lastVacuum}, Expected {Machine.CurrentMachineToolHead.NoPartPickedVacuum} .");


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

            long? lastVacuum = null;

            PercentError = null;
            Vacuum = null;
            ErrorValue = null;

            double? err = null;
            double? errorPercent = null;

            var done = false;
            var timeout = DateTime.Now.AddMilliseconds(timeoutMS);
            while (!done && DateTime.Now < timeout)
            {
                var result = await Machine.ReadVacuumAsync();
                if (result.Successful)
                {
                    lastVacuum = result.Result;

                    if (lastVacuum > 50)
                    {
                        err = Machine.CurrentMachineToolHead.PartPickedVacuum - result.Result;
                        var delta = Math.Abs(err.Value);
                        errorPercent = Math.Round(delta / Machine.CurrentMachineToolHead.PartPickedVacuum * 100, 2);
                        if (errorPercent < 25)// Machine.CurrentMachineToolHead.VacuumTolerancePercent)
                        {
                            ErrorValue = Math.Round(err.Value);
                            PercentError = errorPercent;
                            Vacuum = lastVacuum;
                            return InvokeResult.Success;
                        }
                    }
                }

                await Task.Delay(5);
            }

            if (err.HasValue)
                ErrorValue = Math.Round(err.Value, 2);
            else
                ErrorValue = null;
            PercentError = errorPercent;
            Vacuum = lastVacuum;


            if (Vacuum != null)
                return InvokeResult.FromError($"Part Not Detected Pressure Detected {lastVacuum}, Expected {Machine.CurrentMachineToolHead.PartPickedVacuum}.");

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
