// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ff99bb9b76f15793be61ca3cdd2294961662aae234dd6d696d77c1b1403a52a3
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV;
using LagoVista.Core.IOC;
using LagoVista.PickAndPlace.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public static class Startup
    {
       public static void Init()
        {
            SLWIOC.Register<ICircleDetector<IInputOutputArray>, CircleDetector>();
            SLWIOC.Register<ICornerDetector<IInputOutputArray>, CornerDetector>();
            SLWIOC.Register<IImageHelper<IInputOutputArray>, ImageHelper>();
            SLWIOC.Register<ILineDetector<IInputOutputArray>, LineDetector>();
            SLWIOC.Register<IRectangleDetector<IInputOutputArray>, RectangleDetector>();
        }
    }
}
