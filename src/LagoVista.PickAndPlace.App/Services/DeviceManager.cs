// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 39cc8e30775d790deeb969b833902c2b4d22391e689780f9fb347ddf359e489f
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.Services
{
    public class DeviceManager : IDeviceManager
    {
        public ISerialPort CreateSerialPort(SerialPortInfo portInfo)
        {
            return new SerialPort(portInfo);
        }

        public Task<ObservableCollection<SerialPortInfo>> GetSerialPortsAsync()
        {
            var ports = new ObservableCollection<SerialPortInfo>();
            foreach (var port in System.IO.Ports.SerialPort.GetPortNames())
            {
                ports.Add(new SerialPortInfo()
                {
                    Id = port,
                    Name = port,
                });
                
            }

            return Task.FromResult(ports);
        }
    }
}
