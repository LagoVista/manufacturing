

using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class MachineControlViewModel : ViewModelBase, IMachineControlViewModel
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILogger _logger;

        public MachineControlViewModel(IMachineRepo machineRepo, ILogger logger) 
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitCommands();
            XYStepMode = _machineRepo.CurrentMachine.Settings.XYStepMode;
            ZStepMode = _machineRepo.CurrentMachine.Settings.ZStepMode;
        }      

        public IMachineRepo MachineRepo
        {
            get => _machineRepo;
        }
    }
}
