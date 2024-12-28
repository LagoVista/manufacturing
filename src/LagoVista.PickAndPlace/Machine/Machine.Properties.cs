using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
        private IGCodeFileManager _gcodeFileManager;
        public IGCodeFileManager GCodeFileManager
        {
            get { return _gcodeFileManager; }
            set
            {
                _gcodeFileManager = value;
                RaisePropertyChanged();
            }
        }

        IHeightMapManager _heightMapManager;
        public IHeightMapManager HeightMapManager
        {
            get { return _heightMapManager; }
            private set
            {
                _heightMapManager = value;
                RaisePropertyChanged();
            }
        }

        IProbingManager _probingManager;
        public IProbingManager ProbingManager
        {
            get { return _probingManager; }
            private set
            {
                _probingManager = value;
                RaisePropertyChanged();
            }
        }

        public bool IsPnPMachine
        {
            get
            {
                if (Settings == null)
                    return false;

                return Settings.MachineType == FirmwareTypes.LagoVista_PnP ||
                       Settings.MachineType == FirmwareTypes.LumenPnP_V4_Marlin ||
                       Settings.MachineType == FirmwareTypes.Repeteir_PnP ||
                       Settings.MachineType == FirmwareTypes.SimulatedMachine;
            }
        }

        IPCBManager _pcbManager;
        public IPCBManager PCBManager
        {
            get { return _pcbManager; }
            private set
            {
                _pcbManager = value;
                RaisePropertyChanged();
            }
        }

        IToolChangeManager _toolChangeManager;
        public IToolChangeManager ToolChangeManager
        {
            get { return _toolChangeManager; }
            private set
            {
                _toolChangeManager = value;
                RaisePropertyChanged();
            }
        }

        public Core.Models.Drawing.Vector3 NormalizedPosition
        {
            get { return MachinePosition - WorkPositionOffset; }
        }


        private double _tool0;
        public double Tool0
        {
            get { return _tool0; }
            set
            {
                if (_tool0 != value)
                {
                    _tool0 = value;
                    RaisePropertyChanged();
                }
            }
        }


        private double _tool1;
        public double Tool1
        {
            get { return _tool1; }
            set
            {
                if (_tool1 != value)
                {
                    _tool1 = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _tool2;
        public double Tool2
        {
            get { return _tool2; }
            set
            {
                if (_tool2 != value)
                {
                    _tool2 = value;
                    RaisePropertyChanged();
                }
            }
        }


        bool _isInitialized = false;
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set
            {
                _isInitialized = value;
                RaisePropertyChanged();
            }
        }

        IMachineVisionManager _machineVisionManager;
        public IMachineVisionManager MachineVisionManager
        {
            get { return _machineVisionManager; }
            private set
            {
                _machineVisionManager = value;
                RaisePropertyChanged();
            }
        }


        IBoardAlignmentManager _boardAlignmentManager;
        public IBoardAlignmentManager BoardAlignmentManager
        {
            get { return _boardAlignmentManager; }
            private set
            {
                _boardAlignmentManager = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> PendingQueue { get; } = new ObservableCollection<string>();

        ObservableCollection<Models.StatusMessage> _messages;
        public ObservableCollection<Models.StatusMessage> Messages
        {
            get { return _messages; }
            private set
            {
                _messages = value;
                RaisePropertyChanged();
            }
        }

        private bool _motorsEnabled;
        public bool MotorsEnabled
        {
            get { return _motorsEnabled; }
            set
            {
                _motorsEnabled = value;
                SendCommand(value ? "M17" : "M18");
            }
        }

        public int MessageCount
        {
            get
            {
                if (Messages == null)
                {
                    return 0;
                }

                return Messages.Count - 1;
            }
        }

        public bool IsOnHold => _isOnHold;


        private bool _locationUpdateEnabled = true;
        public bool LocationUpdateEnabled
        {
            get { return _locationUpdateEnabled; }
            set
            {
                _locationUpdateEnabled = value;
                RaisePropertyChanged();
            }
        }


        LagoVista.Manufacturing.Models.Machine _settings;
        public LagoVista.Manufacturing.Models.Machine Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsPnPMachine));
            }
        }

        private byte _topRed = 0xff;
        public byte TopRed
        {
            get => _topRed;
            set
            {
                _topRed = value;
                RaisePropertyChanged();
            }
        }

        private byte _bottomRed = 0xff;
        public byte BottomRed
        {
            get => _bottomRed;
            set
            {
                _bottomRed = value;
                RaisePropertyChanged();
            }
        }


        private byte _topGreen = 0xff;
        public byte TopGreen
        {
            get => _topGreen;
            set
            {
                _topGreen = value;
                RaisePropertyChanged();
            }
        }

        private byte _bottomGreen = 0xff;
        public byte BottomGreen
        {
            get => _bottomGreen;
            set
            {
                _bottomGreen = value;
                RaisePropertyChanged();
            }
        }

        private byte _topBlue = 0xff;
        public byte TopBlue
        {
            get => _topBlue;
            set
            {
                _topBlue = value;
                RaisePropertyChanged();
            }
        }

        private byte _bottomBlue = 0xff;
        public byte BottomBlue
        {
            get => _bottomBlue;
            set
            {
                _bottomBlue = value;
                RaisePropertyChanged();
            }
        }

        private byte _topPower = 0xff;
        public byte TopPower
        {
            get => _topPower;
            set
            {
                _topPower = value;
                RaisePropertyChanged();
            }
        }

        private byte _bottomPower = 0xff;
        public byte BottomPower
        {
            get => _bottomPower;
            set
            {
                _bottomPower = value;
                RaisePropertyChanged();
            }
        }


        private string ConvertTopLightColors(string gcode)
        {
            return gcode.Replace("{red}", $"{TopRed}").Replace("{green}", $"{TopGreen}").Replace("{blue}", $"{TopBlue}").Replace("{pwr}", TopPower.ToString());
        }

        private string ConvertBottomLightColors(string gcode)
        {
            return gcode.Replace("{red}", $"{TopRed}").Replace("{green}", $"{BottomGreen}").Replace("{blue}", $"{BottomBlue}").Replace("{pwr}", BottomPower.ToString());
        }

        private bool _topLightOn = false;
        public bool TopLightOn
        {
            get { return _topLightOn; }
            set
            {
                if (_topLightOn != value)
                {                     
                    switch (Settings.MachineType)
                    {
                        case FirmwareTypes.Repeteir_PnP: Enqueue($"M42 P31 S{(value ? 0 : 255)}"); break;
                        case FirmwareTypes.LagoVista_PnP: Enqueue($"M60 S{(value ? 255 : 0)}"); break;
                        default:
                            if (Settings.GcodeMapping.Value != null)
                            {
                                if (value)
                                    Enqueue(ConvertTopLightColors(Settings.GcodeMapping.Value.TopLightOn));
                                else
                                    Enqueue(Settings.GcodeMapping.Value.TopLightOff);
                            }
                            break;
                    }

                    _topLightOn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _bottomLightOn = false;
        public bool BottomLightOn
        {
            get { return _bottomLightOn; }
            set
            {
                if (_bottomLightOn != value)
                {
                    switch (Settings.MachineType)
                    {
                        case FirmwareTypes.Repeteir_PnP: Enqueue($"M42 P33 S{(value ? 0 : 255)}"); break;
                        case FirmwareTypes.LagoVista_PnP: Enqueue($"M61 S{(value ? 255 : 0)}"); break;
                        default:
                            if (Settings.GcodeMapping.Value != null)
                            {
                                if (value)
                                    Enqueue(ConvertBottomLightColors(Settings.GcodeMapping.Value.BottmLightOn));
                                else
                                    Enqueue(Settings.GcodeMapping.Value.BottmLightOff);
                            }
                            break;
                    }

                    _bottomLightOn = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _vacuumPump = false;
        public bool VacuumPump
        {
            get { return _vacuumPump; }
            set
            {
                if (_vacuumPump != value)
                {
                    switch (Settings.MachineType)
                    {
                        case FirmwareTypes.Repeteir_PnP:
                            Enqueue($"M42 P10 S{(value ? 255 : 0)}");
                            break;
                        case FirmwareTypes.LagoVista_PnP:
                            Enqueue($"M64 S{(value ? 255 : 0)}");                            
                            break;
                        default:
                            if (Settings.GcodeMapping.Value != null)
                            {
                                if (value)
                                    Enqueue(Settings.GcodeMapping.Value.LeftVacuumOn);
                                else
                                    Enqueue(Settings.GcodeMapping.Value.LeftVacuumOff);
                            }
                            break;
                    }

                    _vacuumPump = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _puffPump = false;
        public bool PuffPump
        {
            get { return _puffPump; }
            set
            {
                if (_puffPump != value)
                {
                    switch (Settings.MachineType)
                    {
                        case FirmwareTypes.Repeteir_PnP: 
                            Enqueue($"M42 P07 S{(value ? 255 : 0)}");
                            break;
                        case FirmwareTypes.LagoVista_PnP: 
                            Enqueue($"M63 S{(value ? 255 : 0)}");                            
                            break;
                        default:
                            if (Settings.GcodeMapping.Value != null)
                            {
                                if (value)
                                    Enqueue(Settings.GcodeMapping.Value.RightVacuumOn);
                                else
                                    Enqueue(Settings.GcodeMapping.Value.RightVacuumOff);
                            }
                            break;

                    }
                }

                _puffPump = value;
                RaisePropertyChanged();
            }
        }

        private bool _headSolenoid = false;
        public bool HeadSolenoid
        {
            get { return _headSolenoid; }
            set
            {
                if (_headSolenoid != value)
                {
                    switch (Settings.MachineType)
                    {
                        case FirmwareTypes.Repeteir_PnP:
                            Enqueue($"M42 P23 S{(value ? 255 : 0)}");
                            break;
                        default:
                            if(Settings.GcodeMapping.Value != null)
                            {
                                Enqueue(Settings.GcodeMapping.Value.ReadLeftVacuumCmd);
                            }
                            break;
                    }
                }

                _headSolenoid = value;
                RaisePropertyChanged();
            }
        }


        private bool _puffSolenoid = false;
        public bool PuffSolenoid
        {
            get { return _puffSolenoid; }
            set
            {
                if (_puffSolenoid != value)
                {
                    switch (Settings.MachineType)
                    {
                        case FirmwareTypes.Repeteir_PnP:
                                Enqueue($"M42 P23 S{(value ? 255 : 0)}");
                            break;
                        default:
                            if (Settings.GcodeMapping.Value != null)
                            {
                                Enqueue(Settings.GcodeMapping.Value.ReadRightVacuumCmd);
                            }
                            break;
                    }
                }

                _puffSolenoid = value;
                RaisePropertyChanged();
            }
        }


        private bool _vacuumSolenoid = false;
        public bool VacuumSolendoid
        {
            get { return _vacuumSolenoid; }
            set
            {
                if (_vacuumSolenoid != value)
                {
                    switch (Settings.MachineType)
                    {
                        case FirmwareTypes.Repeteir_PnP:
                            Enqueue($"M42 P27 S{(value ? 255 : 0)}");
                            
                            break;
                        case FirmwareTypes.LagoVista_PnP:
                            
                            Enqueue($"M64 S{(value ? 255 : 0)}"); 
                            break;
                    }

                    _vacuumSolenoid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _machinePendingQueueLength;
        public int MachinePendingQueueLength
        {
            get { return _machinePendingQueueLength; }
            set
            {
                _machinePendingQueueLength = value;
                RaisePropertyChanged();
            }
        }
    }
}
