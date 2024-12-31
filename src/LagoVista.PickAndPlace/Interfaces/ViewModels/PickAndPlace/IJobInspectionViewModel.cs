using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IJobInspectionViewModel : IViewModel
    {
        string Status { get; }
        PickAndPlaceJob Job { get; set; }

        PlaceableParts Current { get; set; }

        RelayCommand FirstInspectCommand { get; }
        RelayCommand NextInspectCommand { get; }
        RelayCommand PrevInspectCommand { get; }

        RelayCommand GoToInspectPartRefHoleCommand { get; }
        RelayCommand SetInspectPartRefHoleCommand { get; }
        RelayCommand GoToInspectedPartCommand { get; }
    }
}
