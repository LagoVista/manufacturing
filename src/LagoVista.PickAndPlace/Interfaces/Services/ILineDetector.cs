using Emgu.CV;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface ILineDetector<T> where T : class, IDisposable
    {
        void FindLines(IMVImage<IInputOutputArray> input, MachineCamera camera, System.Drawing.Size size);
        ObservableCollection<MVLocatedLine> FoundLines { get; }
        void Reset();
    }
}
