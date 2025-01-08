using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IStripFeederViewModel : IMachineViewModelBase
    {
        InvokeResult<Point2D<double>> FindLocation(StripFeederLocationTypes moveType);

        ObservableCollection<StripFeeder> Feeders { get; }

        RelayCommand SetFeederOriginCommand { get; }

        RelayCommand GoToFeederOriginCommand { get;  }
        RelayCommand GoToFeederReferenceHoleCommand { get;  }

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
        RelayCommand CancelCommand { get; }
        RelayCommand RefreshTemplatesCommand { get; }
    }
}
