using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Vision
{
    public interface IImageCaptureService : IMachineViewModelBase
    {
        VisionProfile Profile { get; }
        CameraTypes CameraType { get; set; }
        MachineCamera Camera { get; }
        RelayCommand StartCaptureCommand { get; }
        RelayCommand StopCaptureCommand { get; }
        RelayCommand CenterFoundItemCommand { get; }

        void StartCapture();

        void StopCapture();
    }
}
