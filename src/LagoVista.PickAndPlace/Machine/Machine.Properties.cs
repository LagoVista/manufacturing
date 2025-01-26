using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using RingCentral;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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

        // TODO: 31.5 should NOT be a constant, but need to get some boards built.
        public double LeftToolHeadZ { get => (ToolCommonZ - 31.5) + 31.5; }
        
        public double RightToolHeadZ { get => (31.5 - ToolCommonZ) + 31.5; }

        private double _toolHeadCommonZ;
        public double ToolCommonZ
        {
            get { return _toolHeadCommonZ; }
            set
            {
                if (_toolHeadCommonZ != value)
                {
                    _toolHeadCommonZ = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(LeftToolHeadZ));
                    RaisePropertyChanged(nameof(RightToolHeadZ));
                }
            }
        }

        private double _leftToolHeadRotate;
        public double LeftToolHeadRotate
        {
            get { return _leftToolHeadRotate; }
            set
            {
                if (_leftToolHeadRotate != value)
                {
                    _leftToolHeadRotate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _rightToolHeadRotate;
        public double RightToolHeadRotate
        {
            get { return _rightToolHeadRotate; }
            set
            {
                if (_rightToolHeadRotate != value)
                {
                    _rightToolHeadRotate = value;
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
                UpdateTopLight();
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
                UpdateBottomLight();
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
                UpdateTopLight();
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
                UpdateBottomLight();
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
                UpdateTopLight();
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
                UpdateBottomLight();
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
                UpdateTopLight();
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
                UpdateBottomLight();
            }
        }

        public void ConfigureTopLight(bool on, byte power, byte red, byte green, byte blue)
        {
            _topLightOn = on;
            _topPower = power;
            _topRed = red;
            _topGreen = green;
            _topBlue = blue;

            if (_topLightOn)
                Enqueue(ConvertTopLightColors(Settings.GcodeMapping.Value.TopLightOn));
            else
                Enqueue(Settings.GcodeMapping.Value.TopLightOff);

            UpdateTopLight();

            RaisePropertyChanged(nameof(TopLightOn));
            RaisePropertyChanged(nameof(TopPower));
            RaisePropertyChanged(nameof(TopRed));
            RaisePropertyChanged(nameof(TopBlue));
            RaisePropertyChanged(nameof(TopGreen));
            
        }
        public void ConfigureBottomLight(bool on, byte power, byte red, byte green, byte blue)
        {
            _bottomLightOn = on;
            _bottomPower = power;
            _bottomRed = red;
            _bottomGreen = green;
            _bottomBlue = blue;

            if (_bottomLightOn)
                Enqueue(ConvertTopLightColors(Settings.GcodeMapping.Value.BottmLightOn));
            else
                Enqueue(Settings.GcodeMapping.Value.BottmLightOff);

            UpdateBottomLight();

            RaisePropertyChanged(nameof(BottomLightOn));
            RaisePropertyChanged(nameof(BottomPower));
            RaisePropertyChanged(nameof(BottomRed));
            RaisePropertyChanged(nameof(BottomBlue));
            RaisePropertyChanged(nameof(BottomGreen));
        }

        private void UpdateTopLight()
        {
            if (TopLightOn && Connected)
            {
                Enqueue(ConvertTopLightColors(Settings.GcodeMapping.Value.TopLightOn));
            }
        }
        
        private void UpdateBottomLight()
        { 
            if (BottomLightOn && Connected)
            {
                Enqueue(ConvertBottomLightColors(Settings.GcodeMapping.Value.BottmLightOn));
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

                RaisePropertyChanged();
            }
        }

        private bool _bottomLightOn = false;
        public bool BottomLightOn
        {
            get { return _bottomLightOn; }
            set
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


        private bool _leftVacuumPump = false;
        public bool LeftVacuumPump
        {
            get { return _leftVacuumPump; }
            set
            {
                if (Settings.GcodeMapping.Value != null)
                {
                    if (value)
                        Enqueue(Settings.GcodeMapping.Value.LeftVacuumOn);
                    else
                        Enqueue(Settings.GcodeMapping.Value.LeftVacuumOff);
                }

                _leftVacuumPump = value;
                RaisePropertyChanged();
            }
        }

        private bool _rightVacuumPump = false;
        public bool RightVacuumPump
        {
            get { return _rightVacuumPump; }
            set
            {
                if (value)
                    Enqueue(Settings.GcodeMapping.Value.RightVacuumOn);
                else
                    Enqueue(Settings.GcodeMapping.Value.RightVacuumOff);

                _rightVacuumPump = value;
                RaisePropertyChanged();
            }
        }

        private bool _leftToolHead
        {
            get
            {
                if (CurrentMachineToolHead == null)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, "Attempt to call _leftTooLHead, should never do so with camera view, very likely a bug");
                    return true;
                }

                return CurrentMachineToolHead.HeadIndex == 1;
            }
        }



        private bool _vacuumPump = false;
        public bool VacuumPump
        {
            get 
            {
                if (CurrentMachineToolHead == null)
                {
                    return false;
                }
                else 
                {
                    return _leftToolHead ? LeftVacuumPump : RightVacuumPump;                    
                }                 
            }
            set
            {
                if (CurrentMachineToolHead == null)
                {
                    LeftVacuumPump = false;
                    RightVacuumPump = false;
                }
                else
                {
                    if (_leftToolHead)
                    {
                        LeftVacuumPump = value;
                    }
                    else
                    {
                        RightVacuumPump = value;
                    }
                }

                RaisePropertyChanged();
            }
        }

        public ulong Vacuum
        {
            get
            {
                if (CurrentMachineToolHead == null)
                {
                    return 0;
                }
                else
                {
                    return _leftToolHead ? _leftVacuum : _rightVacuum;
                }
            }
            set
            {
                if(CurrentMachineToolHead != null)
                {
                    if (_leftToolHead)
                        LeftVacuum = value;
                    else
                        RightVacuum = value;
                }
                else
                {
                    LeftVacuum = 0;
                    RightVacuum = 0;
                }
            }

        }

        ulong _leftVacuum;
        public ulong LeftVacuum
        {
            get => _leftVacuum;
            set
            {
                _leftVacuum = value;
                RaisePropertyChanged();
            }
        }

        ulong _rightVacuum;
        public ulong RightVacuum
        {
            get => _rightVacuum;
            set
            {
                _rightVacuum = value;
                RaisePropertyChanged();
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
                            if (Settings.GcodeMapping.Value != null)
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

        public async Task MoveToToolHeadAsync(MachineToolHead toolHeadToMoveTo)
        {
            if (_viewType == ViewTypes.Moving)
            {
                AddStatusMessage(StatusMessageTypes.Info, "Can not move to camera, busy.");
                return;
            }

            if(_currentMachineToolHead == toolHeadToMoveTo)
            {
                return;
            }

            if (_currentMachineToolHead != null )
            {
                await MoveToCameraAsync();
            }

            await Task.Run(() =>
            {
                var currentLocationX = MachinePosition.X;
                var currentLocationY = MachinePosition.Y;

                Enqueue("G91"); // relative move

                _viewType = ViewTypes.Moving;
                RaisePropertyChanged();

                Enqueue($"G0 X{toolHeadToMoveTo.Offset.X} Y{toolHeadToMoveTo.Offset.Y} F{Settings.FastFeedRate}");

                Enqueue("M400"); // Wait for previous command to finish before executing next one.
                                 //            Enqueue("G4 P1"); // just pause for 1ms

                // Wait for the all the messages to get sent out (but won't get an OK for G4 until G0 finishes)
                System.Threading.SpinWait.SpinUntil(() => ToSendQueueCount > 0, 5000);

                Debug.WriteLine("All Sent");
                System.Threading.SpinWait.SpinUntil(() => UnacknowledgedBytesSent == 0, 5000);
                Debug.WriteLine("Un Ack Bytes Good.");

                // 4. set the machine back to absolute points
                Enqueue("G90");

                // 5. Set the machine location to where it was prior to the move.
                Enqueue($"G92 X{currentLocationX} Y{currentLocationY}");

                _viewType = ViewTypes.Tool;
                _currentMachineToolHead = toolHeadToMoveTo;

                Services.DispatcherServices.Invoke(() =>
                {
                    RaisePropertyChanged(nameof(ViewType));
                    RaisePropertyChanged(nameof(CurrentMachineToolHead));
                    AddStatusMessage(StatusMessageTypes.Info, $"Moved to tool head {toolHeadToMoveTo.Name}.");
                    
                });
            });
            // wait until G4 gets marked at sent
            //    

            //        Debug.WriteLine("Un Ack Bytes Good.");

        }

        public async Task  MoveToCameraAsync()
        {
            if(_viewType == ViewTypes.Moving)
            {
                AddStatusMessage(StatusMessageTypes.Info, "Can not move to camera, busy.");
                return;
            }

            if(_currentMachineToolHead == null)
            {
                AddStatusMessage(StatusMessageTypes.Info, "Already viewing camera.");
                return;
            }

            await Task.Run(() =>
            {
                var currentLocationX = MachinePosition.X;
                var currentLocationY = MachinePosition.Y;

                Enqueue("G91"); // relative move

                _viewType = ViewTypes.Moving;
                RaisePropertyChanged();

                Enqueue($"G0 X{-_currentMachineToolHead.Offset.X} Y{-_currentMachineToolHead.Offset.Y} F{Settings.FastFeedRate}");

                Enqueue("M400"); // Wait for previous command to finish before executing next one.
                                 //            Enqueue("G4 P1"); // just pause for 1ms

                // Wait for the all the messages to get sent out (but won't get an OK for G4 until G0 finishes)
                System.Threading.SpinWait.SpinUntil(() => ToSendQueueCount > 0, 5000);

                Debug.WriteLine("All Sent");
                System.Threading.SpinWait.SpinUntil(() => UnacknowledgedBytesSent == 0, 5000);
                Debug.WriteLine("Un Ack Bytes Good.");

                // 4. set the machine back to absolute points
                Enqueue("G90");

                // 5. Set the machine location to where it was prior to the move.
                Enqueue($"G92 X{currentLocationX} Y{currentLocationY}");

                _currentMachineToolHead = null;
                _viewType = ViewTypes.Camera;

                Services.DispatcherServices.Invoke(() =>
                {
                    RaisePropertyChanged(nameof(ViewType));
                    RaisePropertyChanged(nameof(CurrentMachineToolHead));
                    AddStatusMessage(StatusMessageTypes.Info, "Reset to camera view.");
                });
                
            });
        }

        public async void MoveToToolHead(MachineToolHead toolHead)
        {
            await MoveToToolHeadAsync(toolHead);
        }

        public async void MoveToCamera()
        {
            await MoveToCameraAsync();
        }

        MachineToolHead _currentMachineToolHead;
        public MachineToolHead CurrentMachineToolHead
        {
            get => _currentMachineToolHead;
            set
            {
                if (_currentMachineToolHead != null && value == null)
                {
                    MoveToCamera();
                }
                else if(_currentMachineToolHead == null && value != null)
                {
                    MoveToToolHead(value);
                }
                else if(_currentMachineToolHead != value)
                {
                    MoveToToolHead(value);
                }
            }
        }

        bool _wasMachineHomed = false;
        public bool WasMachineHomed
        {
            get => _wasMachineHomed;
            private set
            {
                _wasMachineHomed = value;
                _currentMachineToolHead = null;
                RaisePropertyChanged(nameof(CurrentMachineToolHead));
                RaisePropertyChanged();
            }
        }
        
        bool _wasMachineOriginCalibrated = false;
        public bool WasMachinOriginCalibrated
        {
            get => _wasMachineOriginCalibrated;
            set
            {
                _wasMachineOriginCalibrated = value;
                RaisePropertyChanged();
            }
        }

        IImageCaptureService _positionImageCaptureService;
        public IImageCaptureService PositionImageCaptureService
        {
            get => _positionImageCaptureService;
            set
            {
                _positionImageCaptureService = value;
                RaisePropertyChanged();
            }
        }
    }
}
