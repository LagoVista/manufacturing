// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 834f9a66773981bc3a6f329e188501f2d231e7b9f200deffbfa5aa3100403ef8
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System.Threading.Tasks;
using System.Drawing;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Vision
{
    public interface IImageCaptureService : IMachineViewModelBase
    {
        VisionProfile Profile { get; }
        CameraTypes CameraType { get; set; }
        MachineCamera Camera { get; }

        // Latest processed frame dimensions; unlike Camera.ImageSize, this reflects
        // the actual capture resolution selected by the Windows camera driver.
        Size CaptureFrameSize { get; }

        // Set by the selected feeder: true = top-to-bottom barcode, false = left-to-right.
        bool? FeederBarcodeVertical { get; set; }
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
