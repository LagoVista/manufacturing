

using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class MachineControlViewModel : ViewModelBase
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILogger _logger;

        public MachineControlViewModel(IMachineRepo machineRepo, ILogger logger) 
        {
            InitCommands();
            //if(machine.Settings != null)
            //{
                XYStepMode = _machineRepo.CurrentMachine.Settings.XYStepMode;
                ZStepMode = _machineRepo.CurrentMachine.Settings.ZStepMode;
            //}
        }      
    }
}
