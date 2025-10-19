// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6ab3cb2673bca00cccb08848449780b9b31bb6afbd0ce44fd7aa510a61dd3f0c
// IndexVersion: 0
// --- END CODE INDEX META ---
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

        void Reset();
    }
}
