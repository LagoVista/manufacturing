// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b51bbc568a925e6092d8d81c876e424270c72f485f3a84a488ec10c5e16fec6d
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.Services
{
    public class SocketClient : ISocketClient
    {
        TcpClient _client;

        public async Task ConnectAsync(string ipAddress, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ipAddress, port);
            InputStream = _client.GetStream();
            OutputStream = _client.GetStream();
        }

        public Task CloseAsync()
        {
            _client.Close();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_client != null)
                {
                    _client.Dispose();
                }
            }
        }

        public Stream InputStream { get; private set; }
        public Stream OutputStream { get; private set; }
    }
}
