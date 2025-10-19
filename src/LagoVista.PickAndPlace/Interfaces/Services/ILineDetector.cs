// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9d58acfe87238f1bcbba8d940410ad0aa55517fad99d3fc6ad5a23aab23735b0
// IndexVersion: 0
// --- END CODE INDEX META ---
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
