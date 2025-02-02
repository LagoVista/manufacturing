using Emgu.CV.DepthAI;
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

            CreateAutoFeederFromTemplateCommand = CreatedCommand(CreateAutoFeederFromTemplateAsync, () => !string.IsNullOrEmpty(SelectedTemplateId));
            SetFeederFiducialLocationCommand = CreatedMachineConnectedSettingsCommand(SetFeederFiducialLocationAsync, () => Current != null);
            SetPartPickLocationCommand = CreatedMachineConnectedSettingsCommand(SetPickLocationCommand, () => Current != null);

            GoToFiducialCommand = CreatedMachineConnectedCommand(GoToFeederFiducial, () => Current != null);
            GoToPickLocationCommand = CreatedMachineConnectedCommand(GoToPickLocation, () => Current != null);

            InitializeFeederCommand = CreatedMachineConnectedCommand(async () => await InitializeFeederAsync(), () => Current != null);
            AdvanceFeedCommand = CreatedMachineConnectedCommand(async () => await AdvanceFeedAsync(), () => Current != null);
            RetractFeedCommand = CreatedMachineConnectedCommand(async () => await RetractFeedAsync(), () => Current != null);

            AddCommand = CreatedCommand(Add, () => MachineRepo.HasValidMachine && SelectedTemplateId.HasValidId() && CurrentPhotonFeeder != null);
            SaveCommand = CreatedCommand(Save, () => Current != null);
            CancelCommand = CreatedCommand(Cancel, () => Current != null);
            ReloadFeederCommand = CreatedCommand(ReloadFeeder, () => Current != null);
        }

        protected async override void OnMachineConnected()
        {
            base.OnMachineConnected();

            foreach(var feeder in _autoFeeders)
            {
                var result = await PhotonFeederViewModel.InitializeFeederAsync((byte)feeder.Slot, feeder.FeederId);
                if (result.Successful)
                    Machine.AddStatusMessage(StatusMessageTypes.Info, $"Initialized feeder {feeder.Name}");
            }
        }

        private void PhotonFeederViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
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

            var rail = MachineConfiguration.FeederRails.FirstOrDefault(fdr => slot >= fdr.SlotStartIndex && slot <= (fdr.SlotStartIndex + fdr.NumberSlots));

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

            _feederOffset = new Point2D<double>();

            _locatorViewModel.RegisterRectangleLocatedHandler(this);

            var toolHead = Machine.CurrentMachineToolHead;
            await Machine.MoveToCameraAsync();

            if (CurrentComponent.ComponentPackage.Value.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.ComponentPackage, CurrentComponent.ComponentPackage.Id,
                    CurrentComponent.ComponentPackage.Value.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInClearTape);

            _waitForCenter = new ManualResetEventSlim(false);
            _locatorViewModel.RegisterRectangleLocatedHandler(this);

            await Task.Run(() =>
            {
                var attemptCount = 0;
                while (!_waitForCenter.IsSet && ++attemptCount < 200)
                    _waitForCenter.Wait(25);
            });

            _locatorViewModel.UnregisterRectangleLocatedHandler(this);

            var success = _waitForCenter.IsSet;
            _waitForCenter.Dispose();
            _waitForCenter = null;

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
                var result = await _restClient.GetAsync<DetailResponse<AutoFeeder>>($"/api/mfg/autofeeder/{Current.Id}");
                if (result.Successful)
                {
                    Current = result.Result.Model;
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
            var result = await _restClient.GetListResponseAsync<AutoFeederTemplate>("/api/mfg/autofeeder/templates");
            if (result.Successful)
            {
                var feeders = result.Model.ToList();
                feeders.Insert(0, new AutoFeederTemplate() { Id = StringExtensions.NotSelectedId, Name = "-select feeder template-" });
                Templates = new ObservableCollection<AutoFeederTemplate>(feeders);
            }

            SelectedTemplateId = StringExtensions.NotSelectedId;
        }

        protected async override void MachineChanged(IMachine machine)
        {
            if (MachineRepo.HasValidMachine)
            {
                await LoadFeeders();
            }
            base.MachineChanged(machine);
        }


        private MachineFeederRail GetRail()
        {
            return MachineConfiguration.FeederRails.FirstOrDefault(rl => Current.Slot >= rl.SlotStartIndex && Current.Slot <= (rl.SlotStartIndex + rl.NumberSlots));
        }

        public InvokeResult<Point2D<double>> GetCurrentFeederOrigin()
        {
            if (Current == null) return InvokeResult<Point2D<double>>.FromError("No current auto feeder");

            var rail = GetRail();
            if (rail == null) return InvokeResult<Point2D<double>>.FromError($"Could not find feeder rail for slot ${Current.Slot}.");

            var slotOnRail = Current.Slot - rail.SlotStartIndex;
            var deltaX = slotOnRail * Current.Size.Height;

            var origin = new Point2D<double>( rail.Rotation.Value == FeederRotations.OneEighty ? rail.FirstFeederOrigin.X - deltaX : rail.FirstFeederOrigin.X + deltaX, rail.FirstFeederOrigin.Y);
            return InvokeResult<Point2D<double>>.Create(origin);
        }

        public InvokeResult<Point2D<double>> FindFeederFiducial()
        {
            var origin = GetCurrentFeederOrigin();
            if (origin.Successful)
            {
                var fiducialLocation = origin.Result + Current.FiducialOffset;
                return InvokeResult<Point2D<double>>.Create(fiducialLocation);
            }
            else
                return origin;
        }

        public void GoToFeederFiducial()
        {
            var result = FindFeederFiducial();
            if (result.Successful)
            {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_FeederFiducial);
                Machine.GotoPoint(result.Result);
                //MachineConfiguration.PositioningCamera.CurrentVisionProfile = MachineConfiguration.PositioningCamera.VisionProfiles.FirstOrDefault(prf => prf.Key == VisionProfile.VisionProfile_BoardFiducial);
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }

        public async Task<InvokeResult> InitializeFeederAsync()
        {
            return await PhotonFeederViewModel.InitializeFeederAsync((byte)Current.Slot, Current.FeederId);
        }

        public async Task<InvokeResult> AdvanceFeedAsync()
        {
            return await PhotonFeederViewModel.AdvanceFeed((byte)Current.Slot, 4);
        }

        public async Task<InvokeResult> RetractFeedAsync()
        {
            return await PhotonFeederViewModel.RetractFeed((byte)Current.Slot, 4);
        }

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


        public void SetFeederFiducialLocationAsync()
        {
            var origin = GetCurrentFeederOrigin();
            if(origin.Successful)
            {
                Current.FiducialOffset = (Machine.MachinePosition.ToPoint2D() - origin.Result).Round(2);
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, origin.ErrorMessage);
            }
            
        }

        public void SetPickLocationCommand()
        {
            var origin = GetCurrentFeederOrigin();
            if(origin.Successful)
            {
                Current.PickOffset = (Machine.MachinePosition.ToPoint2D() - (Current.FiducialOffset + origin.Result)).Round(2);
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

        public async void Save()
        {
            if (String.IsNullOrEmpty(Current.Name))
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Please ensure you have a name for your auto feeder.");
                return;
            }

            if(CurrentComponent != null)
            {
                Current.Component = EntityHeader<Component>.Create(CurrentComponent.Id, CurrentComponent.Key, CurrentComponent.Name);
            }
            else
            {
                Current.Component = null;
            }

            var result = this._isEding ? await _restClient.PutAsync("/api/mfg/autofeeder", Current) : await _restClient.PostAsync("/api/mfg/autofeeder", Current);
            if(result.Successful)
            {
                if(!_isEding)
                    _autoFeeders.Add(Current);
               
                Current = null;
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
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
            if (Current != null)
            {
                Current.PartCount--;
                return await AdvanceFeedAsync();
            }

            return InvokeResult.FromError("No current feeder.");
            
        }

        AutoFeeder _current;
        public AutoFeeder Current
        {
            get => _current;
            set
            {
                Set(ref _current, value);
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

        private Point2D<double> _currentPartLocation;
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

        ObservableCollection<AutoFeederTemplate> _templates;
        public ObservableCollection<AutoFeederTemplate> Templates 
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
