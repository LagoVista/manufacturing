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
            SLWIOC.Register<IRectangleDetector<IInputOutputArray>, RectangleDetector>();
        }
    }
}
