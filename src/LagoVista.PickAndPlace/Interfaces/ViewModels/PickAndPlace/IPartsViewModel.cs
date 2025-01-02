using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IPartsViewModel : IViewModel
    {
        Task<InvokeResult> SaveCurrentFeederAsync();
        Task<InvokeResult> RefreshAsync();

        StripFeeder CurrentStripFeeder { get; set; }
        StripFeederRow CurrentStripFeederRow { get; set; }

        AutoFeeder CurrentAutoFeeder { get; set; }
        ComponentPackage CurrentPackage { get;  }
        PlaceableParts CurrentPlaceableParts { get;  }
        Component CurrentComponentToBePlaced { get;  }
        ObservableCollection<PlaceableParts> ConfigurationParts { get; }
        void RefreshConfigurationParts();
        RelayCommand RefreshBoardCommand { get; }

        ObservableCollection<ComponentSummary> Components { get; }
        ObservableCollection<EntityHeader> ComponentCategories { get; }
        EntityHeader SelectedCategory { get; set; }

        ComponentSummary SelectedComponentSummary { get; set; }
        Manufacturing.Models.Component SelectedComponent { get; }

        IMachine Machine { get; }

        ObservableCollection<MachineStagingPlate> StagingPlates { get; }
        ObservableCollection<MachineFeederRail> FeederRails { get; }

        ObservableCollection<StripFeeder> StripFeeders { get; }
        ObservableCollection<AutoFeeder> AutoFeeders { get; }

    }
}
