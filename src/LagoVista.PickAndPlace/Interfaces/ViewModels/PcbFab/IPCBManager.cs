// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d15e503f43bce2e4ee2e75c38f0ac3032c9a099e55a3d4f03f877984b5cfb347
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab
{
    public interface IPCBManager : INotifyPropertyChanged
    {
        bool HasBoard { get; }

        bool HasTopEtching { get; }

        bool HasBottomEtching { get; }

        PrintedCircuitBoard Board { get; }

        Task<bool> OpenFileAsync(string boardFile);
        Point2D<double> FirstFiducial { get; set; }

        Point2D<double> SecondFiducial { get; set; }



        bool HasProject { get; }

        PcbMillingProject Project { get; set; }

        string ProjectFilePath { get; set; }

        void SetMeasuredOffset(Point2D<double> offset, double angleDegrees);

        void ClearMeasuredOffset();

        double MeasuredOffsetAngle { get; }

        Point2D<double> MeasuredOffset { get; }

        /// <summary>
        /// If there is an offset and angle stored, apply that to the point, otherwise return original point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        Point2D<double> GetAdjustedPoint(Point2D<double> point);

        bool CameraNavigation { get; set; }

        bool Tool1Navigation { get; set; }

        bool Tool2Navigation { get; set; }

        bool IsSetFiducialMode { get; set; }

        bool IsNavigationMode { get; set; }
    }
}
