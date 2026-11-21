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

        // Time of the most recently acquired, actually processed camera frame.
        // A new fiducial reading must be newer than the last completed machine move.
        System.DateTimeOffset LastProcessedFrameUtc { get; }

        // An active live camera stream is required before an automated
        // calibration procedure is allowed to move the machine.
        bool IsCapturing { get; }

        /// <summary>Starts the configured camera automatically and waits for
        /// a fresh processed frame, or returns an actionable failure.</summary>
        Task<InvokeResult> EnsureCaptureReadyAsync();

        // A fresh, processed position-camera frame with the already-configured
        // part-in-tape rectangle detector's results. Null until a valid frame.
        PocketRectangleFrameObservation LatestRectangleFrame { get; }

        // Set by the selected feeder: true = top-to-bottom barcode, false = left-to-right.
        bool? FeederBarcodeVertical { get; set; }
        RelayCommand StartCaptureCommand { get; }
        RelayCommand StopCaptureCommand { get; }
        RelayCommand CenterFoundItemCommand { get; }

        // Uses the same bounded stable-circle jog as the camera's second Play.
        // Rejects changed detector identity/profile/offset before any motion.
        InvokeResult TryJogToObservedCircle(System.Guid circleId, double expectedOffsetX,
            double expectedOffsetY, string expectedProfileKey);

        // Last real sampled exposure observation, including dark/white image levels.
        // Null until this camera has collected its first sample for the current tune.
        CameraExposureTuneProgress ExposureTuneProgress { get; }

        Task<InvokeResult> AutoTuneExposureAsync();

        // Capture-only tuning: 24 fresh frames, no machine motion or
        // camera-exposure writes; caller persists the verified profile.
        Task<InvokeResult<DarkCircularOpeningLocator.TuneResult>> TuneNozzleOpeningAsync();

        // Returns one fresh processed 10-pixel-band barcode signal.
        Task<InvokeResult<byte[]>> CaptureFeederBarcodeSignalAsync();

        // Start each new physical reference hole with fresh detector history.
        void ResetDetections();

        void StartCapture();

        void StopCapture();
    }
}
