// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c848055ccc1986eb171c129f36cb46a86ed4979a0b10368e1091ecf125f7abd8
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces;

using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using LagoVista.Core.Commanding;
using System;
using LagoVista.PickAndPlace.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Models;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class HeightMapManager : MachineViewModelBase, IHeightMapManager, IProbeCompletedHandler
    {        
        public HeightMapManager(IMachineRepo machineRepo, IPCBManager pcbManager, IGCodeFileManager gcodeFileManager, ILogger logger) : base (machineRepo)
        {
            SaveHeightMapCommand = CreateCommand(SaveHeightMap, CanSaveHeightMap);
            ApplyHeightMapCommand = CreateCommand(ApplyHeightMap, CanApplyHeightMap);
            ClearHeightMapCommand = CreateCommand(ClearHeightMap, CanClearHeightMap);
            StartProbeHeightMapCommand = CreateCommand(StartHeightMap, CanProbeHeightMap);
            OpenHeightMapCommand = CreateCommand(OpenHeightMapFile);

            GCodeFileManager = gcodeFileManager;
            PcbManager = pcbManager;
        }

        public async void SaveHeightMap()
        {
            var file = await Popups.ShowSaveFileAsync(Constants.FileFilterHeightMap);
            if (!String.IsNullOrEmpty(file))
            {
                await SaveHeightMapAsync(file);
                RaiseCanExecuteChanged();
            }
        }


        public async void OpenHeightMapFile()
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterHeightMap);
            if (!String.IsNullOrEmpty(file))
            {
                await OpenHeightMapAsync(file);
                RaiseCanExecuteChanged();
            }
        }

        private bool _hasSetFirstProbeOffsetToZero = false;

        private double _initialOffset;

        public async void ProbeCompleted(Vector3 position)
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
                return;

            if (!Machine.Connected || HeightMap == null)
            {
                CancelProbing();
                return;
            }

            if (HeightMap == null)
            {
                Logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "HeightMap_ProbeCompleted", "Probe Completed without valid Height Map.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Probe Height Map Completed without valid Height Map");
                Status = HeightMapStatus.NotAvailable;
                CancelProbing();
                return;
            }

            if (_currentPoint == null)
            {
                Logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "HeightMap_ProbeCompleted", "Probe Completed without Current Point.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Probe Height Map Completed without valid Current Point");
                Status = HeightMapStatus.NotPopulated;
                CancelProbing();
                return;
            }

            if (!_hasSetFirstProbeOffsetToZero)
            {
                _initialOffset = position.Z;
                HeightMap.SetPointHeight(_currentPoint, 0);
                _hasSetFirstProbeOffsetToZero = true;
                await Task.Delay(2000);
                Machine.AddStatusMessage(StatusMessageTypes.Info, $"First Completed - Zero Z Axis.");
            }
            else
            {
                var offset = position.Z - _initialOffset;
                Machine.AddStatusMessage(StatusMessageTypes.Info, $"Completed Point X={_currentPoint.Point.X.ToDim()}, Y={_currentPoint.Point.Y.ToDim()}, Z={offset.ToDim()}");
                HeightMap.SetPointHeight(_currentPoint, offset);
            }

            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Postion as returned {position.Z.ToDim()}.");

            DispatcherServices.Invoke(() =>
            {
                RaisePropertyChanged(nameof(HeightMap));
            });

            _currentPoint = null;

            if (HeightMap.Status == HeightMapStatus.Populated)
            {
                DispatcherServices.Invoke(async () =>
                {
                    Status = HeightMapStatus.Populated;
                    Machine.SendCommand($"G0 Z{Machine.Settings.ProbeSafeHeight.ToDim()}");
                    Machine.AddStatusMessage(StatusMessageTypes.Info, $"Creating Height Map Completed");
                    Machine.AddStatusMessage(StatusMessageTypes.Info, $"Next - Apply Height Map to GCode");
                    CancelProbing();

                    await Core.PlatformSupport.Services.Popups.ShowAsync("Capturing Height Map completed.  You can now save or apply this height map to your GCode.");
                });
            }
            else
            {
                HeightMapProbeNextPoint();
            }
        }

        public void NewHeightMap(HeightMap heightMap)
        {
            HeightMap = heightMap;
            HeightMap.Refresh();
            ConstructVisuals();

            RaiseCanExecuteChanged();
        }

        private void ConstructVisuals()
        {
            if (!HasHeightMap)
            {
                Logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "HeightMapManager_ConstructVisuals", "Attempt to construct visual w/o a height map.");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Attempt to construct visual w/o a height map.");
            }

            if (HeightMap.GridSize == 2.5)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Grid size must be creater than 2.5, current grid size {HeightMap.GridSize}.");
                return;
            }            
        }

        private string _heightMapPath;
        public async Task OpenHeightMapAsync(string path)
        {
            _heightMapPath = path;
            HeightMap = await Core.PlatformSupport.Services.Storage.GetAsync<HeightMap>(path);
            HeightMap.Initialized = true;
        }

        public async Task SaveHeightMapAsync(string path)
        {
            _heightMapPath = path;
            await Core.PlatformSupport.Services.Storage.StoreAsync(HeightMap, path);
        }

        public async Task SaveHeightMapAsync()
        {
            await Core.PlatformSupport.Services.Storage.StoreAsync(HeightMap, _heightMapPath);
        }

        public void CreateTestPattern()
        {
            var heightMap = new Models.HeightMap(Logger);
            if (PcbManager.HasBoard)
            {
                heightMap.Min = new Core.Models.Drawing.Vector2(PcbManager.Project.ScrapSides, PcbManager.Project.ScrapTopBottom);
                heightMap.Max = new Core.Models.Drawing.Vector2(PcbManager.Board.Width + PcbManager.Project.ScrapSides, PcbManager.Board.Height + PcbManager.Project.ScrapTopBottom);
                heightMap.GridSize = PcbManager.Project.HeightMapGridSize;
            }

            heightMap.Refresh();
            heightMap.FillWithTestPattern();
            HeightMap = heightMap;
        }



        public void CloseHeightMap()
        {
            HeightMap = null;
            RaiseCanExecuteChanged();
        }

        public void ProbeFailed()
        {
            Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Probe Failed! aborting");
            CancelProbing();
        }

        public void CancelProbing()
        {
            Machine.SetMode(OperatingMode.Manual);
            Machine.UnregisterProbeCompletedHandler();
            RaiseCanExecuteChanged();
            Machine.SendSafeMoveHeight();
        }

        HeightMapProbePoint _currentPoint;

        private void HeightMapProbeNextPoint()
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "No Longer in Probing Mode - Can't Continue.");
                CancelProbing();
                return;
            }

            if (!Machine.Connected)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine No Longer Connected - Can't Continue.");
                CancelProbing();
                return;
            }

            if (HeightMap == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Height Map Empty - Can't Continue.");
                CancelProbing();
                return;
            }

            if (HeightMap.Status == HeightMapStatus.Populated)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Unexpected Completion, Please Review before Continuing");
                Machine.SetMode(OperatingMode.Manual);
            }

            _currentPoint = HeightMap.GetNextPoint();
            if (_currentPoint == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "No Point Available - Can't Continue.");
                return;
            }

            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Probing Point X={_currentPoint.Point.X.ToDim()}, Y={_currentPoint.Point.Y.ToDim()}");

            Machine.SendCommand($"G0 Z{Machine.Settings.ProbeMinimumHeight.ToDim()}");
            Machine.SendCommand($"G0 X{_currentPoint.Point.X.ToDim()} Y{_currentPoint.Point.Y.ToDim()}");

            Machine.SendCommand($"G38.3 Z-{Machine.Settings.ProbeMaxDepth.ToDim()} F{Machine.Settings.ProbeFeed}");
        }

        public void StartProbing()
        {
            if (!Machine.Connected)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Not Connected - Can't start.");
                return;
            }

            if (Machine.Mode != OperatingMode.Manual)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine Busy - Can't start.");
                return;
            }

            if (!Machine.Connected || Machine.Mode != OperatingMode.Manual || HeightMap == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "No Height Map - Can't start.");
                return;
            }

            if (HeightMap.TotalPoints == 0)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Empty Height Map - Can't Start");
                return;
            }

            if (!Machine.SetMode(OperatingMode.ProbingHeightMap))
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine Couldn't Transition to Probe Mode.");
                return;
            }

            if (this.Machine.RegisterProbeCompletedHandler(this))
            {
                _hasSetFirstProbeOffsetToZero = false;

                Status = HeightMapStatus.Populating;

                Machine.AddStatusMessage(StatusMessageTypes.Info, "Creating Height Map - Started");

                Machine.SendCommand("G90");

                HeightMapProbeNextPoint();
            }

            RaiseCanExecuteChanged();
        }

        public void Reset()
        {
            if (HeightMap != null)
            {
                HeightMap.Reset();
                RaiseCanExecuteChanged();
            }
        }

        public void PauseProbing()
        {
            if (Machine.Mode != OperatingMode.ProbingHeightMap) 
            {
                RaiseCanExecuteChanged();
                return;
            }
        }


        public async void ApplyHeightMap()
        {
            if (CanApplyHeightMap())
            {
                if (GCodeFileManager.HeightMapApplied)
                {
                    if (await Popups.ConfirmAsync("Height Map", "Height Map has already been applied, doing so again will likely produce incorrect results.\r\n\r\nYou should reload the original GCode File then re-apply the height map.\r\n\r\nContinue? "))
                    {
                        GCodeFileManager.ApplyHeightMap(HeightMap);
                    }
                }
                else
                    GCodeFileManager.ApplyHeightMap(HeightMap);

                RaiseCanExecuteChanged();
            }
        }


        public bool CanApplyHeightMap()
        {
            return GCodeFileManager.HasValidFile &&
                   HasHeightMap &&
                  HeightMap.Status == Models.HeightMapStatus.Populated;
        }



        public void StartHeightMap()
        {
            StartProbing();
        }


        public void ClearHeightMap()
        {
            CloseHeightMap();
        }


        public bool CanSaveHeightMap()
        {
            return HasHeightMap &&
                  HeightMap.Status == Models.HeightMapStatus.Populated;
        }

        public bool CanClearHeightMap()
        {
            return HasHeightMap &&
                  HeightMap.Status == Models.HeightMapStatus.Populated;
        }


        public bool CanProbeHeightMap()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                MachineRepo.CurrentMachine.Connected
                && MachineRepo.CurrentMachine.Mode == OperatingMode.Manual
                && HasHeightMap;
        }

        public bool CanCancel()
        {
            return MachineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeightMap;
        }

        private HeightMap _heightMap;
        public HeightMap HeightMap
        {
            get { return _heightMap; }
            set
            {
                _heightMap = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasHeightMap));
                RaisePropertyChanged(nameof(Status));
            }
        }

        public HeightMapStatus Status
        {
            get
            {
                return (HasHeightMap) ? HeightMap.Status : HeightMapStatus.NotAvailable;
            }
            set
            {
                if (HasHeightMap)
                {
                    HeightMap.Status = value;
                }

                RaisePropertyChanged();
            }
        }

        public bool HasHeightMap
        {
            get { return _heightMap != null; }
        }

        private bool _heightMapDirty = false;
        public bool HeightMapDirty
        {
            get { return _heightMapDirty; }
            set
            {
                _heightMapDirty = value;
                RaisePropertyChanged();
            }
        }

        public IPCBManager PcbManager { get; }
        public IGCodeFileManager GCodeFileManager { get; }

        public ObservableCollection<LagoVista.Core.Models.Drawing.Line3D> RawBoardOutline { get; private set; }

        /// <summary>
        /// The XY Coordinates of the points that will be probed.
        /// </summary>
        public ObservableCollection<Vector3> ProbePoints { get; private set; }

        public RelayCommand OpenHeightMapCommand { get; private set; }
        public RelayCommand SaveHeightMapCommand { get; private set; }
        public RelayCommand ApplyHeightMapCommand { get; private set; }
        public RelayCommand ClearHeightMapCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }
        public RelayCommand CancelProbeHeightMapCommand { get; private set; }
    }
}
