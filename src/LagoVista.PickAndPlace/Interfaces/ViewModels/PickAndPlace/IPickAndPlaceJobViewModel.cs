using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IPickAndPlaceJobViewModel : IViewModel
    {
        ObservableCollection<PickAndPlaceJobSummary> Jobs { get; }
        PickAndPlaceJob Job { get; }
        PickAndPlaceJobSummary SelectedJob { get; set; }
        ObservableCollection<PlaceableParts> ConfigurationParts { get; }
        PlaceableParts PlaceablePart { get; set; }
        RelayCommand ReloadJobCommand { get; }
        RelayCommand RefreshConfigurationPartsCommand { get; }
        RelayCommand SaveCommand { get; }
        CircuitBoardRevision CircuitBoard { get; }
    }
}
