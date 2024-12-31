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
