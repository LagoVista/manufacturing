using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface IShapeDetectorService<T> where T : class, IDisposable
    {
        T PerformShapeDetection(IMVImage<T> image, MachineCamera camera, Size size);

        IEnumerable<MVLocatedCircle> FoundCircles { get; }
        IEnumerable<MVLocatedRectangle> FoundRectangles { get; }
        IEnumerable<MVLocatedCorner> FoundCorners { get; }
    }
}
