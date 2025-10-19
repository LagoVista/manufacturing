// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 834f9a66773981bc3a6f329e188501f2d231e7b9f200deffbfa5aa3100403ef8
// IndexVersion: 0
// --- END CODE INDEX META ---
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
