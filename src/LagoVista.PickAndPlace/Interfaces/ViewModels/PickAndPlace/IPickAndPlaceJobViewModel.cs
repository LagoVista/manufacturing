using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IPickAndPlaceJobViewModel : IViewModel
    {
        ObservableCollection<PickAndPlaceJobSummary> Jobs { get; }
        PickAndPlaceJob Job { get; }
        PickAndPlaceJobSummary SelectedJob { get; set; }
        CircuitBoardRevision CircuitBoard { get; }

        Component CurrentComponent { get; }

        PickAndPlaceJobPart PickAndPlaceJobPart { get; set; }

        IPartsViewModel PartsViewModel { get; }


        RelayCommand SaveCommand { get; }
        RelayCommand ReloadJobCommand { get; }
        RelayCommand RefreshConfigurationPartsCommand { get; }
        RelayCommand SetBoardOriginCommand { get; }
        RelayCommand CheckBoardFiducialsCommand { get; }

        RelayCommand ShowComponentDetailCommand { get; }
        RelayCommand ShowSchematicCommand { get; }

        RelayCommand ResolvePartsCommand { get; }
        RelayCommand ResolveJobCommand { get; }

        RelayCommand ShowDataSheetCommand { get; }
        
    }
}
