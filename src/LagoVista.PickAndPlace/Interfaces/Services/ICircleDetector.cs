// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b43eeaff753964c57fe6ebccfb0378cc3e32dcac5178926f8892c4ef2b72911e
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface ICircleDetector<T> where T: class, IDisposable
    {
        void FindCircles(IMVImage<T> input, MachineCamera camera, System.Drawing.Size size);
        void Reset();
        MVLocatedCircle FoundCircle { get; }
        ObservableCollection<MVLocatedCircle> FoundCircles { get; }
    }
}
