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
