using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;

namespace LagoVista.PickAndPlace.ViewModels.GCode
{
    public partial class GCodeJobControlViewModel : ViewModelBase
    {
        IMachineRepo _machineRepo;

        public GCodeJobControlViewModel(IMachineRepo machineRepo)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
        }
    }
}
