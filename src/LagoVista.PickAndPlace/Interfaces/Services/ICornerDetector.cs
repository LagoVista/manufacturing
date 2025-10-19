// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7192d186dcc2487a2e064946cdcc7575af94437b504b1514e0d2ff252142d31f
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface ICornerDetector<T> where T: class, IDisposable
    {
        void FindCorners(IMVImage<T> input, MachineCamera camera, System.Drawing.Size size);
        void Reset();
        ObservableCollection<MVLocatedCorner> FoundCorners { get; }
    }
}
