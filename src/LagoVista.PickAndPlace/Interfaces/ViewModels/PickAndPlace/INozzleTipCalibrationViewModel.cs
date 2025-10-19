// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 46b50cbd1c1a85354a4e0fcea8d90709665198e28a685de04d99d5b4a27493f1
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface INozzleTipCalibrationViewModel
    {
        void CircleLocation(Point2D<double> center, double diameter, Point2D<double> stdDeviation);
        Task StartAsync();
        ObservableCollection<string> CalibrationUpdates { get; }
        int TargetAngle { get; }
        int CurrentCalibrationAngle { get; }
        string Status { get; }
    }
}
