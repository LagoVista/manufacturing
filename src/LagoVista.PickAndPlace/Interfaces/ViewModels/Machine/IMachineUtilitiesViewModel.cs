using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IMachineUtilitiesViewModel : IViewModel
    {

        ulong RightVacuum { get; }
        ulong LeftVacuum { get; }

        RelayCommand ReadLeftVacuumCommand { get; }
        RelayCommand ReadRightVacuumCommand { get; }

        RelayCommand LeftVacuumOnCommand { get; }
        RelayCommand LeftVacuumOffCommand { get; }
        RelayCommand RightVacuumOnCommand { get; }
        RelayCommand RightVacuumOffCommand { get; }
        RelayCommand TopLightOnCommand { get; }
        RelayCommand TopLightOffCommand { get; }
        RelayCommand BottomLightOnCommand { get; }
        RelayCommand BottomLightOffCommand { get; }

        IMachine Machine { get; }
    }
}
