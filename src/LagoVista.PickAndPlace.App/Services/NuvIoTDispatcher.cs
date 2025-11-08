// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 737af51cb2c48a7170afb60e67877761a56f9fc1d41b532e4c7cea51d039e9cf
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LagoVista.PickAndPlace.App.Services
{
    public class NuvIoTDispatcher : IDispatcherServices
    {
        Dispatcher _dispatcher;
        public NuvIoTDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Invoke(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }
    }
}
