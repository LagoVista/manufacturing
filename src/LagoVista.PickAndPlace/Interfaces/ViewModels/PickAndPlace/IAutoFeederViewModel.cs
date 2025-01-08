using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using System.Collections;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IAutoFeederViewModel : IMachineViewModelBase
    {
        ObservableCollection<MachineFeederRail> FeederRails { get; }
        ObservableCollection<AutoFeeder> Feeders { get; }

        string SelectedTemplateId { get; set; }
        ObservableCollection<AutoFeederTemplate> Templates { get;  }

        AutoFeeder Current { get; set; }
        
        RelayCommand CreateAutoFeederFromTemplateCommand { get; }
        RelayCommand SetPartPickLocationCommand { get; }
        RelayCommand SetFeederFiducialLocationCommand { get; }


        RelayCommand AddCommand { get; }
        RelayCommand SaveCommand { get; }
        RelayCommand CancelCommand { get; }
        RelayCommand RefreshTemplatesCommand { get; }

        RelayCommand AdvancePartCommand { get; }
        RelayCommand GoToPickLocationCommand { get; }
        RelayCommand GoToFiducialCommand { get; }
    }
}
