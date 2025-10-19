// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3a95232325e60a9a50c92c0aeffc9f97e4a7b2efb6cc4827fc724e8540fc46dd
// IndexVersion: 0
// --- END CODE INDEX META ---
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
