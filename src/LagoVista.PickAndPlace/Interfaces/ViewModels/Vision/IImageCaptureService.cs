// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 834f9a66773981bc3a6f329e188501f2d231e7b9f200deffbfa5aa3100403ef8
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System.Threading.Tasks;

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

        Task<InvokeResult> AutoTuneExposureAsync();

        // Start each new physical reference hole with fresh detector history.
        void ResetDetections();

        void StartCapture();

        void StopCapture();
    }
}
