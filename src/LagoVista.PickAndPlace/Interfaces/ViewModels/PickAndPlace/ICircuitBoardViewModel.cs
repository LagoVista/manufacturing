// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7c61c12ad044463f86aa521e9373e644ab5651d8c8cbb4fe8bd52f1305664167
// IndexVersion: 0
// --- END CODE INDEX META ---
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
        Task<InvokeResult> GoToPartOnBoardAsync(PartsGroup part, PickAndPlaceJobPlacement placement);

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
