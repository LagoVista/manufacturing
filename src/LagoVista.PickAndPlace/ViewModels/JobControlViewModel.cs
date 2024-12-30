using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using System;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class JobControlViewModel : ViewModelBase
    {
        IMachineRepo _machineRepo;

        public JobControlViewModel(IMachineRepo machineRepo)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
        }
    }
}
