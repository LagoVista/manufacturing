using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCalibrationViewModel : ViewModelBase, IMachineCalibrationViewModel
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILogger _logger;


        public MachineCalibrationViewModel(IMachineRepo machineRepo, ILogger logger)
        {
            Machine = machineRepo.CurrentMachine.Settings;
            machineRepo.MachineChanged += MachineRepo_MachineChanged;
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void MachineRepo_MachineChanged(object sender, IMachine e)
        {
            Machine = e.Settings;
        }

        public RelayCommand SetToolReferencePointCommand { get; }

        Manufacturing.Models.Machine _machine;
        public Manufacturing.Models.Machine Machine
        {
            set => Set(ref _machine, value);
            get => _machine;
        }
    }
}
