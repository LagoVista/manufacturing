using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IMachineCoreActionsViewModel : IMachineViewModelBase
    {
        RelayCommand SkipHomeCommand { get; }
        RelayCommand HomeCommand { get; }
        RelayCommand GoToSafeMoveHeightCommand { get;  }
        RelayCommand MachineVisionOriginCommand { get; }
        RelayCommand GoToPartInspectionCameraCommand { get; }
        RelayCommand SetCameraNavigationCommand { get; }
        RelayCommand<MachineToolHead> SetToolHeadNavigationCommand { get; }
    }
}
