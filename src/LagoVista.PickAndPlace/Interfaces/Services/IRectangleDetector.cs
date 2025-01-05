using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface IRectangleDetector<T> where T : class, IDisposable
    {
        void FindRectangles(IMVImage<T> input, MachineCamera camera, System.Drawing.Size size);
        List<MVLocatedRectangle> FoundRectangles { get; }
        void Reset();

        T Image { get; }
    }
}
