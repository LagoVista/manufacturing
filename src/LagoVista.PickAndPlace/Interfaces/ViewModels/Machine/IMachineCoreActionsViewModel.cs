using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IMachineCoreActionsViewModel : IMachineViewModelBase
    {
        RelayCommand HomeCommand { get; set; }
        RelayCommand MachineVisionOriginCommand { get; set; }
    }
}
