// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8191c772fe5315e5885eb601a4ff0e8b7bc201badfd7426697d4b0aa6ffa2a27
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
