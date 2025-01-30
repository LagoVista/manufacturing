using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface ICircuitBoardViewModel : IMachineViewModelBase
    {
        Task<InvokeResult> AlignBoardAsync();
        Task<InvokeResult> InspectPartOnboardAsync(Component component, PickAndPlaceJobPlacement placement);
        Task<InvokeResult> PlacePartOnboardAsync(Component component, PickAndPlaceJobPlacement placement);
        Task<InvokeResult> PickPartFromBoardAsync(Component component, PickAndPlaceJobPlacement placement);
        Task<InvokeResult> GoToPartOnBoardAsync(PickAndPlaceJobPart part, PickAndPlaceJobPlacement placement);

        PickAndPlaceJob Job { get; set; }

        BoardFiducial SelectedFiducial { get; set; }

        RelayCommand GoToExpectedFiducialCommand { get; }
        RelayCommand GoToActualFiducialCommand { get; }
        RelayCommand AlignFiducialCommand { get; }
        RelayCommand AlignCommand { get; }

        double AngleOffset { get; }
        Point2D<double> ScalingError { get; }

        bool IsBoardAligned { get; }
    }
}
