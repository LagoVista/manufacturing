// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: bcb96b3342cc810655ef8dcdf090279ac6cd3ca4e8a03a12bc892c55610f7f09
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public partial class SettingsViewModel : INotifyPropertyChanged
    {
        LagoVista.Manufacturing.Models.Machine _settings;
        IMachineRepo _machineRepo;

        public event PropertyChangedEventHandler PropertyChanged;


        public void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            Services.DispatcherServices.Invoke(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))
            );
        }

        public SettingsViewModel(IMachineRepo machineRepo, LagoVista.Manufacturing.Models.Machine settings) //: base(machine)
        {

            Cameras = new List<MachineCamera>();
            _settings = settings;
            _machineRepo = machineRepo;
            InitComamnds();
            Init();
        }

        public async void Init()
        {
            await InitAsync();
        }

        public async Task InitAsync()
        {
            if (Settings.CurrentSerialPort == null)
            {
                Settings.CurrentSerialPort = new SerialPortInfo()
                {
                    Id = "empty",
                    Name = "-select-"
                };
            }


            var ports = await SLWIOC.Get<IDeviceManager>().GetSerialPortsAsync();
#if DEBUG
            ports.Insert(0, new SerialPortInfo() { Id = "Simulated", Name = "Simulated" });
#endif
            ports.Insert(0, new SerialPortInfo() { Id = "empty", Name = "-select-" });
            SerialPorts = ports;
            RaisePropertyChanged(nameof(SerialPorts));

            var firmwareTypes = new ObservableCollection<string>();
            var enums = Enum.GetValues(typeof(FirmwareTypes));
            foreach(var enumType in enums)
            {
                firmwareTypes.Add(enumType.ToString().Replace("_","."));
            }

            FirmwareTypes = firmwareTypes;
            RaisePropertyChanged(nameof(FirmwareTypes));

            var machineTypes = new ObservableCollection<string>();
            enums = Enum.GetValues(typeof(MachineTypes));
            foreach (var enumType in enums)
            {
                machineTypes.Add(enumType.ToString().Replace("_", "."));
            }

            MachineTypes = machineTypes;
            RaisePropertyChanged(nameof(MachineTypes));

            var gcodeCommands = Enum.GetValues(typeof(JogGCodeCommand));
            var gcodeJogCommands = new ObservableCollection<string>();
            foreach (var gcodeCmd in gcodeCommands)
            {
                gcodeJogCommands.Add(gcodeCmd.ToString().Replace("_", "."));
            }

            GCodeJogCommands = gcodeJogCommands;
            RaisePropertyChanged(nameof(GCodeJogCommands));

            var origins = Enum.GetValues(typeof(MachineOrigin));
            var originOptions = new ObservableCollection<string>();
            foreach(var origin in origins)
            {
                originOptions.Add(origin.ToString().Replace("_", " "));
            }

            MachineOrigins = originOptions;
            RaisePropertyChanged(nameof(MachineOrigins));

            var outputLevels = Enum.GetValues(typeof(MessageVerbosityLevels));
            var outputLevelOptions = new ObservableCollection<string>();
            foreach (var outputLevel in outputLevels)
            {
                outputLevelOptions.Add(outputLevel.ToString().Replace("_", " "));
            }

            MessageOutputLevels = outputLevelOptions;
            RaisePropertyChanged(nameof(MessageOutputLevels));

            var connectionTypes = Enum.GetValues(typeof(ConnectionTypes));
            var connectionTypeOptions = new ObservableCollection<string>();
            foreach (var connectionType in connectionTypes)
            {
                connectionTypeOptions.Add(connectionType.ToString().Replace("_", " "));
            }

            ConnectionTypes = connectionTypeOptions;
            RaisePropertyChanged(nameof(ConnectionTypes));
        }
    }
}
