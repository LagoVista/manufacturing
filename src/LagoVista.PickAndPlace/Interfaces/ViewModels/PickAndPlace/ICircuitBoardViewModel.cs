using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface ICircuitBoardViewModel : IMachineViewModelBase
    {
        Task<InvokeResult> AlignBoardAsync();
        Task<InvokeResult> InspectPartOnboardAsync(PickAndPlaceJobPart part, PickAndPlaceJobPlacement placement);
        Task<InvokeResult> PlacePartOnboardAsync(PickAndPlaceJobPart part, PickAndPlaceJobPlacement placement);
        Task<InvokeResult> PickPartFromBoardAsync(PickAndPlaceJobPart part, PickAndPlaceJobPlacement placement);
    }
}
