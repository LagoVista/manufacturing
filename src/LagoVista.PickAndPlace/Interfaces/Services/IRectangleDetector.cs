// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 296946c8cbfe54093b479d0bac2069631cb071e941a4677126ca92b948c0a096
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface IRectangleDetector<T> where T : class, IDisposable
    {
        void FindRectangles(IMVImage<T> input, MachineCamera camera, System.Drawing.Size size);
        ObservableCollection<MVLocatedRectangle> FoundRectangles { get; }
        void Reset();

        T Image { get; }
    }
}
