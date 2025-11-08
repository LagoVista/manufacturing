// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d61f8d7a072b2264522b340b5479949b676101ff626fcaa20d407589347c405d
// IndexVersion: 2
// --- END CODE INDEX META ---
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
