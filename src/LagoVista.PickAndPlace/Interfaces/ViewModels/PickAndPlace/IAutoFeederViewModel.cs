using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IAutoFeederViewModel : IFeederViewModel
    {
        ObservableCollection<MachineFeederRail> FeederRails { get; }
        ObservableCollection<AutoFeeder> Feeders { get; }

        string SelectedTemplateId { get; set; }
        ObservableCollection<AutoFeederTemplate> Templates { get;  }

        AutoFeeder Current { get; set; }

        Task InitializeFeederAsync();
        Task AdvanceFeed();
        Task RetractFeed();

        RelayCommand CreateAutoFeederFromTemplateCommand { get; }
        RelayCommand SetPartPickLocationCommand { get; }
        RelayCommand SetFeederFiducialLocationCommand { get; }

        InvokeResult<Point2D<double>> FindFeederFiducial();
        InvokeResult<Point2D<double>> FindPickLocation();

        InvokeResult<Point2D<double>> FindFeederFiducial(string autoFeederId);
        InvokeResult<Point2D<double>> FindPickLocation(string autoFeederId);


        RelayCommand AddCommand { get; }
        RelayCommand SaveCommand { get; }
        RelayCommand CancelCommand { get; }
        RelayCommand RefreshTemplatesCommand { get; }

        RelayCommand InitializeFeederCommand { get; }
        RelayCommand AdvanceFeedCommand { get; }
        RelayCommand RetractFeedCommand { get; }
        RelayCommand GoToPickLocationCommand { get; }
        RelayCommand GoToFiducialCommand { get; }
    }
}
