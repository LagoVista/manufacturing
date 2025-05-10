using System;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Models.Drawing;
using System.ComponentModel;
using LagoVista.PickAndPlace.Util;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.GCode;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Managers.PcbFab;
using System.Linq;
using Emgu.CV.DepthAI;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using LagoVista.Core.Validation;

namespace LagoVista.PickAndPlace
{
    public partial class Machine : IMachine
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<string> LineReceived;
        public event EventHandler MachineConnected;
        public event EventHandler MachineDisconnected;
        public event EventHandler SettingsLocked;
        public event EventHandler SettingsUnlocked;

        public Machine()
        {
            Settings = new Manufacturing.Models.Machine()
            {
                Name = "No Machine",
            };

            Messages = new System.Collections.ObjectModel.ObservableCollection<Models.StatusMessage>();
            SentMessages = new System.Collections.ObjectModel.ObservableCollection<Models.StatusMessage>();
            AddStatusMessage(StatusMessageTypes.Info, "Startup.");
          

        }

        public Task InitAsync()
        {
            IsInitialized = true;

            return Task.FromResult(default(object));
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            Services.DispatcherServices.Invoke(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))
            );
        }

        public void ClearQueue()
        {
            if (Mode != OperatingMode.Manual)
            {
                AddStatusMessage(StatusMessageTypes.Info, "Not in manual mode.");
                return;
            }

            lock (_toSend)
            {
                _toSend.Clear();
            }
        }

        //TODO: Removed Compiled Option
        private static Regex GCodeSplitter = new Regex(@"(G)\s*(\-?\d+\.?\d*)");

        /// <summary>
        /// Updates Status info from each line sent
        /// </summary
        /// <param name="line"></param>
        private void UpdateStatus(string line)
        {
            if (!Connected)
                return;

            //we use a Regex here so G91.1 etc don't get recognized as G91

            foreach (Match m in GCodeSplitter.Matches(line))
            {
                if (m.Groups[1].Value != "G")
                    continue;

                float code = float.Parse(m.Groups[2].Value);

                if (code == 17)
                    Plane = ArcPlane.XY;
                if (code == 18)
                    Plane = ArcPlane.YZ;
                if (code == 19)
                    Plane = ArcPlane.ZX;

                if (code == 20)
                    Unit = ParseUnit.Imperial;
                if (code == 21)
                    Unit = ParseUnit.Metric;

                if (code == 90)
                    DistanceMode = ParseDistanceMode.Absolute;
                if (code == 91)
                    DistanceMode = ParseDistanceMode.Relative;
            }
        }

        public void SetAbsoluteMode()
        {
            SendCommand("G90");
            DistanceMode = ParseDistanceMode.Absolute;
        
        }

        public void SetRelativeMode()
        {
            SendCommand("G91");
            DistanceMode = ParseDistanceMode.Relative;
        }


        public bool CanSetMode(OperatingMode mode)
        {
            return true;
        }

        public bool SetMode(OperatingMode mode)
        {
            Mode = mode;
            return true;
        }

        private bool _busy;
        public bool Busy
        {
            set
            {
                _busy = value;
                BusyStatus = value ? "Busy" : "Idle";
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(BusyStatus));
            }
            get => _busy;
        }

        public string BusyStatus { get; set; } = "-";

        public void GotoPoint(double x, double y, double feedRate)
        {
            SendCommand($"G1 X{x.ToDim()} Y{y.ToDim()} F{feedRate}");
        }

        public void GotoPoint(Point2D<double> point, bool rapidMove = true, bool relativeMove = false)
        {
            GotoPoint(point.X, point.Y, rapidMove, relativeMove);
        }

        public void GotoPoint(double x, double y, bool rapidMove = true, bool relativeMove = false)
        {
            var cmd = rapidMove ? "G0" : "G1";
            if (Settings.FirmwareType != FirmwareTypes.Repeteir_PnP && !relativeMove)
            {
                x = Math.Max(0, Math.Min(x, Settings.WorkAreaSize.X));
                y = Math.Max(0, Math.Min(y, Settings.WorkAreaSize.Y));
            }

            if (relativeMove)
                SetRelativeMode();

            SendCommand($"{cmd} X{x.ToDim()} Y{y.ToDim()} F{Settings.FastFeedRate} ");

            if (relativeMove)
                SetAbsoluteMode();
        }

        public Task<InvokeResult> ResetMachineCoordinates(Point2D<double> point)
        {
            Enqueue($"G92 X{point.X} Y{point.Y}");
            return Task.FromResult(InvokeResult.Success);
        }

        public void GotoPoint(double x, double y, double z, bool rapidMove = true)
        {
            var cmd = rapidMove ? "G0" : "G1";
            if (Settings.FirmwareType != FirmwareTypes.Repeteir_PnP)
            {
                x = Math.Max(0, Math.Min(x, Settings.WorkAreaSize.X));
                y = Math.Max(0, Math.Min(y, Settings.WorkAreaSize.Y));
            }

            SendCommand($"{cmd} X{x.ToDim()} Y{y.ToDim()} Z{z.ToDim()}");
        }

        public void SendSafeMoveHeight()
        {
            SendCommand($"G0 Z{Settings.SafMoveHeight}");
        }

        public void SetVisionProfile(CameraTypes cameraType, VisionProfileSource sourceType, string sourceId, VisionProfile profile)
        {
            var camera = Settings.Cameras.FirstOrDefault(cam => cam.CameraType.Value == cameraType);
            if (camera == null)
            {
                return;
            }
            else
            {
                camera.CurrentVisionProfile = profile;
                camera.ProfileSource = sourceType;
                camera.ProfileSourceId = sourceId;

                if (camera.CameraType.Value == CameraTypes.Position)
                {
                    ConfigureTopLight(camera.CurrentVisionProfile.LightOn, camera.CurrentVisionProfile.LightPower, camera.CurrentVisionProfile.LightRed, camera.CurrentVisionProfile.LightBlue, camera.CurrentVisionProfile.LightGreen);
                }
                else if (camera.CameraType.Value == CameraTypes.PartInspection)
                {
                    ConfigureBottomLight(camera.CurrentVisionProfile.LightOn, camera.CurrentVisionProfile.LightPower, camera.CurrentVisionProfile.LightRed, camera.CurrentVisionProfile.LightBlue, camera.CurrentVisionProfile.LightGreen);
                }
            }
        }

        public void SetVisionProfile(CameraTypes cameraType, string profileName)
        {
            var camera = Settings.Cameras.FirstOrDefault(cam => cam.CameraType.Value == cameraType);
            if (camera == null)
            {
                return;
            }
            else
            { 
                if(profileName == camera.CurrentVisionProfile.Key)
                {
                    return;
                }

                var profile = camera.VisionProfiles.Where(pf => pf.Key == profileName).SingleOrDefault();
                if (profile == null)
                {
                    var ehProfile = VisionProfile.DefaultVisionProfiles.FirstOrDefault(pf => pf.Key == profileName);
                    if (ehProfile == null)
                    {
                        AddStatusMessage(StatusMessageTypes.FatalError, $"Could not set vision profile, could not find camera of type: {cameraType}");
                    }
                    else
                    {
                        var defaultProfile = camera.VisionProfiles.FirstOrDefault(vp => vp.Key == VisionProfile.VisionProfile_Defauilt);
                        var newProfile = JsonConvert.DeserializeObject<VisionProfile>(JsonConvert.SerializeObject(defaultProfile));
                        newProfile.Key = ehProfile.Key;
                        newProfile.Id = ehProfile.Id;
                        newProfile.Name = ehProfile.Text;
                        camera.VisionProfiles.Add(newProfile);
                        camera.CurrentVisionProfile = newProfile;
                    }
                }
                else
                    camera.CurrentVisionProfile = profile;

                camera.ProfileSource = VisionProfileSource.Camera;
                camera.ProfileSourceId = null;

                if (camera.CameraType.Value == CameraTypes.Position)
                {
                    ConfigureTopLight(camera.CurrentVisionProfile.LightOn, camera.CurrentVisionProfile.LightPower, camera.CurrentVisionProfile.LightRed, camera.CurrentVisionProfile.LightBlue, camera.CurrentVisionProfile.LightGreen);
                }
                else if(camera.CameraType.Value == CameraTypes.PartInspection)
                {
                    ConfigureBottomLight(camera.CurrentVisionProfile.LightOn, camera.CurrentVisionProfile.LightPower, camera.CurrentVisionProfile.LightRed, camera.CurrentVisionProfile.LightBlue, camera.CurrentVisionProfile.LightGreen);
                }
            }
        }

        SemaphoreSlim _waitCurrent = new SemaphoreSlim(1);

        ManualResetEventSlim _waitForPositionResetEvent;
        ManualResetEventSlim _waitForIdleResetEvent = new ManualResetEventSlim();


        const int retryPause = 25;

        public async Task<InvokeResult<Point2D<double>>> GetCurrentLocationAsync(uint ms = 2500)
        {

            await _waitCurrent.WaitAsync();
            
            if(_waitForPositionResetEvent != null)
            {
                throw new InvalidOperationException(@"Already requested current location.");
            }

            _waitForPositionResetEvent = new ManualResetEventSlim(false);
            _waitForPositionResetEvent.Reset();

            var attemptCount = 0;
            var maxAttempts = ms / 25;
            await Task.Run(() =>
            {
                while (!_waitForPositionResetEvent.IsSet && ++attemptCount < ms / retryPause)
                {
                    _waitForPositionResetEvent.Wait(retryPause);                    
                }

                var timedOut = !_waitForPositionResetEvent.IsSet || attemptCount == maxAttempts;

                _waitForPositionResetEvent.Dispose();
                _waitForPositionResetEvent = null;
            });


            _waitCurrent.Release();

            return InvokeResult<Point2D<double>>.Create(MachinePosition.ToPoint2D());
        }

        bool _spinningWhileBusy = false;

        private void PendingBusyReader(Object sender, string line)
        {
            if (line == "ok")
                _waitForIdleResetEvent.Set();
            else
                Debug.WriteLine("wait some more => " + line);
        }

        public async Task<InvokeResult> SpinUntilIdleAsync(uint ms = 2500)
        {
            var start = DateTime.Now;

            Debug.WriteLine("------------------------ " + start);
            Debug.WriteLine("Start waiting...");
            _waitForIdleResetEvent = new ManualResetEventSlim();

            _waitForIdleResetEvent.Reset();

            var attemptCount = 0;
            var maxAttempts = ms / 25;

           
            await Task.Run(() =>
            {
                Enqueue("M400"); // Wait for previous command to finish before executing next one.
                LineReceived += PendingBusyReader;

                while (!_waitForIdleResetEvent.IsSet && ++attemptCount < ms / retryPause)
                {
                    _waitForIdleResetEvent.Wait(retryPause);
                }
            });

            LineReceived -= PendingBusyReader;

            var timedOut = !_waitForIdleResetEvent.IsSet || attemptCount == maxAttempts;

            _spinningWhileBusy = false;
            Debug.WriteLine("Done waiting: " + (DateTime.Now - start).TotalMilliseconds + " ms Attempt Count " + attemptCount + "  max " + maxAttempts);
            Debug.WriteLine("------------------------" + DateTime.Now);

            _waitForIdleResetEvent = null;

            return timedOut ? InvokeResult.FromError("Timed out") : InvokeResult.Success; 
        }

        public void Dwell(int ms)
        {
            this.SendCommand($"G4 P{ms}");
        }


        public Task GoToPartInspectionCameraAsync(double? rotate)
        {
            var partInspectionCamera = Settings.Cameras.FirstOrDefault(cam => cam.CameraType.Value == CameraTypes.PartInspection);
            if (partInspectionCamera != null)
            {
                if(rotate.HasValue)
                    SendCommand($"G0 X{partInspectionCamera.AbsolutePosition.X.ToDim()} Y{partInspectionCamera.AbsolutePosition.Y.ToDim()} {(_leftToolHead ? "A" : "B")}{-rotate.Value} F{Settings.FastFeedRate} ");
                else
                    SendCommand($"G0 X{partInspectionCamera.AbsolutePosition.X.ToDim()} Y{partInspectionCamera.AbsolutePosition.Y.ToDim()} F{Settings.FastFeedRate} ");


                if (partInspectionCamera.CurrentVisionProfile.DetectionHeight.HasValue)
                {
                    this.SendCommand($"G0 ZL{partInspectionCamera.CurrentVisionProfile.DetectionHeight.Value}");
                }
                else
                {
                    if (CurrentMachineToolHead != null)
                    {
                        if (_leftToolHead)
                            this.SendCommand($"G0 ZL{partInspectionCamera.FocusHeight}");
                        else
                            this.SendCommand($"G0 ZR{partInspectionCamera.FocusHeight}");
                    }
                }
            }
            else
            {
                this.AddStatusMessage(StatusMessageTypes.FatalError, "Could not find part inspection camera.");
            }

            return Task.CompletedTask;
        }



    }
}