using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IMachineUtilitiesViewModel : IViewModel
    {

        long RightVacuum { get; }
        long LeftVacuum { get; }

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

        Task<long> ReadVacuumAsync();
        Task<long> ReadLeftVacuumAsync();
        Task<long> ReadRightVacuumAsync();
    }
}
