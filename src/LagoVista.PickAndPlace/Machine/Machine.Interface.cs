﻿using Emgu.CV.CvEnum;
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

                if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
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
                    PendingQueue.Clear();
                    if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                        _toSendPriority.Enqueue(((char)0x18).ToString());
                    else if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                        _toSendPriority.Enqueue("M112");


                    VacuumPump = false;
                    PuffPump = false;
                    VacuumSolendoid = false;
                }

                UnacknowledgedBytesSent = 0;
            }
        }

        public void Enqueue(String cmd, bool highPriority = false)
        {
            if(cmd == null)
            {
                Messages.Add(StatusMessage.Create(StatusMessageTypes.Warning, "Null command, will not send."));
                return;
            }

            if (AssertConnected())
            {
                Services.DispatcherServices.Invoke(() =>
                {
                    lock (_queueAccessLocker)
                    {
                        var cmds = cmd.Split('|');
                        foreach (var cmd in cmds)
                        {
                            if (highPriority)
                            {
                                _toSendPriority.Enqueue(cmd);
                            }
                            else
                            {
                                _toSend.Enqueue(cmd);
                                if (Settings.MachineType == FirmwareTypes.LagoVista_PnP ||
                                    Settings.MachineType == FirmwareTypes.SimulatedMachine ||
                                    Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                                    PendingQueue.Add(cmd);
                            }
                        }
                    }
                    });
            }
        }

        //TODO: Thinking we need a better "emergency stop"
        public void EmergencyStop()
        {
            SoftReset();
        }

        public async void GotoWorkspaceHome()
        {
            if (Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                Enqueue("M57");
            }
            else if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
            {
                Enqueue($"G0 Z{Settings.DefaultSafeMoveHeight} F{Settings.FastFeedRate}");
                await SetViewTypeAsync(ViewTypes.Camera);
                GotoPoint(0, 0);
                //ifSettings.PartInspectionCamera
                //Enqueue($"G0 Z{Settings.PartInspectionCamera.FocusHeight}");                
            }
            else
            {

                if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                {
                    Enqueue($"G0 Z{Settings.DefaultSafeMoveHeight}");
                }

                Enqueue("G0 X0 Y0");

                if (Settings.MachineType == FirmwareTypes.GRBL1_1)
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
            if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
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

        public void HomingCycle()
        {
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

            if (Settings.MachineType == FirmwareTypes.GRBL1_1)
            {
                Enqueue("$H\n", true);
            }
            else
            {
                VacuumPump = false;
                PuffPump = false;
                VacuumSolendoid = false;
                Enqueue("G28");
                if (Settings.MachineType == FirmwareTypes.Repeteir_PnP)
                {
                    Enqueue($"G0 X{Settings.DefaultWorkspaceHome.X} Y{Settings.DefaultWorkspaceHome.Y} F{Settings.FastFeedRate}");
                    GotoPoint(Settings.DefaultWorkspaceHome.X, Settings.DefaultWorkspaceHome.Y);
                    SetWorkspaceHome();
                }
            }

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
                Enqueue($"G0 ZL{height}");
            else
                Enqueue($"G0 ZR{height}");
        }        

        public void RotateToolHead(double angle)
        {
            if (CurrentMachineToolHead == null)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, "to call RotateToolHead, need to have a current tool head selected.");
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

        public async Task<InvokeResult<ulong>> ReadVacuumAsync()
        {
            if (CurrentMachineToolHead == null)
            {
                AddStatusMessage(StatusMessageTypes.FatalError, "to call ReadVacuumAsync, need to have a current tool head selected.");
                return InvokeResult<ulong>.FromError("No current head.");
            }

            if (_leftToolHead)
                return await ReadLeftVacuumAsync();
            
            return await ReadRightVacuumAsync();
        }

        // https://cfsensor.com/wp-content/uploads/2022/11/XGZP6857D-Pressure-Sensor-V2.7.pdf
        public async Task<InvokeResult<ulong>> ReadLeftVacuumAsync()
        {
            // Select multiplexer At addr 112 - Left Pressure Monitor is at port 1
            I2CSend(112, 1);

            I2CSend(109, 0x30, 0x0A);

            await Task.Delay(20);

            // Requst MSB 23:16 Register: 0x06
            I2CSend(109, 6);
            var result = await I2CReadHexByte(109);
            if (!result.Successful) return InvokeResult<ulong>.FromInvokeResult(result.ToInvokeResult());
            var msb = result.Result;
            //msb = 0;
            // Request CSB 15-8 Register: 0x07
            I2CSend(109, 7);
            result = await I2CReadHexByte(109);
            if (!result.Successful) return InvokeResult<ulong>.FromInvokeResult(result.ToInvokeResult());
            var csb = result.Result;

            // Request CSB 7-0 Register: 0x08
            I2CSend(109, 8);
            result = await I2CReadHexByte(109);
            if (!result.Successful) return InvokeResult<ulong>.FromInvokeResult(result.ToInvokeResult());
            var lsb = result.Result;

            return InvokeResult<ulong>.Create((ulong)(msb << 4 | csb >> 4));

            //return InvokeResult<ulong>.Create((ulong)(msb << 16 | csb << 8 | lsb));
            //return InvokeResult<ulong>.Create((ulong)(msb << 8 | csb));
        }

        public async Task<InvokeResult<ulong>> ReadRightVacuumAsync()
        {
            // Select multiplexer At addr 112 - right Pressure Monitor is at port 2
            I2CSend(112, 2);

            // Requst MSB 23:16 Register: 0x06
            I2CSend(109, 6);
            var result = await I2CReadHexByte(109);
            if (!result.Successful) return InvokeResult<ulong>.FromInvokeResult(result.ToInvokeResult());
            var msb = result.Result;
           
            //msb = 0;
            // Request CSB 15-8 Register: 0x07
            I2CSend(109, 7);
            result = await I2CReadHexByte(109);
            if (!result.Successful) return InvokeResult<ulong>.FromInvokeResult(result.ToInvokeResult());
            var csb = result.Result;

            // Request CSB 7-0 Register: 0x08
            I2CSend(109, 8);
            result = await I2CReadHexByte(109);
            if (!result.Successful) return InvokeResult<ulong>.FromInvokeResult(result.ToInvokeResult());
            var lsb = result.Result;

            //return InvokeResult<ulong>.Create((ulong)(msb));
            //return InvokeResult<ulong>.Create((ulong)(msb << 16 | csb << 8 | lsb));
            return InvokeResult<ulong>.Create((ulong)(msb << 4 | csb >> 4));
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
            if (Settings.MachineType == FirmwareTypes.GRBL1_1)
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
            Enqueue("M3 S1000");
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
