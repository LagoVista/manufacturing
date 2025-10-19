// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f6178540487f69958b34dbbdf31c90fc6032c8b3217215b7f95dd12ac73b86b1
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public partial class SettingsViewModel
    {

        public String SelectedPortId
        {
            get { return Settings.CurrentSerialPort.Id; }
            set
            {
                var port = SerialPorts.Where(prt => prt.Id == value).FirstOrDefault();
                if (port == null)
                    port = SerialPorts.First();
                else
                {
                    port.BaudRate = 115200;
                }

                Settings.CurrentSerialPort = port;
            }
        }

        public String SelectedPort2Id
        {
            get { return Settings.SerialPort2?.Id; }
            set
            {
                var port = SerialPorts.Where(prt => prt.Id == value).FirstOrDefault();
                if (port == null)
                    port = SerialPorts.First();
                else
                {
                    port.BaudRate = 115200;
                }

                Settings.SerialPort2 = port;
            }
        }


        public ObservableCollection<String> ConnectionTypes { get; private set; }
        public ObservableCollection<String> FirmwareTypes { get; private set; }
        public ObservableCollection<String> MachineTypes { get; private set; }
        public ObservableCollection<String> GCodeJogCommands { get; private set; }
        public ObservableCollection<String> MachineOrigins { get; private set; }
        public ObservableCollection<String> MessageOutputLevels { get; private set; }

        public string MessgeOutputLevel
        {
            get { return Settings.MessageVerbosity.ToString(); }
            set
            {
                Settings.MessageVerbosity = (MessageVerbosityLevels)Enum.Parse(typeof(MessageVerbosityLevels), value);
            }
        }

        public String GCodeJogCommand
        {
            get { return Settings.JogGCodeCommand.ToString().Replace("_"," "); }
            set
            {
                Settings.JogGCodeCommand = (JogGCodeCommand)Enum.Parse(typeof(JogGCodeCommand), value);
            }
        }

        public String ConnectionType
        {
            get { return Settings.ConnectionType.ToString().Replace("_", " "); }
            set
            {
                var newValue = value.Replace(" ", "_");
                Settings.ConnectionType = (ConnectionTypes)Enum.Parse(typeof(ConnectionTypes), newValue);
                RaisePropertyChanged(nameof(CanSetIPAddress));
                RaisePropertyChanged(nameof(CanSelectSerialPort));
            }
        }

        public String MachineOrigin
        {
            get { return Settings.MachineOrigin.ToString().Replace("_", " "); }
            set
            {
                var newValue = value.Replace(" ", "_");
                Settings.MachineOrigin = (MachineOrigin)Enum.Parse(typeof(MachineOrigin), newValue);
            }
        }

        public List<MachineCamera> Cameras { get; private set; }

        public String MachineType
        {
            get { return Settings.CartesianMachineType.ToString().Replace("_", "."); }
            set { Settings.CartesianMachineType = (MachineTypes)Enum.Parse(typeof(MachineTypes), value.Replace(".", "_")); }
        }

        public string FirmwareType
        {
            get => Settings.FirmwareType.ToString().Replace("_", ".");
            set => Settings.FirmwareType = (FirmwareTypes)Enum.Parse(typeof(FirmwareTypes), value.Replace(".", "_"));
        }

        public LagoVista.Manufacturing.Models.Machine Settings
        {
            get { return _settings; }
        }

        public ObservableCollection<SerialPortInfo> SerialPorts { get; private set; }

        public bool CanSetIPAddress
        {
            get { return Settings.ConnectionType == Manufacturing.Models.ConnectionTypes.Network && CanChangeMachineConfig; }
        }

        public bool CanSelectSerialPort
        {
            get { return Settings.ConnectionType == Manufacturing.Models.ConnectionTypes.Serial_Port && CanChangeMachineConfig; }
        }
    }
}
