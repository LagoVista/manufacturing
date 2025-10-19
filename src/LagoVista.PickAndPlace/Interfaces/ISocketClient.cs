// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f94400c090cee2aab76c8a251d220a44a1f8dcfd36fa987469f647eead766a4d
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface ISocketClient : IDisposable
    {
        Task ConnectAsync(String ipAddress, int port);
        Stream InputStream { get; }
        Stream OutputStream { get; }
        Task CloseAsync();

    }
}
