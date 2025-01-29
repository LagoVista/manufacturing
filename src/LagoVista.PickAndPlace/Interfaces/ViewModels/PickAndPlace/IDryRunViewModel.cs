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

        string LastStatus { get; }
        bool LastActionSuccess { get; }

        RelayCommand GoToPartOnBoardCommand { get; }

        RelayCommand MoveToPartInFeederCommand { get; }
        RelayCommand PickPartCommand { get; }
        RelayCommand InspectPartCommand { get; }
        RelayCommand RecyclePartCommand { get; }
        RelayCommand InspectPartOnBoardCommand { get; }
        RelayCommand PickPartFromBoardCommand { get; }
        RelayCommand PlacePartCommand { get; }

        RelayCommand RotatePartCommand { get; }
        RelayCommand RotateBackPartCommand { get; }


        RelayCommand CheckPartPresentCommand { get; }
        RelayCommand CheckNoPartPresentCommand { get; }

        RelayCommand ClonePartInTapeVisionProfileCommand { get; }
        RelayCommand ClonePartOnBoardVisionProfileCommand { get; }
        RelayCommand ClonePartInspectionVisionProfileCommand { get; }
    }
}
