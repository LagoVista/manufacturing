// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ffe7a04936476a960d6eebf0cc7271568a7c2a728401d6243aa6fb2d63e7078c
// IndexVersion: 2
// --- END CODE INDEX META ---
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
