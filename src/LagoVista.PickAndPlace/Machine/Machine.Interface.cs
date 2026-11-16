// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a1856a3be475bad44cf35c3d235ebc248afb92ba216beff9fe472c4cfefa830b
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using LagoVista.Client.Core.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.GCode;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
        SerialPort _port;

        ISocketClient _socketClient;

        public async Task ConnectAsync(SimulatedSerialPort port)
        {
            Connected = true;

            _cancelSource = new CancellationTokenSource();

            AddStatusMessage(StatusMessageTypes.Info, $"Opened Serial Port");

            await port.OpenAsync();

            await Task.Run(() =>
            {
                Work(port.InputStream, port.OutputStream);
            }, _cancelSource.Token);
        }

        public async void Connect(SerialPort port)
        {
            await ConnectAsync(port);
        }


        public async Task ConnectAsync(SerialPort port)
        {
            _port = port;

            if (Connected)
                throw new Exception("Can't Connect: Already Connected");

            try
            {
                port.Open();
                port.DataReceived += Port_DataReceived;

                var outputStream = _port.BaseStream;
                if (outputStream == null)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, $"Could not open serial port");
                    return;
                }


                Connected = true;

                lock (_queueAccessLocker)
                {
                    _toSend.Clear();
                    _sentQueue.Clear();
                    _toSendPriority.Clear();
                }

                Mode = OperatingMode.Manual;

                if (Settings.FirmwareType == FirmwareTypes.Repeteir_PnP)
                {
                    Enqueue("M43 P25");
                    Enqueue("M43 P27");
                    Enqueue("M43 P29");
                    Enqueue("M43 P31");
                    Enqueue("M43 P32");
                    Enqueue("M43 P33");
                    Enqueue("G90");
                }

                _cancelSource = new CancellationTokenSource();
                _port = port;

                AddStatusMessage(StatusMessageTypes.Info, $"Opened Serial Port");
                MachineConnected?.Invoke(this, null);
                await Task.Run(() =>
                {
                    Work(_port.BaseStream, _port.BaseStream);
                }, _cancelSource.Token);
            }
            catch (Exception ex)
            {
                MachineDisconnected?.Invoke(this, null);
                _port = null;
                Connected = false;
                AddStatusMessage(StatusMessageTypes.Warning, $"Could not open serial port: " + ex.Message);
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var obj = sender;
        }

        CancellationTokenSource _cancelSource;

        public async void Connect(ISocketClient socketClient)
        {
            await ConnectAsync(socketClient);
        }

        public async Task ConnectAsync(ISocketClient socketClient)
        {
            try
            {
                _socketClient = socketClient;
                _cancelSource = new CancellationTokenSource();

                Connected = true;
                Mode = OperatingMode.Manual;

                AddStatusMessage(StatusMessageTypes.Info, $"Opened Network Connection");
                MachineConnected?.Invoke(this, null);
                await Task.Run(() =>
                {
                    Work(socketClient.InputStream, socketClient.OutputStream);
                }, _cancelSource.Token);
            }
            catch (Exception ex)
            {
                AddStatusMessage(StatusMessageTypes.Warning, $"Could not connect to socket client: " + ex.Message);
                MachineDisconnected?.Invoke(this, null);
            }
        }

        private object _queueAccessLocker = new object();

        public async Task DisconnectAsync()
        {
            lock (this)
            {
                if (!Connected)
                {
                    AddStatusMessage(StatusMessageTypes.Warning, "Can not disconnected - Not Connected");
                    return;
                }

                Mode = OperatingMode.Disconnected;
                Connected = false;

                MachinePosition = new Vector3();
                WorkspacePosition = new Vector3();

                Status = "Disconnected";
                DistanceMode = ParseDistanceMode.Absolute;
                Unit = ParseUnit.Metric;
                Plane = ArcPlane.XY;
                UnacknowledgedBytesSent = 0;

                lock (_queueAccessLocker)
                {
                    _toSend.Clear();
                    _toSendPriority.Clear();
                    _sentQueue.Clear();
                }
            }

            if (_port != null)
            {
                _port.Close();
                AddStatusMessage(StatusMessageTypes.Info, "Closed Serial Port");
                _port = null;
            }

            if (_socketClient != null)
            {
                await _socketClient.CloseAsync();
                AddStatusMessage(StatusMessageTypes.Info, "Closed Socket Connection");
                _socketClient.Dispose();
                _socketClient = null;
            }

            if (this._cancelSource != null)
            {
                _cancelSource.Cancel();
                _cancelSource = null;
            }
            
            MachineDisconnected?.Invoke(this, null);

            AddStatusMessage(StatusMessageTypes.Info, "Disconnected");
        }

        private bool AssertConnected()
        {
            if (!Connected)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Not Connected");
                return false;
            }

            return true;
        }

        private bool AssertNotBusy()
        {
            if (Mode != OperatingMode.Manual && Mode != OperatingMode.Disconnected)
            {
                AddStatusMessage(StatusMessageTypes.Warning, "Busy");
                return false;
            }

            return true;
        }

        public int ToSendQueueCount
        {
            get
            {
                lock (_queueAccessLocker)
                {
                    return _toSend.Count;
                }
            }
        }

        public object MVLocatorState { get; private set; }

        /// <summary>
        /// Send CTRL-X to the machine
        /// </summary>
        public void SoftReset()
        {
            if (AssertConnected())
            {
                Mode = OperatingMode.Manual;

                lock (_queueAccessLocker)
                {
                    _toSend.Clear();
                    _toSendPriority.Clear();
                    _sentQueue.Clear();
                    _internalPendingQueue.Clear();
                    PendingQueue.Clear();
                    if (Settings.FirmwareType == FirmwareTypes.GRBL1_1 || Settings.FirmwareType == FirmwareTypes.GRBL1_1_SL_Custom)
                        _toSendPriority.Enqueue(((char)0x18).ToString());
                    else if (Settings.FirmwareType == FirmwareTypes.Repeteir_PnP)
                        _toSendPriority.Enqueue("M112");


                    VacuumPump = false;
                    PuffPump = false;
                    VacuumSolendoid = false;
                }

                UnacknowledgedBytesSent = 0;
            }
        }

        public void DebugWriteLine(string msg)
        {
            var now = DateTime.Now;
            Debug.WriteLine($"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}.{now.Millisecond:0000}] {msg}");
        }

        public void Enqueue(String cmd, bool highPriority = false)
        {
            if(cmd == null)
            {
                Messages.Add(StatusMessage.Create(StatusMessageTypes.Warning, "Null command, will not send."));
                return;
            }

            var cmds = cmd.Split('|');
            foreach (var m in cmds)
            {
                if (String.IsNullOrEmpty(m))
                    Debugger.Break();
            }

            if (AssertConnected())
            {
                    lock (_queueAccessLocker)
                    {
                        foreach (var gCodeCommands in cmds)
                        {
                            if (highPriority)
                            {
                                DebugWriteLine($"    [PRIORITYSEND] {gCodeCommands}");
                                _toSendPriority.Enqueue(gCodeCommands);
                            }
                            else
                            {
                                if (_toSend.Count < 255)
                                {
                                    if (String.IsNullOrEmpty(gCodeCommands))
                                        return;

                                    _toSend.Enqueue(gCodeCommands.Trim());
                                if (Settings.FirmwareType == FirmwareTypes.LagoVista_PnP ||
                                    Settings.FirmwareType == FirmwareTypes.SimulatedMachine ||
                                    Settings.FirmwareType == FirmwareTypes.LumenPnP_V4_Marlin ||
                                    Settings.FirmwareType == FirmwareTypes.Repeteir_PnP)
                                {
                                    _internalPendingQueue.Add(gCodeCommands);

                                    Services.DispatcherServices.Invoke(() =>
                                    {
                                        PendingQueue.Add(gCodeCommands);
                                    });
                                    }
                                }
                            }
                        }
                    }
            }
        }

        //TODO: Thinking we need a better "emergency stop"
        public void EmergencyStop()
        {
            SoftReset();
        }

        public async void GotoWorkspaceHome()
        {
            if (Settings.FirmwareType == FirmwareTypes.LagoVista_PnP)
            {
                Enqueue("M57");
            }
            else if (Settings.FirmwareType == FirmwareTypes.Repeteir_PnP)
            {
                Enqueue($"G0 Z{Settings.DefaultSafeMoveHeight} F{Settings.FastFeedRate}");
                await SetViewTypeAsync(ViewTypes.Camera);
                GotoPoint(0, 0);
                //ifSettings.PartInspectionCamera
                //Enqueue($"G0 Z{Settings.PartInspectionCamera.FocusHeight}");                
            }
            else
            {

                if (Settings.FirmwareType == FirmwareTypes.GRBL1_1 || Settings.FirmwareType == FirmwareTypes.GRBL1_1_SL_Custom)
                {
                    Enqueue($"G0 Z{Settings.DefaultSafeMoveHeight}");
                }

                Enqueue("G0 X0 Y0");

                if (Settings.FirmwareType == FirmwareTypes.GRBL1_1 || Settings.FirmwareType == FirmwareTypes.GRBL1_1_SL_Custom)
                {
                    Enqueue("G0 Z0");
                }

            }
        }

        public void GotoFiducialHome()
        {
            Enqueue("M53");
        }

        public void SetWorkspaceHome()
        {
            if (Settings.FirmwareType == FirmwareTypes.Repeteir_PnP)
            {
                //Settings.DefaultWorkspaceHome.X -= this.NormalizedPosition.X;
                //                Settings.DefaultWorkspaceHome.Y -= this.NormalizedPosition.Y;

                //await this.MachineRepo.SaveAsync();

                Enqueue("G92 X0 Y0");
            }
            else
            {
                Enqueue("M77");
            }
        }

        public void SetFavorite1()
        {
            Enqueue("M78");
        }

        public void SetFavorite2()
        {

            Enqueue("M79");
        }

        public void GotoFavorite1()
        {
            Enqueue("M58");
        }

        public void GotoFavorite2()
        {
            Enqueue("M59");
        }

        public async void HomingCycle()
        {
            var sw = Stopwatch.StartNew();
            DebugWriteLine("-------------------------");

            SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_Defauilt);
            SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_Defauilt);

            _viewType = ViewTypes.Camera;
            RaisePropertyChanged(nameof(ViewType));

            ConfigureTopLight(false, 0xff, 0xff, 0xff, 0xFF);
            ConfigureBottomLight(false, 0xff, 0xff, 0xff, 0xFF);
            LeftVacuumPump = false;
            RightVacuumPump = false;
            _currentMachineToolHead = null;
            RaisePropertyChanged(nameof(CurrentMachineToolHead));


            if (Settings.FirmwareType == FirmwareTypes.GRBL1_1 || Settings.FirmwareType == FirmwareTypes.GRBL1_1_SL_Custom)
            {
                Enqueue("$H\n", true);
                Enqueue("G92 X0 Y0 Z0");
            }
            else
            {
                VacuumPump = false;
                PuffPump = false;
                VacuumSolendoid = false;
                Enqueue("G28");
                if (Settings.FirmwareType == FirmwareTypes.Repeteir_PnP)
                {
                    Enqueue($"G0 X{Settings.DefaultWorkspaceHome.X} Y{Settings.DefaultWorkspaceHome.Y} F{Settings.FastFeedRate}");
                    GotoPoint(Settings.DefaultWorkspaceHome.X, Settings.DefaultWorkspaceHome.Y);
                    SetWorkspaceHome();
                }

                Enqueue("G92 A0 B0");
                await SpinUntilIdleAsync();
            }

            

            WasMachineHomed = true;

            DebugWriteLine($"[HomingCycle] Completed in {sw.Elapsed.TotalMilliseconds} ms");
            DebugWriteLine("-------------------------");
        }

        public void SetHomed()
        {
            WasMachineHomed = true;
        }

        public void SetToolHeadHeight(double height)
        {
            if (CurrentMachineToolHead == null)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, "to call GoToPlaceHeight, need to have a current tool head selected.");
                return;
            }

            if (_leftToolHead)
                Enqueue($"G0 ZL{height} F100000");
            else
                Enqueue($"G0 ZR{height} F100000");
        }        

        public void RotateToolHeadRelative(double angle)
        {
            if (CurrentMachineToolHead == null)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, "to call RotateToolHeadRelative, need to have a current tool head selected.");
                return;
            }

            SetRelativeMode();
            if (_leftToolHead)
            {
                Enqueue($"G0 A{angle}");
            }
            else
            {
                Enqueue($"G0 B{angle}");
            }
            SetAbsoluteMode();
         }

        public void RotateToolHeadAngleAbsolute(double angle)
        {
            if (CurrentMachineToolHead == null)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, "to call SetToolHeadAngleAbsolute, need to have a current tool head selected.");
                return;
            }

            if (_leftToolHead)
            {
                Enqueue($"G0 A{angle}");
            }
            else
            {
                Enqueue($"G0 B{angle}");
            }
        }

        public void ClearToolHeadAngle()
        {
            if (CurrentMachineToolHead == null)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, "to call ClearToolHeadAngle, need to have a current tool head selected.");
                return;
            }

            if (_leftToolHead)
            {
                Enqueue($"G92 A0");
            }
            else
            {
                Enqueue($"G92 B0");
            }
        }

        public async Task<InvokeResult<long>> ReadVacuumAsync()
        {
            if (CurrentMachineToolHead == null)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, "to call ReadVacuumAsync, need to have a current tool head selected.");
                return InvokeResult<long>.FromError("No current head.");
            }

            if (_leftToolHead)
                return await ReadLeftVacuumAsync();
            
            return await ReadRightVacuumAsync();
        }

        Int32? _leftPressure;
        Int32? _rightPressure;

        private void PressureLine_LineReceived(object sender, string e)
        {
            if (!VacuumM170Response.TryParse(e, out var leftRaw, out var rightRaw))
                return; // Malformed or unrelated lines must never satisfy a pressure request.

            _leftPressure = leftRaw;
            _rightPressure = rightRaw;
            _pressureReadSet.Set();

            // Some firmware versions report the pressure line instead of 'ok'.
            // Only remove an actual queued M170; do not dequeue another command.
            if (_sentQueue.Count > 0 && _sentQueue.Peek().Trim().Equals("M170",
                StringComparison.OrdinalIgnoreCase))
            {
                var sentLine = _sentQueue.Dequeue();
                UnacknowledgedBytesSent -= sentLine.Length + 1;
            }
        }

        ManualResetEventSlim _pressureReadSet = new ManualResetEventSlim(false);
        private readonly System.Threading.SemaphoreSlim _pressureReadGate =
            new System.Threading.SemaphoreSlim(1, 1);

        // One M170 returns both pressure channels. Register the listener before
        // sending and serialize requests to prevent cross-request contamination.
        async Task<InvokeResult> UpdatePressureAsync()
        {
            await _pressureReadGate.WaitAsync();
            try
            {
                _leftPressure = null;
                _rightPressure = null;
                _pressureReadSet.Reset();
                LineReceived += PressureLine_LineReceived;
                try
                {
                    SendCommand("M170");
                    for (var attempt = 0; attempt < 200 && !_pressureReadSet.IsSet; attempt++)
                        await Task.Delay(5);
                    return _pressureReadSet.IsSet && _leftPressure.HasValue && _rightPressure.HasValue
                        ? InvokeResult.Success
                        : InvokeResult.FromError("Timed out waiting for a fresh M170 pressure response.");
                }
                finally
                {
                    LineReceived -= PressureLine_LineReceived;
                }
            }
            finally
            {
                _pressureReadGate.Release();
            }
        }

        // https://cfsensor.com/wp-content/uploads/2022/11/XGZP6857D-Pressure-Sensor-V2.7.pdf
        public async Task<InvokeResult<long>> ReadLeftVacuumAsync()
        {
            if (Settings.VacuumSensorTransport?.Value != VacuumSensorTransportTypes.I2C)
            {
                var result = await UpdatePressureAsync();
                if(result.Successful && _leftPressure.HasValue)
                    return InvokeResult<long>.Create(_leftPressure.Value);

                return InvokeResult<long>.FromInvokeResult(result.ToInvokeResult());
            }
            else
            {
                // Select multiplexer At addr 112 - Left Pressure Monitor is at port 1
                I2CSend(112, 1);

                I2CSend(109, 0x30, 0x0A);

                await Task.Delay(20);

                // Requst MSB 23:16 Register: 0x06
                I2CSend(109, 6);
                var result = await I2CReadHexByte(109);
                if (!result.Successful) return InvokeResult<long>.FromInvokeResult(result.ToInvokeResult());
                var msb = result.Result;
                //msb = 0;
                // Request CSB 15-8 Register: 0x07
                I2CSend(109, 7);
                result = await I2CReadHexByte(109);
                if (!result.Successful) return InvokeResult<long>.FromInvokeResult(result.ToInvokeResult());
                var csb = result.Result;

                // Request CSB 7-0 Register: 0x08
                I2CSend(109, 8);
                result = await I2CReadHexByte(109);
                if (!result.Successful) return InvokeResult<long>.FromInvokeResult(result.ToInvokeResult());
                var lsb = result.Result;

                _leftPressure = (msb << 4 | csb >> 4);

                return InvokeResult<long>.Create((long)_leftPressure);
            }

            //return InvokeResult<ulong>.Create((ulong)(msb << 16 | csb << 8 | lsb));
            //return InvokeResult<ulong>.Create((ulong)(msb << 8 | csb));
        }

        public async Task<InvokeResult<long>> ReadRightVacuumAsync()
        {
            if (Settings.VacuumSensorTransport?.Value != VacuumSensorTransportTypes.I2C)
            {
                var result = await UpdatePressureAsync();
                if (result.Successful && _rightPressure.HasValue)
                    return InvokeResult<long>.Create(_rightPressure.Value);

                return InvokeResult<long>.FromInvokeResult(result.ToInvokeResult());
            }
            else
            {

                // Select multiplexer At addr 112 - right Pressure Monitor is at port 2
                I2CSend(112, 2);

                // Requst MSB 23:16 Register: 0x06
                I2CSend(109, 6);
                var result = await I2CReadHexByte(109);
                if (!result.Successful) return InvokeResult<long>.FromInvokeResult(result.ToInvokeResult());
                var msb = result.Result;

                //msb = 0;
                // Request CSB 15-8 Register: 0x07
                I2CSend(109, 7);
                result = await I2CReadHexByte(109);
                if (!result.Successful) return InvokeResult<long>.FromInvokeResult(result.ToInvokeResult());
                var csb = result.Result;

                // Request CSB 7-0 Register: 0x08
                I2CSend(109, 8);
                result = await I2CReadHexByte(109);
                if (!result.Successful) return InvokeResult<long>.FromInvokeResult(result.ToInvokeResult());
                var lsb = result.Result;

                _rightPressure = (msb << 4 | csb >> 4);

                //return InvokeResult<ulong>.Create((ulong)(msb));
                //return InvokeResult<ulong>.Create((ulong)(msb << 16 | csb << 8 | lsb));
                return InvokeResult<long>.Create((long)_rightPressure);
            }
        }

        public void HomeViaOrigin()
        {
            _viewType = ViewTypes.Camera;
            RaisePropertyChanged(nameof(ViewType));
            VacuumPump = false;
            PuffPump = false;
            VacuumSolendoid = false;
            Enqueue("G27");
        }

        public void SetAbsoluteWorkSpaceHome()
        {
            Settings.DefaultWorkspaceHome = new Point3D<double>(MachinePosition.X, MachinePosition.Y, MachinePosition.Z);
            SetWorkspaceHome();
        }

        public void FeedHold()
        {
            Enqueue("!", true);
        }

        public void ClearAlarm()
        {
            if (Settings.FirmwareType == FirmwareTypes.GRBL1_1 || Settings.FirmwareType == FirmwareTypes.GRBL1_1_SL_Custom)
            {
                Enqueue("$X\n", true);
            }
            else
            {
                _toSendPriority.Enqueue(((char)0x06).ToString());
            }
        }

        public void CycleStart()
        {
            Enqueue("~", true);
        }

        public void SpindleOn()
        {
            Enqueue($"M3 S{Settings.PowerOrRpm}");
        }

        public void SpindleOff()
        {
            Enqueue("M5");
        }

        public void LaserOff()
        {
            Enqueue("M5");
        }

        public void LaserOn()
        {
            Enqueue("M3");
        }
    }
}
