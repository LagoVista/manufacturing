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
