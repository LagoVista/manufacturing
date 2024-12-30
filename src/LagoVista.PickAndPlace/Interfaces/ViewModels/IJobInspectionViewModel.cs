using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IJobInspectionViewModel
    {
        string Status { get; }
        PickAndPlaceJob Job { get; set; }

        PlaceableParts Current { get; set; }

        RelayCommand FirstInspectCommand { get; }
        RelayCommand NextInspectCommand { get; }
        RelayCommand PrevInspectCommand { get; }

        RelayCommand GoToInspectPartRefHoleCommand { get;}
        RelayCommand SetInspectPartRefHoleCommand { get;  }
        RelayCommand GoToInspectedPartCommand { get; }
    }
}
