// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0db35afca4dfeee31ff2f116484fa37322d007b66151e1e1999b79a7589c4323
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.DepthAI;
using Emgu.CV.Stitching;
using Emgu.CV.XImgproc;
using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels.Vision;
using RingCentral;
using SixLabors.ImageSharp.Formats.Bmp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class AutoFeederViewModel : FeederViewModel, IAutoFeederViewModel
    {

        private bool _isEding = false;
        private readonly IRestClient _restClient;
        ObservableCollection<AutoFeeder> _autoFeeders;

        public AutoFeederViewModel(IMachineRepo machineRepo, IPhotonFeederViewModel photonFeederViewModel, ILocatorViewModel locatorViewModel, IRestClient restClient, IMachineUtilitiesViewModel machineUtilitiesViewModel) : base(machineRepo, restClient, locatorViewModel, machineUtilitiesViewModel)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            PhotonFeederViewModel = photonFeederViewModel ?? throw new ArgumentNullException(nameof(photonFeederViewModel));
            PhotonFeederViewModel.PropertyChanged += PhotonFeederViewModel_PropertyChanged;

            CreateAutoFeederFromTemplateCommand = CreateCommand(CreateAutoFeederFromTemplateAsync, () => !string.IsNullOrEmpty(SelectedTemplateId));
            SetFeederFiducialLocationCommand = CreatedMachineConnectedSettingsCommand(async () => await CalibrateFeederPlacementAsync(), CanCalibratePlacement);
            SetPartPickLocationCommand = CreatedMachineConnectedSettingsCommand(SetPickLocationCommand, () => Current != null && !_placementRunning && !_saving);

            GoToFiducialCommand = CreatedMachineConnectedCommand(GoToFeederFiducial, () => ValidCurrent && !_placementRunning && !_saving);
            GoToRailReferenceCommand = CreateCommand(async () => await NavigateGeometryAsync(GeometryTarget.RailReference), CanNavigateGeometry);
            GoToSlotOriginCommand = CreateCommand(async () => await NavigateGeometryAsync(GeometryTarget.SlotOrigin), CanNavigateGeometry);
            GoToCalculatedFiducialCommand = CreateCommand(async () => await NavigateGeometryAsync(GeometryTarget.Fiducial), CanNavigateGeometry);
            GoToCalculatedPickupCommand = CreateCommand(async () => await NavigateGeometryAsync(GeometryTarget.Pickup), CanNavigateGeometry);
            RefreshGeometryCommand = CreateCommand(NotifyGeometryChanged);
            PreviewGeometryCommand = CreateCommand(async () => await PreviewGeometryAsync(), CanNavigateGeometry);
            GoToPickLocationCommand = CreatedMachineConnectedCommand(GoToPickLocation, () => Current != null && !_placementRunning && !_saving);

            InitializeFeederCommand = CreatedMachineConnectedCommand(async () => await TestConfiguredAsync(null), CanTestConfigured);
            AdvanceFeedCommand = CreatedMachineConnectedCommand(async () => await TestConfiguredAsync(true), CanMoveConfigured);
            RetractFeedCommand = CreatedMachineConnectedCommand(async () => await TestConfiguredAsync(false), CanMoveConfigured);

            AddCommand = CreateCommand(Add, () => MachineRepo.HasValidMachine && SelectedTemplateId.HasValidId() && CurrentPhotonFeeder != null);
            SaveCommand = CreateCommand(Save, () => Current != null && !_placementRunning && !_saving);
            SavePlacementCommand = CreateCommand(async () => await SaveCoreAsync(true), () => HasPlacementCandidate && !PlacementBusy);
            CancelCommand = CreateCommand(Cancel, () => Current != null && !_placementRunning && !_saving);
            ReloadFeederCommand = CreateCommand(ReloadFeeder, () => Current != null && !_placementRunning && !_saving);
        }

        protected async override void OnMachineConnected()
        {
            base.OnMachineConnected();

            if (_autoFeeders == null) return;
            foreach(var feeder in _autoFeeders.ToArray())
            {
                if (Machine == null || !Machine.Connected) break;
                if (feeder.Slot < 1 || feeder.Slot > 254) continue;
                var result = await PhotonFeederViewModel.InitializeFeederAsync((byte)feeder.Slot, feeder.FeederId);
                if (result.Successful)
                    Machine.AddStatusMessage(StatusMessageTypes.Info, $"Initialized feeder {feeder.Name}");
            }
        }

        private void PhotonFeederViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Machine == null || !Machine.Connected || Machine.Mode != OperatingMode.Manual ||
                e.PropertyName == nameof(IPhotonFeederViewModel.SelectedPhotonFeeder)) ConfiguredTestArmed = false;
            RaisePropertyChanged(nameof(ConfiguredFeederStatus));
            RaiseCanExecuteChanged();
            if(e.PropertyName == nameof(IPhotonFeederViewModel.SelectedPhotonFeeder))
            {
                CurrentPhotonFeeder = PhotonFeederViewModel.SelectedPhotonFeeder;
                RaiseCanExecuteChanged();
            }
        }       

        private async void Add()
        {
            if(PhotonFeederViewModel.SelectedPhotonFeeder == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Please select a feeder before attempting to add it.");
                return;
            }

            var feederId = PhotonFeederViewModel.SelectedPhotonFeeder.Address;
            var slot = PhotonFeederViewModel.SelectedPhotonFeeder.Slot;

            var existingFeeder = _autoFeeders.FirstOrDefault(fdr => fdr.FeederId == feederId);
            if(existingFeeder != null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"A feeder with that ID alread exists {existingFeeder.Name}.");
                return;
            }

            var rail = MachineConfiguration.FeederRails.FirstOrDefault(fdr => slot >= fdr.SlotStartIndex && slot < (fdr.SlotStartIndex + fdr.NumberSlots));

            if (rail == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Invalid slot index {slot}");
                return;
            }

            var result = await _restClient.GetAsync<DetailResponse<AutoFeeder>>($"/api/mfg/autofeeder/template/{SelectedTemplateId}/factory");
           
            if (result.Successful)
            {
                Current = result.Result.Model;
                Current.FeederId = feederId;
                Current.Rotation = rail.Rotation;
                Current.Slot = slot;
                Current.Key = $"addr{feederId.ToLower()}";
                Current.Machine = MachineConfiguration.ToEntityHeader();
                _isEding = false;
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }

        public override async Task<InvokeResult> CenterOnPartAsync()
        {
            if (CurrentComponent == null)
            {
                return InvokeResult.FromError("Could not center on part.");
            }

            lock (this)
            {
                if (_waitForCenter != null)
                {
                    return InvokeResult.FromError("Already attempting to center part.");
                }

                _waitForCenter = new ManualResetEventSlim(false);
            }

            _feederOffset = new Point2D<double>();
            
            var toolHead = Machine.CurrentMachineToolHead;
            await Machine.MoveToCameraAsync();

            if (CurrentComponent.ComponentPackage.Value.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.ComponentPackage, CurrentComponent.ComponentPackage.Id,
                    CurrentComponent.ComponentPackage.Value.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInClearTape);

            _locatorViewModel.RegisterRectangleLocatedHandler(this);

            var success = false;

            await Task.Run(() =>
            {
                var attemptCount = 0;
                while (!_waitForCenter.IsSet && ++attemptCount < 200)
                    _waitForCenter.Wait(25);
                
                success = _waitForCenter.IsSet;
                _waitForCenter.Dispose();
                _waitForCenter = null;
            });

            _locatorViewModel.UnregisterRectangleLocatedHandler(this);

            if (toolHead != null)
                await Machine.MoveToToolHeadAsync(toolHead);

            if (success)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Info, $"Centered on part in feeder for {CurrentComponent.Name}.");
                return InvokeResult.Success;
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Could not center on part in feeder for {CurrentComponent.Name}.");
                return InvokeResult.FromError($"Could not center on part in feeder for {CurrentComponent.Name}.");
            }
        }

        public override Task<InvokeResult> CenterOnPartAsync(Component component)
        {
            CurrentComponent = component;
            return CenterOnPartAsync();
        }


        private async void ReloadFeeder()
        {
            if (Current == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Can not reload feeder, no feeder selected.");
            }
            else
            {
                var selected = Current;
                var result = await _restClient.GetAsync<DetailResponse<AutoFeeder>>($"/api/mfg/autofeeder/{selected.Id}");
                if (result.Successful)
                {
                    if (!ReferenceEquals(Current, selected)) return;
                    var index = _autoFeeders.IndexOf(selected);
                    Current = result.Result.Model;
                    if (index >= 0) _autoFeeders[index] = result.Result.Model;
                }
            }
        }

        public override async Task InitAsync()
        {
            await LoadTemplates();
            await base.InitAsync();
        }

        private async Task LoadTemplates()
        {
            var result = await _restClient.GetListResponseAsync<AutoFeederTemplateSummary>("/api/mfg/autofeeder/templates");
            if (result.Successful)
            {
                var feeders = result.Model.ToList();
                feeders.Insert(0, new AutoFeederTemplateSummary() { Id = StringExtensions.NotSelectedId, Name = "-select feeder template-" });
                Templates = new ObservableCollection<AutoFeederTemplateSummary>(feeders);
            }

            SelectedTemplateId = StringExtensions.NotSelectedId;
        }

        protected override void OnMachineDisconnected()
        {
            ClearPlacementCandidate();
            ConfiguredTestArmed = false;
            base.OnMachineDisconnected();
        }

        protected async override void MachineChanged(IMachine machine)
        {
            ClearPlacementCandidate();
            ConfiguredTestArmed = false;
            if (MachineRepo.HasValidMachine)
            {
                await LoadFeeders();
            }
            base.MachineChanged(machine);
        }


        private MachineFeederRail GetRail()
        {
            if (Current == null || MachineConfiguration?.FeederRails == null) return null;
            var rails = MachineConfiguration.FeederRails.Where(rl =>
                AutoFeederPlacementMath.ContainsSlot(rl.SlotStartIndex, rl.NumberSlots, Current.Slot)).ToArray();
            return rails.Length == 1 ? rails[0] : null;
        }

        public InvokeResult<Point2D<double>> GetCurrentFeederOrigin()
        {
            if (Current == null) return InvokeResult<Point2D<double>>.FromError("No current auto feeder");

            var rail = GetRail();
            if (rail == null) return InvokeResult<Point2D<double>>.FromError($"Could not find feeder rail for slot ${Current.Slot}.");

            if (rail.Rotation == null || !AutoFeederPlacementMath.TryOrigin(rail.FirstFeederOrigin,
                rail.SlotStartIndex, rail.NumberSlots, Current.Slot, rail.SlotWidth, rail.Rotation.Value, out var origin))
                return InvokeResult<Point2D<double>>.FromError("Invalid rail geometry or unsupported rail rotation.");
            return InvokeResult<Point2D<double>>.Create(origin);
        }

        public InvokeResult<Point2D<double>> FindFeederFiducial()
        {
            var origin = GetCurrentFeederOrigin();
            if (origin.Successful)
            {
                if (!BoardRigidTransform.Valid(Current.FiducialOffset))
                    return InvokeResult<Point2D<double>>.FromError("No valid feeder fiducial offset.");
                var fiducialLocation = origin.Result + Current.FiducialOffset;
                return InvokeResult<Point2D<double>>.Create(fiducialLocation);
            }
            else
                return origin;
        }

        // Geometry diagnostics deliberately use the same production origin and
        // offset functions as calibration/pickup. No motion on selection.
        private enum GeometryTarget { RailReference, SlotOrigin, Fiducial, Pickup }

        private static string FormatGeometry(Point2D<double> point) =>
            BoardRigidTransform.Valid(point) ? $"X {point.X:F3}   Y {point.Y:F3}" : "Unavailable";

        private static string FormatGeometry(InvokeResult<Point2D<double>> result) =>
            result.Successful ? FormatGeometry(result.Result) : "Unavailable";

        public string GeometryRailSummary
        {
            get
            {
                var rail = GetRail();
                return rail == null ? "No unique configured rail" :
                    $"{rail.Name}  |  {rail.Rotation?.Value.ToString() ?? "Unknown orientation"}  |  {rail.SlotWidth:F3} mm pitch  |  First slot {rail.SlotStartIndex}";
            }
        }
        public string GeometryRailReference => FormatGeometry(GetRail()?.FirstFeederOrigin);
        public string GeometrySlotOrigin => FormatGeometry(GetCurrentFeederOrigin());
        public string GeometryFiducial => FormatGeometry(FindFeederFiducial());
        public string GeometryPickup => FormatGeometry(FindPickLocation());
        public string GeometryFiducialOffset => FormatGeometry(Current?.FiducialOffset);
        public string GeometryPickOffset => FormatGeometry(Current?.PickOffset);
        public string GeometryPreflight
        {
            get
            {
                if (Current == null) return "Select a configured feeder.";
                if (Machine == null || !Machine.Connected) return "Connect the machine.";
                if (!Machine.WasMachineHomed) return "Home the machine.";
                if (!Machine.WasMachinOriginCalibrated) return "Calibrate machine origin.";
                if (Machine.Mode != OperatingMode.Manual) return "Switch the machine to Manual.";
                if (Machine.LeftVacuumPump || Machine.RightVacuumPump) return "Switch vacuum off.";
                if (!ConfiguredTestArmed) return "Confirm safe machine clearance below.";
                if (PlacementBusy || PhotonFeederViewModel.IsBusBusy || Machine.IsLocating) return "Operation in progress.";
                return "Ready for requested navigation. Live identity is checked before movement.";
            }
        }

        private void NotifyGeometryChanged()
        {
            RaisePropertyChanged(nameof(GeometryRailSummary));
            RaisePropertyChanged(nameof(GeometryRailReference));
            RaisePropertyChanged(nameof(GeometrySlotOrigin));
            RaisePropertyChanged(nameof(GeometryFiducial));
            RaisePropertyChanged(nameof(GeometryPickup));
            RaisePropertyChanged(nameof(GeometryFiducialOffset));
            RaisePropertyChanged(nameof(GeometryPickOffset));
            RaisePropertyChanged(nameof(GeometryPreflight));
        }

        private bool CanNavigateGeometry() => !PlacementBusy && ValidCurrent && Machine != null && Machine.Connected;

        private InvokeResult<Point2D<double>> GeometryPoint(GeometryTarget target)
        {
            switch (target)
            {
                case GeometryTarget.RailReference:
                    var rail = GetRail();
                    return rail != null && BoardRigidTransform.Valid(rail.FirstFeederOrigin)
                        ? InvokeResult<Point2D<double>>.Create(rail.FirstFeederOrigin)
                        : InvokeResult<Point2D<double>>.FromError("Rail reference unavailable.");
                case GeometryTarget.SlotOrigin: return GetCurrentFeederOrigin();
                case GeometryTarget.Fiducial: return FindFeederFiducial();
                default: return FindPickLocation();
            }
        }

        private void RequireGeometryIdle(IMachine machine, AutoFeeder feeder)
        {
            RequirePlacementMachine(machine, feeder);
            if (!ConfiguredTestArmed || machine.Busy || machine.ToSendQueueCount != 0 || machine.UnacknowledgedBytesSent != 0)
                throw new InvalidOperationException("Confirm clearance and wait for the machine to be idle.");
            if (!_isEding || !Feeders.Contains(feeder))
                throw new InvalidOperationException("Select a saved feeder before navigation.");
        }

        private async Task NavigateGeometryAsync(GeometryTarget target)
        {
            if (!CanNavigateGeometry()) return;
            var machine = Machine;
            var feeder = Current;
            var slot = feeder.Slot;
            var id = feeder.FeederId;
            var generation = Volatile.Read(ref _placementGeneration);
            _placementRunning = true;
            RaiseCanExecuteChanged();
            try
            {
                RequireGeometryIdle(machine, feeder);
                var result = GeometryPoint(target);
                if (!result.Successful || !BoardRigidTransform.InWorkArea(result.Result, machine.Settings.WorkAreaSize))
                    throw new InvalidOperationException("Calculated target is missing or outside the work area.");
                var point = new Point2D<double>(result.Result.X, result.Result.Y);
                void Check()
                {
                    RequireGeometryIdle(machine, feeder);
                    var actual = GeometryPoint(target);
                    if (generation != Volatile.Read(ref _placementGeneration) || feeder.Slot != slot || feeder.FeederId != id ||
                        !actual.Successful || !BoardRigidTransform.InWorkArea(actual.Result, machine.Settings.WorkAreaSize) ||
                        BoardRigidTransform.Distance(actual.Result, point) > 0.000001)
                        throw new InvalidOperationException("Feeder or calculated target changed. Request navigation again.");
                }

                PlacementStatus = $"Checking slot {slot} identity before moving to {target}.";
                var verified = await PhotonFeederViewModel.VerifyFeederIdentityAsync((byte)slot, id);
                if (!verified.Successful) throw new InvalidOperationException(verified.ErrorMessage);
                await PlacementIdleAsync(machine, feeder);
                Check();
                var capture = machine.PositionImageCaptureService;
                if (capture?.Camera?.CameraType?.Value != CameraTypes.Position)
                    throw new InvalidOperationException("Position camera unavailable.");
                machine.SetVisionProfile(CameraTypes.Position, target == GeometryTarget.Fiducial
                    ? VisionProfile.VisionProfile_FeederLandmark : VisionProfile.VisionProfile_Defauilt);
                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful) throw new InvalidOperationException(ready.ErrorMessage);
                Check();
                machine.SendSafeMoveHeight();
                await PlacementIdleAsync(machine, feeder);
                Check();
                await machine.MoveToCameraAsync();
                await PlacementIdleAsync(machine, feeder);
                Check();
                if (machine.CurrentMachineToolHead != null) throw new InvalidOperationException("Camera navigation unavailable.");
                PlacementStatus = $"Moving to {target}: {FormatGeometry(point)}.";
                machine.GotoPoint(point);
                await PlacementIdleAsync(machine, feeder);
                Check();
                var location = await machine.GetCurrentLocationAsync(5000);
                await PlacementIdleAsync(machine, feeder);
                Check();
                if (!location.Successful || !BoardRigidTransform.Valid(location.Result))
                    throw new InvalidOperationException("Could not verify arrived position.");
                PlacementStatus = $"Requested {FormatGeometry(point)}; actual {FormatGeometry(location.Result)}; difference {BoardRigidTransform.Distance(point, location.Result):F3} mm. Inspect the live camera; no offsets changed.";
                machine.TopLightOn = true; // Existing bounded 30-second preview.
            }
            catch (Exception ex)
            {
                PlacementStatus = "Navigation stopped: " + ex.Message;
                machine?.AddStatusMessage(StatusMessageTypes.Warning, PlacementStatus);
            }
            finally { _placementRunning = false; NotifyGeometryChanged(); RaiseCanExecuteChanged(); }
        }

        private async Task PreviewGeometryAsync()
        {
            if (!CanNavigateGeometry()) return;
            var machine = Machine;
            var feeder = Current;
            _placementRunning = true;
            RaiseCanExecuteChanged();
            try
            {
                RequireGeometryIdle(machine, feeder);
                var capture = machine.PositionImageCaptureService;
                if (capture?.Camera?.CameraType?.Value != CameraTypes.Position)
                    throw new InvalidOperationException("Position camera unavailable.");
                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful) throw new InvalidOperationException(ready.ErrorMessage);
                RequireGeometryIdle(machine, feeder);
                machine.TopLightOn = true;
                PlacementStatus = "Position camera preview requested (30-second illumination).";
            }
            catch (Exception ex) { PlacementStatus = "Preview unavailable: " + ex.Message; }
            finally { _placementRunning = false; RaiseCanExecuteChanged(); }
        }

        public RelayCommand GoToRailReferenceCommand { get; }
        public RelayCommand GoToSlotOriginCommand { get; }
        public RelayCommand GoToCalculatedFiducialCommand { get; }
        public RelayCommand GoToCalculatedPickupCommand { get; }
        public RelayCommand RefreshGeometryCommand { get; }
        public RelayCommand PreviewGeometryCommand { get; }

        public async void GoToFeederFiducial()
        {
            if (_placementRunning || _saving) return;
            var machine = Machine;
            var feeder = Current;
            var result = FindFeederFiducial();
            try
            {
                RequirePlacementMachine(machine, feeder);
                if (!result.Successful || !BoardRigidTransform.InWorkArea(result.Result, machine.Settings.WorkAreaSize))
                    throw new InvalidOperationException("Invalid or out-of-bounds feeder fiducial target.");
                _placementRunning = true;
                RaiseCanExecuteChanged();
                await PlacementIdleAsync(machine, feeder);
                machine.SendSafeMoveHeight();
                await PlacementIdleAsync(machine, feeder);
                await machine.MoveToCameraAsync();
                await PlacementIdleAsync(machine, feeder);
                if (machine.CurrentMachineToolHead != null) throw new InvalidOperationException("Camera navigation unavailable.");
                machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_FeederFiducial);
                machine.GotoPoint(result.Result);
                await PlacementIdleAsync(machine, feeder);
                PlacementStatus = "At saved/estimated feeder fiducial. Enable timed camera preview to inspect.";
            }
            catch (Exception ex) { PlacementStatus = ex.Message; machine?.AddStatusMessage(StatusMessageTypes.Warning, ex.Message); }
            finally { _placementRunning = false; RaiseCanExecuteChanged(); }
        }

        private bool ValidCurrent => Current != null && Current.Slot > 0 && Current.Slot < 255;
        public string ConfiguredFeederStatus
        {
            get
            {
                if (!ValidCurrent) return "No valid configured slot.";
                var found = PhotonFeederViewModel.DiscoveredFeeders.FirstOrDefault(f => f.Slot == Current.Slot);
                if (found == null) return PhotonFeederViewModel.GetDiscoveryStatus((byte)Current.Slot);
                if (!string.Equals(found.Address, Current.FeederId, StringComparison.OrdinalIgnoreCase)) return "Mismatched feeder: " + found.Address;
                return PhotonFeederViewModel.IsInitialized((byte)Current.Slot, Current.FeederId)
                    ? "Discovered and initialized" : "Discovered; initialization unknown or tape position uncertain.";
            }
        }
        private bool _configuredTestArmed;
        public bool ConfiguredTestArmed
        {
            get => _configuredTestArmed;
            set { Set(ref _configuredTestArmed, value); RaiseCanExecuteChanged(); }
        }
        private bool CanTestConfigured() => !_placementRunning && !_saving && ValidCurrent && Machine != null && Machine.Connected &&
            Machine.Mode == OperatingMode.Manual && !Machine.IsLocating &&
            !PhotonFeederViewModel.IsBusBusy && ConfiguredTestArmed &&
            PhotonFeederViewModel.DiscoveredFeeders.Any(f => f.Slot == Current.Slot && string.Equals(f.Address, Current.FeederId, StringComparison.OrdinalIgnoreCase));
        private bool CanMoveConfigured() => CanTestConfigured() && PhotonFeederViewModel.IsInitialized((byte)Current.Slot, Current.FeederId) &&
            LagoVista.PickAndPlace.LumenSupport.PhotonProtocolHandler.TryEncodeDistance(PhotonFeederViewModel.ManualDistance, out _);
        private async Task TestConfiguredAsync(bool? forward)
        {
            if (forward.HasValue ? !CanMoveConfigured() : !CanTestConfigured()) return;
            if (Machine.Busy || Machine.ToSendQueueCount != 0 || Machine.UnacknowledgedBytesSent != 0)
            {
                PhotonFeederViewModel.Status = "Machine is busy. No feeder command sent; try again when idle.";
                return;
            }
            var feeder = Current;
            var distance = PhotonFeederViewModel.ManualDistance;
            var result = !forward.HasValue ? await PhotonFeederViewModel.InitializeFeederAsync((byte)feeder.Slot, feeder.FeederId) :
                forward.Value ? await PhotonFeederViewModel.AdvanceFeed((byte)feeder.Slot, distance, feeder.FeederId) :
                await PhotonFeederViewModel.RetractFeed((byte)feeder.Slot, distance, feeder.FeederId);
            if (!result.Successful)
            {
                ConfiguredTestArmed = false;
                PhotonFeederViewModel.Status = result.ErrorMessage;
            }
        }
        public Task<InvokeResult> InitializeFeederAsync() => !ValidCurrent ? Task.FromResult(InvokeResult.FromError("No valid feeder selected.")) :
            PhotonFeederViewModel.InitializeFeederAsync((byte)Current.Slot, Current.FeederId);
        public Task<InvokeResult> AdvanceFeedAsync() => !ValidCurrent || CurrentComponentPackage?.TapePitch == null || CurrentComponent?.Id != Current.Component?.Id
            ? Task.FromResult(InvokeResult.FromError("Valid feeder and assigned component pitch required."))
            : PhotonFeederViewModel.AdvanceFeed((byte)Current.Slot, CurrentComponentPackage.TapePitch.ToDouble(), Current.FeederId);
        public Task<InvokeResult> RetractFeedAsync() => !ValidCurrent || CurrentComponentPackage?.TapePitch == null || CurrentComponent?.Id != Current.Component?.Id
            ? Task.FromResult(InvokeResult.FromError("Valid feeder and assigned component pitch required."))
            : PhotonFeederViewModel.RetractFeed((byte)Current.Slot, CurrentComponentPackage.TapePitch.ToDouble(), Current.FeederId);

        public InvokeResult<Point2D<double>> FindFeederFiducial(string autoFeederId)
        {
            Current = Feeders.SingleOrDefault(fdr => fdr.Id == autoFeederId);
            if(Current == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"Could find find auto feeder with id {autoFeederId}");
            }

            return FindFeederFiducial();
        }

        public InvokeResult<Point2D<double>> FindPickLocation(string autoFeederId)
        {
            Current = Feeders.SingleOrDefault(fdr => fdr.Id == autoFeederId);
            if (Current == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"Could find find auto feeder with id {autoFeederId}");
            }

            return FindPickLocation();
        }


        public InvokeResult<Point2D<double>> FindPickLocation()
        {
            var origin = GetCurrentFeederOrigin();
            if (origin.Successful)
            {
                if (!BoardRigidTransform.Valid(Current.FiducialOffset) || !BoardRigidTransform.Valid(Current.PickOffset))
                    return InvokeResult<Point2D<double>>.FromError("Feeder fiducial or pickup offset is missing.");
                var pickLocation = (origin.Result + Current.FiducialOffset + Current.PickOffset).Round(2);
                return InvokeResult<Point2D<double>>.Create(pickLocation);
            }
            else
                return origin;

        }

        public void GoToPickLocation()
        {
            var result = FindPickLocation();
            if(result.Successful)
            {
                if (CurrentComponentPackage != null)
                {
                    var tapeColor = CurrentComponentPackage.TapeColor.Value;
                    if(!EntityHeader.IsNullOrEmpty(CurrentComponent.TapeColor))
                    {
                        tapeColor = CurrentComponent.TapeColor.Value;
                    }

                    switch (tapeColor)
                    {
                        case TapeColors.Clear:
                            Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInClearTape);
                            break;
                        case TapeColors.Black:
                            Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInBlackTape);
                            break;
                        case TapeColors.White:
                            Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape);
                            break;

                    }
                }
                else
                    Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape);

                Machine.GotoPoint(result.Result);
            //    MachineConfiguration.PositioningCamera.CurrentVisionProfile = MachineConfiguration.PositioningCamera.VisionProfiles.FirstOrDefault(prf => prf.Key == VisionProfile.VisionProfile_SquarePart);
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }


        private bool _placementRunning, _saving;
        private int _placementGeneration;
        private Point2D<double> _placementCandidate, _placementOrigin, _placementPrevious;
        private AutoFeeder _placementFeeder;
        private IMachine _placementMachine;
        private int _placementSlot;
        private string _placementId;
        private string _placementStatus = "Select a saved feeder, confirm clearance, then calibrate its fiducial.";
        public ObservableCollection<string> PlacementMessages { get; } = new ObservableCollection<string>();
        public string PlacementStatus
        {
            get => _placementStatus;
            private set
            {
                Set(ref _placementStatus, value);
                PlacementMessages.Add($"{DateTime.Now:HH:mm:ss}  {value}");
                if (PlacementMessages.Count > 100) PlacementMessages.RemoveAt(0);
            }
        }
        public bool PlacementBusy => _placementRunning || _saving;
        public bool HasPlacementCandidate => _placementCandidate != null;
        public RelayCommand SavePlacementCommand { get; }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(PlacementBusy));
            RaisePropertyChanged(nameof(HasPlacementCandidate));
            RaisePropertyChanged(nameof(GeometryPreflight));
        }

        public void ResetPlacementDialog()
        {
            if (PlacementBusy) return;
            ClearPlacementCandidate();
            ConfiguredTestArmed = false;
            PlacementMessages.Clear();
            PlacementStatus = "Select a saved feeder, unlock settings, confirm clearance and press Run Calibration.";
            RaiseCanExecuteChanged();
        }

        private void ClearPlacementCandidate()
        {
            if (_placementCandidate != null)
                PlacementStatus = "Unsaved placement candidate cleared; calibrate again before saving.";
            Interlocked.Increment(ref _placementGeneration);
            _placementCandidate = null;
            _placementFeeder = null;
            _placementMachine = null;
            RaisePropertyChanged(nameof(HasPlacementCandidate));
        }

        private bool CanCalibratePlacement() => !_placementRunning && !_saving && ValidCurrent && _isEding &&
            ConfiguredTestArmed && Machine != null && Machine.Connected && !Machine.AreSettingsLocked &&
            Machine.Mode == OperatingMode.Manual && Machine.WasMachineHomed && Machine.WasMachinOriginCalibrated &&
            !Machine.IsLocating && !PhotonFeederViewModel.IsBusBusy;

        private void RequirePlacementMachine(IMachine machine, AutoFeeder feeder, bool noLocator = true)
        {
            if (machine == null || !ReferenceEquals(machine, Machine) || !ReferenceEquals(machine, MachineRepo.CurrentMachine) ||
                feeder == null || !ReferenceEquals(feeder, Current) || feeder.Machine?.Id != machine.Settings?.Id ||
                !machine.Connected || !machine.WasMachineHomed || !machine.WasMachinOriginCalibrated ||
                machine.Mode != OperatingMode.Manual || machine.LeftVacuumPump || machine.RightVacuumPump ||
                PhotonFeederViewModel.IsBusBusy || (noLocator && (machine.IsLocating || _locatorViewModel.IsLocating)))
                throw new InvalidOperationException("Requires selected feeder on the connected, homed, origin-calibrated Manual machine, vacuum off and no conflicting operation.");
        }

        private async Task<DateTimeOffset> PlacementIdleAsync(IMachine machine, AutoFeeder feeder)
        {
            RequirePlacementMachine(machine, feeder);
            var idle = await machine.SpinUntilIdleAsync(10000);
            if (!idle.Successful) throw new InvalidOperationException(idle.ErrorMessage);
            int quiet = 0;
            for (int n = 0; n < 50; n++)
            {
                RequirePlacementMachine(machine, feeder);
                quiet = machine.Busy ? 0 : quiet + 1;
                if (quiet >= 11) return DateTimeOffset.UtcNow;
                await Task.Delay(100);
            }
            throw new InvalidOperationException("Machine did not settle.");
        }

        public async Task CalibrateFeederPlacementAsync()
        {
            if (!CanCalibratePlacement()) return;
            var machine = Machine;
            var feeder = Current;
            var slot = feeder.Slot;
            var id = feeder.FeederId;
            _placementRunning = true;
            ClearPlacementCandidate();
            PlacementMessages.Clear();
            PlacementStatus = $"Starting placement calibration for {feeder.Name}, slot {slot}, expected ID {id}.";
            var generation = Volatile.Read(ref _placementGeneration);
            RaiseCanExecuteChanged();
            try
            {
                RequirePlacementMachine(machine, feeder);
                var origin = GetCurrentFeederOrigin();
                var approach = FindFeederFiducial();
                if (!origin.Successful || !approach.Successful ||
                    !BoardRigidTransform.InWorkArea(approach.Result, machine.Settings.WorkAreaSize))
                    throw new InvalidOperationException("Invalid feeder rail or fiducial target.");
                var previous = new Point2D<double>(feeder.FiducialOffset.X, feeder.FiducialOffset.Y);
                await PlacementIdleAsync(machine, feeder);
                if (generation != Volatile.Read(ref _placementGeneration) || !ConfiguredTestArmed)
                    throw new InvalidOperationException("Calibration context changed.");
                PlacementStatus = $"Checking live Photon identity at slot {slot}.";
                var identity = await PhotonFeederViewModel.VerifyFeederIdentityAsync((byte)slot, id);
                if (!identity.Successful) throw new InvalidOperationException(identity.ErrorMessage + " Check the installed feeder and saved slot; no gantry movement was requested.");
                PlacementStatus = "Live feeder identity verified. Preparing position camera.";
                await PlacementIdleAsync(machine, feeder);
                var capture = machine.PositionImageCaptureService;
                if (capture?.Camera?.CameraType?.Value != CameraTypes.Position)
                    throw new InvalidOperationException("Position camera unavailable.");
                machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_FeederFiducial);
                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful) throw new InvalidOperationException(ready.ErrorMessage);
                var profile = capture.Profile;
                var camera = capture.Camera;
                var scale = camera.EffectivePixelsPerMM;
                if (profile?.Key != VisionProfile.VisionProfile_FeederFiducial || !profile.PerformShapeDetection ||
                    !profile.FindCircles || !BoardRigidTransform.Finite(scale) || scale <= 0 ||
                    !BoardRigidTransform.Finite(profile.ErrorToleranceMM) || profile.ErrorToleranceMM <= 0 ||
                    !BoardRigidTransform.Finite(profile.TargetImageRadius) || profile.TargetImageRadius <= 0 ||
                    !BoardRigidTransform.Finite(profile.HoughCirclesMinRadius) || !BoardRigidTransform.Finite(profile.HoughCirclesMaxRadius) ||
                    profile.HoughCirclesMinRadius <= 0 || profile.HoughCirclesMaxRadius <= profile.HoughCirclesMinRadius ||
                    profile.StabilizationCount < 2)
                    throw new InvalidOperationException("Feeder Fiducial profile requires calibrated scale, circle detection, radius range and stabilization.");
                var tolerance = profile.ErrorToleranceMM;
                var radius = profile.TargetImageRadius;
                var min = profile.HoughCirclesMinRadius;
                var max = profile.HoughCirclesMaxRadius;
                var count = profile.StabilizationCount;
                var threshold = profile.PrimaryThreshold;
                var auto = profile.AutoThreshold;
                var invert = profile.Invert;
                var exposure = camera.EffectiveExposure;
                void Check()
                {
                    RequirePlacementMachine(machine, feeder);
                    var actualOrigin = GetCurrentFeederOrigin();
                    if (generation != Volatile.Read(ref _placementGeneration) || !ConfiguredTestArmed ||
                        feeder.Slot != slot || feeder.FeederId != id || machine.AreSettingsLocked ||
                        !actualOrigin.Successful || BoardRigidTransform.Distance(actualOrigin.Result, origin.Result) > 0.000001 ||
                        !BoardRigidTransform.Valid(feeder.FiducialOffset) || BoardRigidTransform.Distance(feeder.FiducialOffset, previous) > 0.000001 ||
                        !ReferenceEquals(machine.PositionImageCaptureService, capture) || !ReferenceEquals(capture.Camera, camera) ||
                        !ReferenceEquals(capture.Profile, profile) || !capture.IsCapturing ||
                        Math.Abs(camera.EffectivePixelsPerMM - scale) > 0.0001 || profile.ErrorToleranceMM != tolerance ||
                        profile.TargetImageRadius != radius || profile.HoughCirclesMinRadius != min || profile.HoughCirclesMaxRadius != max ||
                        profile.StabilizationCount != count || profile.PrimaryThreshold != threshold || profile.AutoThreshold != auto ||
                        profile.Invert != invert || camera.EffectiveExposure != exposure || !profile.FindCircles || !profile.PerformShapeDetection)
                        throw new InvalidOperationException("Feeder, rail or camera context changed; restart calibration.");
                }
                Check();
                machine.SendSafeMoveHeight();
                await PlacementIdleAsync(machine, feeder);
                Check();
                await machine.MoveToCameraAsync();
                await PlacementIdleAsync(machine, feeder);
                Check();
                if (machine.CurrentMachineToolHead != null) throw new InvalidOperationException("Camera navigation unavailable.");
                PlacementStatus = $"Moving to slot {slot} fiducial at {approach.Result}.";
                machine.GotoPoint(approach.Result);
                var settled = await PlacementIdleAsync(machine, feeder);
                Point2D<double> measured = null;
                for (int attempt = 0; attempt <= FeederAutoCornerJogMath.MaxAutomaticJogs; attempt++)
                {
                    Check();
                    if (machine.Busy || machine.CurrentMachineToolHead != null) throw new InvalidOperationException("Machine moved during acquisition.");
                    PlacementStatus = $"Acquiring fresh feeder fiducial (observation {attempt + 1}).";
                    FiducialCircleObservation observation;
                    await using (var light = await machine.BeginTopCameraAcquisitionAsync(11000))
                    {
                        capture.ResetDetections();
                        var handler = new FeederFiducialObservationHandler(capture, profile, scale, settled);
                        _locatorViewModel.RegisterCircleLocatedHandler(handler, 10000);
                        try
                        {
                            var completed = await Task.WhenAny(handler.Completion.Task, Task.Delay(11000));
                            light.ThrowIfInvalid();
                            observation = completed == handler.Completion.Task ? await handler.Completion.Task : null;
                        }
                        finally { _locatorViewModel.UnregisterCircleLocatedHandler(handler); }
                    }
                    Check();
                    if (observation == null) throw new InvalidOperationException("No fresh stable feeder fiducial; check the Feeder Fiducial profile and lighting.");
                    if (machine.Busy || machine.CurrentMachineToolHead != null) throw new InvalidOperationException("Machine moved during observation.");
                    var position = await machine.GetCurrentLocationAsync(5000);
                    Check();
                    if (!position.Successful || machine.Busy || machine.CurrentMachineToolHead != null ||
                        !FiducialCalibrationGate.TryQualify(observation, profile.Key, scale, settled, DateTimeOffset.UtcNow, out _) ||
                        !AutoFeederPlacementMath.TryMeasure(position.Result, approach.Result, observation, scale, radius, min, max,
                            machine.Settings.WorkAreaSize, out var target, out var residual))
                        throw new InvalidOperationException("Fiducial observation is stale or outside safe acquisition geometry.");
                    PlacementStatus = $"Slot {slot}: residual {residual:F3} mm, radius {observation.RadiusMm:F3} mm.";
                    if (residual <= tolerance) { measured = target; break; }
                    if (!FeederAutoCornerJogMath.CanJogAgain(attempt)) throw new InvalidOperationException("Fiducial did not center within five corrections.");
                    Check();
                    machine.GotoPoint(target);
                    settled = await PlacementIdleAsync(machine, feeder);
                }
                if (measured == null) throw new InvalidOperationException("No centered fiducial.");
                await PlacementIdleAsync(machine, feeder);
                Check();
                PlacementStatus = "Rechecking live feeder identity before staging the measured offset.";
                identity = await PhotonFeederViewModel.VerifyFeederIdentityAsync((byte)slot, id);
                if (!identity.Successful) throw new InvalidOperationException(identity.ErrorMessage);
                Check();
                _placementCandidate = measured - origin.Result;
                _placementOrigin = origin.Result;
                _placementPrevious = previous;
                _placementFeeder = feeder;
                _placementMachine = machine;
                _placementSlot = slot;
                _placementId = id;
                PlacementStatus = $"Verified slot {slot}, measured {measured}; candidate offset {_placementCandidate}. Click Save to persist. Pick offset unchanged.";
            }
            catch (Exception ex)
            {
                ClearPlacementCandidate();
                PlacementStatus = "Calibration stopped: " + ex.Message + " Previous offset retained.";
                machine?.AddStatusMessage(StatusMessageTypes.Warning, PlacementStatus);
            }
            finally
            {
                _placementRunning = false;
                RaiseCanExecuteChanged();
            }
        }

        private sealed class FeederFiducialObservationHandler : ICircleLocatedHandler
        {
            private readonly IImageCaptureService _capture;
            private readonly VisionProfile _profile;
            private readonly double _scale;
            private readonly DateTimeOffset _settled;
            internal readonly TaskCompletionSource<FiducialCircleObservation> Completion =
                new TaskCompletionSource<FiducialCircleObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal FeederFiducialObservationHandler(IImageCaptureService capture, VisionProfile profile, double scale, DateTimeOffset settled)
            { _capture = capture; _profile = profile; _scale = scale; _settled = settled; }

            public void CircleLocated(MVLocatedCircle circle)
            {
                if (Completion.Task.IsCompleted || circle == null || !circle.Stabilized || !circle.PositionStable ||
                    !ReferenceEquals(_capture.Profile, _profile) || circle.CenterPixels == null || circle.OffsetPixels == null) return;
                var snapshot = new FiducialCircleObservation
                {
                    TrackId = circle.Id, CameraType = circle.CameraType, ProfileKey = _profile.Key,
                    ObservedAtUtc = DateTimeOffset.UtcNow, FrameAtUtc = _capture.LastProcessedFrameUtc,
                    PixelsPerMillimeter = circle.PixelsPerMM, FrameWidth = _capture.CaptureFrameSize.Width,
                    FrameHeight = _capture.CaptureFrameSize.Height, Stabilized = circle.Stabilized,
                    PositionStable = circle.PositionStable, Centered = circle.Centered, FoundCount = circle.FoundCount,
                    CenterXpx = circle.CenterPixels.X, CenterYpx = circle.CenterPixels.Y,
                    OffsetXpx = circle.OffsetPixels.X, OffsetYpx = circle.OffsetPixels.Y,
                    RadiusPx = circle.RadiusPixels, RadiusMm = circle.RadiusPixels / _scale,
                    ErrorToleranceMm = _profile.ErrorToleranceMM
                };
                if (FiducialCalibrationGate.TryQualify(snapshot, VisionProfile.VisionProfile_FeederFiducial,
                    _scale, _settled, DateTimeOffset.UtcNow, out _)) Completion.TrySetResult(snapshot);
            }
            public void CircleLocatorTimeout() => Completion.TrySetResult(null);
            public void CircleLocatorAborted() => Completion.TrySetResult(null);
        }

        public void SetPickLocationCommand()
        {
            if (_placementRunning || _saving || _placementCandidate != null)
            {
                PlacementStatus = "Finish or discard the placement calibration before changing pickup offset.";
                return;
            }
            var origin = GetCurrentFeederOrigin();
            if(origin.Successful)
            {
                Current.PickOffset = (Machine.MachinePosition.ToPoint2D() - (Current.FiducialOffset + origin.Result)).Round(2);
                NotifyGeometryChanged();
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, origin.ErrorMessage);
            }
            
        }

        public async void CreateAutoFeederFromTemplateAsync()
        {
            var result = await _restClient.GetAsync<DetailResponse<AutoFeeder>>($"/api/mfg/autofeeder/template/{SelectedTemplateId}/factory");
            if (result.Successful)
            {
                Current = result.Result.Model;
            }
        }

        public async void Save() => await SaveCoreAsync(false);

        private async Task SaveCoreAsync(bool placementOnly)
        {
            if (Current == null || _saving || _placementRunning || (placementOnly && (!HasPlacementCandidate || !_isEding))) return;
            if (String.IsNullOrEmpty(Current.Name))
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Please ensure you have a name for your auto feeder.");
                return;
            }

            if (!placementOnly)
            {
                if(CurrentComponent != null)
                    Current.Component = EntityHeader<Component>.Create(CurrentComponent.Id, CurrentComponent.Key, CurrentComponent.Name);
                else
                    Current.Component = null;
            }

            var feeder = Current;
            var editing = _isEding;
            var oldOffset = feeder.FiducialOffset;
            var candidate = _placementCandidate;
            var saved = false;
            _saving = true;
            RaiseCanExecuteChanged();
            try
            {
                if (candidate != null)
                {
                    PlacementStatus = "Verifying feeder identity and calibration context before save.";
                    var machine = Machine;
                    RequirePlacementMachine(machine, feeder);
                    var origin = GetCurrentFeederOrigin();
                    if (!ReferenceEquals(_placementFeeder, feeder) || !ReferenceEquals(_placementMachine, machine) ||
                        feeder.Slot != _placementSlot || feeder.FeederId != _placementId || machine.AreSettingsLocked ||
                        !origin.Successful || BoardRigidTransform.Distance(origin.Result, _placementOrigin) > 0.000001 ||
                        !BoardRigidTransform.Valid(oldOffset) || BoardRigidTransform.Distance(oldOffset, _placementPrevious) > 0.000001)
                        throw new InvalidOperationException("Calibration context changed; recalibrate before saving.");
                    await PlacementIdleAsync(machine, feeder);
                    var identity = await PhotonFeederViewModel.VerifyFeederIdentityAsync((byte)feeder.Slot, feeder.FeederId);
                    if (!identity.Successful) throw new InvalidOperationException(identity.ErrorMessage);
                    RequirePlacementMachine(machine, feeder);
                    origin = GetCurrentFeederOrigin();
                    if (!ReferenceEquals(_placementCandidate, candidate) || feeder.Slot != _placementSlot || feeder.FeederId != _placementId ||
                        machine.AreSettingsLocked || !origin.Successful ||
                        BoardRigidTransform.Distance(origin.Result, _placementOrigin) > 0.000001 ||
                        !BoardRigidTransform.Valid(feeder.FiducialOffset) ||
                        BoardRigidTransform.Distance(feeder.FiducialOffset, _placementPrevious) > 0.000001)
                        throw new InvalidOperationException("Calibration context changed.");
                    feeder.FiducialOffset = candidate;
                }
                var result = editing ? await _restClient.PutAsync("/api/mfg/autofeeder", feeder) : await _restClient.PostAsync("/api/mfg/autofeeder", feeder);
                if (!result.Successful) throw new InvalidOperationException(result.ErrorMessage);
                saved = true;
                if (!editing) _autoFeeders.Add(feeder);
                if (candidate != null)
                {
                    ClearPlacementCandidate();
                    PlacementStatus = "Feeder fiducial saved. Reload/reselect and verify Go To Fiducial.";
                }
                if (!placementOnly && ReferenceEquals(Current, feeder)) Current = null;
            }
            catch (Exception ex)
            {
                if (candidate != null && !saved) feeder.FiducialOffset = oldOffset;
                PlacementStatus = "Save failed: " + ex.Message + (candidate == null ? "" : " Previous offset retained locally; reload if server outcome is uncertain.");
                Machine?.AddStatusMessage(StatusMessageTypes.FatalError, PlacementStatus);
            }
            finally { _saving = false; NotifyGeometryChanged(); RaiseCanExecuteChanged(); }
        }

        public void Cancel()
        {
            Current = null;
        }

        private async Task LoadFeeders()
        {
            var autoFeeders = await _restClient.GetListResponseAsync<AutoFeeder>($"/api/mfg/machine/{MachineRepo.CurrentMachine.Settings.Id}/autofeeders?loadcomponents=true");
            _autoFeeders = new ObservableCollection<AutoFeeder>(autoFeeders.Model);

            RaisePropertyChanged(nameof(Feeders));
        }

        public async override Task<InvokeResult> NextPartAsync()
        {
            if (Current == null)
            {
                return InvokeResult.FromError("No current feeder.");
            }
            
            var feeder = Current;
            if (feeder.PartCount <= 0) return InvokeResult.FromError("No parts remaining.");
            var result = await AdvanceFeedAsync();
            if (!result.Successful) return result;
            feeder.PartCount--;
            await _restClient.PutAsync($"/api/mfg/autofeeder/{feeder.Id}/partcount/{feeder.PartCount}", new { }, waitCursor: false);

            return result;
        
        }

        AutoFeeder _current;
        public AutoFeeder Current
        {
            get => _current;
            set
            {
                if (ReferenceEquals(_current, value))
                    return;

                ClearPlacementCandidate();
                Set(ref _current, value);
                ConfiguredTestArmed = false;
                NotifyGeometryChanged();
                RaisePropertyChanged(nameof(ConfiguredFeederStatus));
                _isEding = true;
                RaiseCanExecuteChanged();
                if (value != null)
                {
                    _tapeSize = value.TapeSize;
                }
                else
                {
                    _tapeSize = null;
                }

                RaisePropertyChanged(nameof(TapeSize));

                if(value?.Component != null)
                {
                    LoadComponent(value.Component.Id);
                }
                else
                {
                    LoadComponent(null);
                }
            }
        }

        PhotonFeeder _currentPhotonFeeder;
        public PhotonFeeder CurrentPhotonFeeder
        {
            get => _currentPhotonFeeder;
            set => Set(ref _currentPhotonFeeder, value);
        }

        private string _selectedTemplateId = StringExtensions.NotSelectedId;
        public string SelectedTemplateId
        {
            get => _selectedTemplateId;
            set
            {
                Set(ref _selectedTemplateId, value);
                RaiseCanExecuteChanged();
            }
        }

        public override int AvailableParts
        {
            get
            {
                if(Current == null)
                {
                    return 0;
                }
                else
                {
                    return Current.PartCount;
                }
            }
        }

        public override Point2D<double> CurrentPartLocation 
        { 
            get
            {
                var result = FindPickLocation();
                if (!result.Successful)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Could not find current feeder location {result.ErrorMessage}, moving to origin.");

                    return new Point2D<double>(0, 0);
                }

                return result.Result;
            }
            
        }

        public override double? PickHeight => _current?.PickHeight;

        ObservableCollection<AutoFeederTemplateSummary> _templates;
        public ObservableCollection<AutoFeederTemplateSummary> Templates 
        {
            get => _templates;
            private set => Set(ref _templates, value);
        } 
        
        public ObservableCollection<MachineFeederRail> FeederRails { get => MachineRepo.CurrentMachine.Settings.FeederRails; }
        public ObservableCollection<AutoFeeder> Feeders { get => _autoFeeders; }


        public IPhotonFeederViewModel PhotonFeederViewModel { get; }

        public RelayCommand CreateAutoFeederFromTemplateCommand { get; }

        public RelayCommand SetPartPickLocationCommand { get; }
        public RelayCommand SetFeederFiducialLocationCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand AdvanceFeedCommand { get; }
        public RelayCommand RetractFeedCommand { get; }

        public RelayCommand GoToPickLocationCommand { get; }
        public RelayCommand GoToFiducialCommand { get; }

        public RelayCommand AddCommand { get; }

        public RelayCommand CancelCommand { get; }

        public RelayCommand RefreshTemplatesCommand { get; }

        public RelayCommand ReloadFeederCommand { get; }
        public RelayCommand InitializeFeederCommand { get; }
    }
}
