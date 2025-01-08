using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IFeederViewModel : IMachineViewModelBase
    {
        string SelectedCategoryKey { get; set; }
        string SelectedComponentSummaryId { get; set; }
        ObservableCollection<EntityHeader> ComponentCategories { get; }
        ObservableCollection<ComponentSummary> Components { get; }
        Component SelectedComponent { get; set; }
        Component CurrentComponent { get; set; }
        ComponentPackage CurrentComponentPackage { get; set; }
        EntityHeader<TapeSizes> TapeSize { get; }
        EntityHeader<TapePitches> TapePitch { get; }
        bool UseCalculated { get; set; }
    }
}
