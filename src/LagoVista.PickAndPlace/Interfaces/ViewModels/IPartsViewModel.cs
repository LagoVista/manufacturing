using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IPartsViewModel : IViewModel
    {
        Task<InvokeResult> SaveCurrentFeederAsync();
        Task<InvokeResult> RefreshAsync();

        StripFeeder CurrentStripFeeder { get; }
        AutoFeeder CurrentAutoFeeder { get; }
        ComponentPackage CurrentPackage { get; }
        PlaceableParts CurrentPlaceableParts { get; }
        Component CurrentComponentToBePlaced { get; }
        ObservableCollection<PlaceableParts> ConfigurationParts { get; }
        void RefreshConfigurationParts();
        RelayCommand RefreshBoardCommand { get; }

        IMachine Machine { get; }

        ObservableCollection<MachineStagingPlate> StagingPlates { get; }
        ObservableCollection<MachineFeederRail> FeederRails { get; }

        ObservableCollection<StripFeeder> StripFeeders { get; }
        ObservableCollection<AutoFeeder> AutoFeeders { get; }

    }
}
