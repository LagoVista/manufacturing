using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface ICircleDetector<T> where T: class, IDisposable
    {
        void FindCircles(IMVImage<T> input, MachineCamera camera, System.Drawing.Size size);
        void Reset();
        MVLocatedCircle FoundCircle { get; }
        List<MVLocatedCircle> FoundCircles { get; }
    }
}
