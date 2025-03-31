using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
  
        public bool IsPnPMachine
        {
            get
            {
                if (Settings == null)
                    return false;

                return Settings.FirmwareType == FirmwareTypes.LagoVista_PnP ||
                       Settings.FirmwareType == FirmwareTypes.LumenPnP_V4_Marlin ||
                       Settings.FirmwareType == FirmwareTypes.SimulatedMachine;
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

        private bool _isLocating;
        public bool IsLocating
        {
            get => _isLocating;
            set
            {
                if(_isLocating != value)
                {
                    _isLocating = value;
                    RaisePropertyChanged();
                }
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
            if (Settings.GcodeMapping != null)
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
        }
        public void ConfigureBottomLight(bool on, byte power, byte red, byte green, byte blue)
        {
            if (Settings.GcodeMapping != null)
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
            return gcode.Replace("{red}", $"{TopRed}")
                        .Replace("{green}", $"{TopGreen}")
                        .Replace("{blue}", $"{TopBlue}")
                        .Replace("{pwr}",
                        TopPower.ToString());
        }

        private string ConvertBottomLightColors(string gcode)
        {
            return gcode.Replace("{red}", $"{BottomRed}")
                        .Replace("{green}", $"{BottomGreen}")
                        .Replace("{blue}", $"{BottomBlue}")
                        .Replace("{pwr}", BottomPower.ToString());
        }

        private bool _topLightOn = false;
        public bool TopLightOn
        {
            get { return _topLightOn; }
            set
            {                
                switch (Settings.FirmwareType)
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
                switch (Settings.FirmwareType)
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
                if (!EntityHeader.IsNullOrEmpty(Settings.GcodeMapping))
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
                if (!EntityHeader.IsNullOrEmpty(Settings.GcodeMapping))
                {
                    if (value)
                        Enqueue(Settings.GcodeMapping?.Value.RightVacuumOn);
                    else
                        Enqueue(Settings.GcodeMapping?.Value.RightVacuumOff);
                }

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
                    switch (Settings.FirmwareType)
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
                    switch (Settings.FirmwareType)
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
                    switch (Settings.FirmwareType)
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
                    switch (Settings.FirmwareType)
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
            try
            {
                _toolHeadMoveSempahorr.Wait();

                if (_viewType == ViewTypes.Moving)
                {
                    AddStatusMessage(StatusMessageTypes.Info, "Can not move to camera, busy.");
                    _toolHeadMoveSempahorr.Release();
                    return;
                }

                if (_currentMachineToolHead == toolHeadToMoveTo)
                {
                    _toolHeadMoveSempahorr.Release();
                    return;
                }

                if (_currentMachineToolHead != null)
                {
                    _toolHeadMoveSempahorr.Release();
                    await MoveToCameraAsync();
                    _toolHeadMoveSempahorr.Wait();
                }

                await Task.Run(async () =>
                {
                    await Task.Delay(500);

                    Enqueue("M400"); // Wait for previous command to finish before executing next one.

                    System.Threading.SpinWait.SpinUntil(() => ToSendQueueCount == 0, 5000);
                    System.Threading.SpinWait.SpinUntil(() => UnacknowledgedBytesSent == 0, 5000);

                    var currentPosition = await GetCurrentLocationAsync();
                    var currentLocationX = currentPosition.X;
                    var currentLocationY = currentPosition.Y;

                    SendSafeMoveHeight();


                    SetRelativeMode();

                    _viewType = ViewTypes.Moving;
                    RaisePropertyChanged();

                    Enqueue($"G0 X{toolHeadToMoveTo.Offset.X} Y{toolHeadToMoveTo.Offset.Y} F{Settings.FastFeedRate}");

                    Enqueue("M400"); // Wait for previous command to finish before executing next one.
                                     //            Enqueue("G4 P1"); // just pause for 1ms

                    // Wait for the all the messages to get sent out (but won't get an OK for G4 until G0 finishes)
                    System.Threading.SpinWait.SpinUntil(() => ToSendQueueCount > 0, 5000);
                    System.Threading.SpinWait.SpinUntil(() => UnacknowledgedBytesSent == 0, 5000);

                    await Task.Delay(500);

                    // 4. set the machine back to absolute points
                    SetAbsoluteMode();

                    // 5. Set the machine location to where it was prior to the move.
                    await ResetMachineCoordinates(new Core.Models.Drawing.Point2D<double>(currentLocationX, currentLocationY));

                    _viewType = ViewTypes.Tool;
                    _currentMachineToolHead = toolHeadToMoveTo;

                    Services.DispatcherServices.Invoke(() =>
                    {
                        RaisePropertyChanged(nameof(ViewType));
                        RaisePropertyChanged(nameof(CurrentMachineToolHead));
                        AddStatusMessage(StatusMessageTypes.Info, $"Moved to tool head {toolHeadToMoveTo.Name}.");

                    });
                });
            }
            catch(Exception ex)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, ex.Message);
            }
            finally
            {
                _toolHeadMoveSempahorr.Release();
            }
          
        }

        SemaphoreSlim _toolHeadMoveSempahorr = new SemaphoreSlim(1);

        public async Task  MoveToCameraAsync()
        {
            try
            {
                _toolHeadMoveSempahorr.Wait();

                if (_viewType == ViewTypes.Moving)
                {
                    AddStatusMessage(StatusMessageTypes.Info, "Can not move to camera, busy.");
                    _toolHeadMoveSempahorr.Release();
                    return;
                }

                if (_currentMachineToolHead == null)
                {
                    AddStatusMessage(StatusMessageTypes.Info, "Already viewing camera.");
                    _toolHeadMoveSempahorr.Release();
                    return;
                }

                await Task.Run(async () =>
                {
                    await Task.Delay(500);

                    Enqueue("M400"); // Wait for previous command to finish before executing next one.

                    System.Threading.SpinWait.SpinUntil(() => ToSendQueueCount == 0, 5000);
                    System.Threading.SpinWait.SpinUntil(() => UnacknowledgedBytesSent == 0, 5000);

                    var currentPosition = await GetCurrentLocationAsync();
                    var currentLocationX = currentPosition.X;
                    var currentLocationY = currentPosition.Y;

                    SendSafeMoveHeight();

                    SetRelativeMode();

                    _viewType = ViewTypes.Moving;
                    RaisePropertyChanged();

                    Enqueue($"G0 X{-_currentMachineToolHead.Offset.X} Y{-_currentMachineToolHead.Offset.Y} F{Settings.FastFeedRate}");
                    Enqueue("M400"); // Wait for previous command to finish before executing next one.
                                     //            Enqueue("G4 P1"); // just pause for 1ms

                    // Wait for the all the messages to get sent out (but won't get an OK for G4 until G0 finishes)
                    System.Threading.SpinWait.SpinUntil(() => ToSendQueueCount > 0, 5000);
                    System.Threading.SpinWait.SpinUntil(() => UnacknowledgedBytesSent == 0, 5000);

                    // 4. set the machine back to absolute points
                    SetAbsoluteMode();

                    await Task.Delay(500);

                    // 5. Set the machine location to where it was prior to the move.
                    await ResetMachineCoordinates(new Core.Models.Drawing.Point2D<double>(currentLocationX, currentLocationY));

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
            catch(Exception ex)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, ex.Message);
            }
            finally
            {
                _toolHeadMoveSempahorr.Release();
            }


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


        IImageCaptureService _partInspectionCaptureService;
        public IImageCaptureService PartInspectionCaptureService
        {
            get => _partInspectionCaptureService;
            set
            {
                _partInspectionCaptureService = value;
                RaisePropertyChanged();
            }
        }
    }
}
