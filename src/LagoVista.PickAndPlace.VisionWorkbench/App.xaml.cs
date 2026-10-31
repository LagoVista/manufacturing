using LagoVista.Core;
using LagoVista.Core.IOC;
using System.Windows;

namespace LagoVista.PickAndPlace.VisionWorkbench;

public partial class App : Application
{
    public App()
    {
        SLWIOC.Register<IDispatcherServices>(new WorkbenchDispatcher(Dispatcher));
    }

    private sealed class WorkbenchDispatcher : IDispatcherServices
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher;
        public WorkbenchDispatcher(System.Windows.Threading.Dispatcher dispatcher) => _dispatcher = dispatcher;
        public void Invoke(Action action) => _dispatcher.BeginInvoke(action);
    }
}
