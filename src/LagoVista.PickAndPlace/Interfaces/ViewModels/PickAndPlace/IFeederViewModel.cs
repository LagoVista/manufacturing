using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IFeederViewModel : IMachineViewModelBase
    {
        string SelectedCategoryKey { get; set; }
        string SelectedComponentSummaryId { get; set; }
        Point2D<double> CurrentPartLocation { get; }
        Component SelectedComponent { get; set; }
        Component CurrentComponent { get; set; }
        bool UseCalculated { get; set; }

        int AvailableParts { get; }

        abstract Task<InvokeResult> NextPartAsync();
  
        Task<InvokeResult> MoveToCurrentPartInFeederAsync();
        Task<InvokeResult> PickCurrentPartAsync();
        Task<InvokeResult> RecycleCurrentPartAsync();
        Task<InvokeResult> InspectCurrentPartAsync();

        Task<InvokeResult> MoveToPartInFeederAsync(Component component);
        Task<InvokeResult> PickPartAsync(Component component);
        Task<InvokeResult> InspectPartAsync(Component component);        
        Task<InvokeResult> RecyclePartAsync(Component component);


        ObservableCollection<EntityHeader> ComponentCategories { get; }
        ObservableCollection<ComponentSummary> Components { get; }
        ComponentPackage CurrentComponentPackage { get; set; }

        EntityHeader<TapeSizes> TapeSize { get; }
        EntityHeader<TapePitches> TapePitch { get; }

        RelayCommand PickCurrentPartCommand { get; }
        RelayCommand InspectCurrentPartCommand { get; }
        RelayCommand RecycleCurrentPartCommand { get; }

        RelayCommand NextPartCommand { get; }
    }
}
