using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IStripFeederViewModel : IFeederViewModel
    {
        InvokeResult<Point2D<double>> FindLocation(StripFeederLocationTypes moveType);

        int CurrentPartIndex { get; }
        int TotalPartsInFeederRow { get; }
        int AvailablePartsInFeederRow { get; }
        string SelectedTemplateId { get; }

        StripFeeder Current { get; set; }
        StripFeederRow CurrentRow { get; set; }
        ObservableCollection<MachineStagingPlate> StagingPlates { get; }
        ObservableCollection<StripFeederTemplate> Templates { get; }

        ObservableCollection<StripFeeder> Feeders { get; }

        RelayCommand SetFeederOriginCommand { get; }

        RelayCommand GoToFeederOriginCommand { get; }
        RelayCommand GoToFeederReferenceHoleCommand { get; }

        RelayCommand SetFirstFeederReferenceHoleCommand { get; }
        RelayCommand SetLastFeederReferenceHoleCommand { get; }
        RelayCommand GoToFirstFeederReferenceHoleCommand { get; }
        RelayCommand GoToLastFeederReferenceHoleCommand { get; }

        RelayCommand GoToFirstPartCommand { get; }
        RelayCommand GoToLastPartCommand { get; }
        RelayCommand GoToCurrentPartCommand { get; }
        RelayCommand GoToNextPartCommand { get; }
        RelayCommand GoToPreviousPartCommand { get; }

        RelayCommand AddCommand { get; }
        RelayCommand SaveCommand { get; }
        RelayCommand SaveAndCloseCommand { get; }
        RelayCommand CancelCommand { get; }
        RelayCommand DoneRowCommand { get; }
        RelayCommand RefreshTemplatesCommand { get; }
        RelayCommand SetComponentCommand {get;}
    }
}
