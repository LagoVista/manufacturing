// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 13b11f8d8fd50087383a7a4a77e9407c27a1c762ba0df116ee406de3902bcc21
// IndexVersion: 0
// --- END CODE INDEX META ---


using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;

namespace LagoVista.PickAndPlace.ViewModels.Machine
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
