using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IDryRunViewModel : IMachineViewModelBase
    {
        IJobManagementViewModel JobVM { get; }
        ICircuitBoardViewModel PcbVM { get; }
        IPartInspectionViewModel PartInspectionVM { get; }

        RelayCommand GoToPartOnBoardCommand { get; }

        RelayCommand MoveToPartInFeederCommand { get; }
        RelayCommand PickPartCommand { get; }
        RelayCommand InspectPartCommand { get; }
        RelayCommand RecyclePartCommand { get; }
        RelayCommand InspectPartOnBoardCommand { get; }
        RelayCommand PickPartFromBoardCommand { get; }
        RelayCommand PlacePartCommand { get; }
    }
}
