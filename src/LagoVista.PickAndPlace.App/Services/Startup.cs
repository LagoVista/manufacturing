using Emgu.CV.Structure;
using Emgu.CV;
using LagoVista.Core.IOC;
using LagoVista.PickAndPlace.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.Core;
using System.Windows.Threading;
using LagoVista.Core.PlatformSupport;

namespace LagoVista.PickAndPlace.App.Services
{
    public static class Startup
    {
        public static void Init()
        {            
            SLWIOC.Register<IShapeDetectorService<Image<Bgr, byte>>, ShapeDetectionService>();
            SLWIOC.Register<ISocketClient, SocketClient>();
        }
    }
}
