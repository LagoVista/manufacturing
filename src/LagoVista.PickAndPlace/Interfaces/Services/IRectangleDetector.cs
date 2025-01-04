using Emgu.CV.Structure;
using Emgu.CV;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using LagoVista.PickAndPlace.Models;
using NLog.Filters;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface IRectangleDetector<T> where T : class, IDisposable
    {
        void FindRectangles(IMVImage<T> input, MachineCamera camera, System.Drawing.Size size);
        List<MVLocatedRectangle> FoundRectangles { get; }
        List<MVLocatedLine> FoundLines { get; }
        void Reset();
    }
}
