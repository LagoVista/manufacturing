using LagoVista.Core.Commanding;
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
        PickAndPlaceJob Job { get; set; }

        BoardFiducial SelectedFiducial { get; set; }

        RelayCommand GoToExpectedFiducialCommand { get; }
        RelayCommand GoToActualFiducialCommand { get; }
        RelayCommand AlignFiducialCommand { get; }
        RelayCommand AlignCommand { get; }
    }
}
