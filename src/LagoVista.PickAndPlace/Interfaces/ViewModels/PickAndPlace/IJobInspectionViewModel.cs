// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 00aef7e1a5d84aaaece08b99833973712fcd5acb1e9493b6fe849c0f83d24b30
// IndexVersion: 2
// --- END CODE INDEX META ---
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
