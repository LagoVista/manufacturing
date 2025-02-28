using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels.Machine;
using LagoVista.PickAndPlace.ViewModels.Vision;
using RingCentral;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public abstract class FeederViewModel : MachineViewModelBase, IFeederViewModel, IRectangleLocatedHandler
    {

        private enum PickStates
        {
            Idle,
            Picked,
            Inspecting,
            TrialPlaced,
        }

        protected Point2D<double> _feederOffset;

        private PickStates _pickStates = PickStates.Idle;
        protected ManualResetEventSlim _waitForCenter;
        protected readonly ILocatorViewModel _locatorViewModel;
        private readonly IRestClient _restClient;
        private readonly IMachineUtilitiesViewModel _machineUtilitiesViewModel;

        protected FeederViewModel(IMachineRepo machineRepo, IRestClient resteClient, ILocatorViewModel locatorViewModel, IMachineUtilitiesViewModel machineUtilitiesViewModel) : base(machineRepo)
        {
            _restClient = resteClient ?? throw new ArgumentNullException(nameof(resteClient));
            _machineUtilitiesViewModel = machineUtilitiesViewModel ?? throw new ArgumentNullException(nameof(machineUtilitiesViewModel));
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            PickCurrentPartCommand = CreatedMachineConnectedCommand(async () => await PickCurrentPartAsync(), () => CurrentComponent != null);
            RecycleCurrentPartCommand = CreatedMachineConnectedCommand(async () => await RecycleCurrentPartAsync(), () => CurrentComponent != null);
            InspectCurrentPartCommand = CreatedMachineConnectedCommand(async () => await InspectCurrentPartAsync(), () => CurrentComponent != null);
            SaveComponentPackageCommand = CreateCommand(SaveComponentPackage, () => CurrentComponentPackage != null);
            ShowComponentDetailCommand = CreateCommand(ShowComponentDetail, () => CurrentComponent != null);
            ShowComponentPackageDetailCommand = CreateCommand(ShowComponentPackageDetail, () => CurrentComponent != null && CurrentComponent?.ComponentPackage != null);
            ShowDataSheetCommand = CreateCommand(ShowDataSheeet, () => CurrentComponent != null && !String.IsNullOrEmpty(CurrentComponent.DataSheet));
            NextPartCommand = CreatedMachineConnectedCommand(async () => await NextPartAsync());
            CenterOnPartCommand = CreatedMachineConnectedCommand(async () => await CenterOnPartAsync(CurrentComponent), () => CurrentComponent != null);
        }

        public override async Task InitAsync()
        {
            var componentCategories = await _restClient.GetListResponseAsync<EntityBase>("/api/categories/component");
            if (componentCategories.Successful)
            {
                ComponentCategories = new ObservableCollection<EntityHeader>(componentCategories.Model.Select(c => EntityHeader.Create(c.Id, c.Key, c.Name)));
                ComponentCategories.Insert(0, new EntityHeader() { Id = StringExtensions.NotSelectedId, Key = StringExtensions.NotSelectedId, Text = "-select component category-" });
                Components = new ObservableCollection<ComponentSummary>();
                Components.Insert(0, new ComponentSummary() { Id = StringExtensions.NotSelectedId, Key = StringExtensions.NotSelectedId, Name = "-select component category first-" });
                SelectedCategoryKey = StringExtensions.NotSelectedId;
                SelectedComponentSummaryId = StringExtensions.NotSelectedId;
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, componentCategories.ErrorMessage);
            }

            await base.InitAsync();
        }

        public void ShowComponentPackageDetail()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current component.");
                return;
            }

            if (CurrentComponent.ComponentPackage == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No component does not have package assigned.");
                return;
            }

            var url = $"https://www.nuviot.com/mfg/package/{CurrentComponent.ComponentPackage.Id}";
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url as string,
                UseShellExecute = true
            });
        }

        public void ShowDataSheeet()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current component.");
                return;
            }

            if (String.IsNullOrEmpty(CurrentComponent.DataSheet))
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Data sheet not available");
            }

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = CurrentComponent.DataSheet,
                UseShellExecute = true
            });
        }

        public void ShowComponentDetail()
        {
            if (CurrentComponent != null)
            {
                var url = $"https://www.nuviot.com/mfg/component/{CurrentComponent.Id}";
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url as string,
                    UseShellExecute = true
                });
            }
        }

        async void SaveComponentPackage()
        {
            var result = await _restClient.PutAsync("/api/mfg/component/package", CurrentComponentPackage);
            if (!result.Successful)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Could not save component package: {result.ErrorMessage}");
            }
        }

        public async Task<InvokeResult> PickCurrentPartAsync()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current part selected, can not pick.");
                return InvokeResult.FromError("No current part selected, can not pick.");
            }

            if (CurrentComponent.ComponentPackage == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Current part does not have a package, can not place..");
                return InvokeResult.FromError("Current part does not have a package, can not place.");
            }

            var currentLocation = CurrentPartLocation;
            if (currentLocation == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not identify location of current part.");
                _pickStates = PickStates.Idle;
                return InvokeResult.FromError("Could not identify location of current part.");
            }

            Machine.SendSafeMoveHeight();
            if (CurrentComponent.ComponentPackage.HasValue)
            {
                if (!EntityHeader.IsNullOrEmpty(CurrentComponent.ComponentPackage.Value.NozzleTip))
                {
                    var toolHead = MachineConfiguration.ToolHeads.FirstOrDefault(th => th.CurrentNozzle?.Id == CurrentComponent.ComponentPackage.Value.NozzleTip.Id);
                    if (toolHead != null)
                        await Machine.MoveToToolHeadAsync(toolHead);
                    else
                        await Machine.MoveToToolHeadAsync(MachineConfiguration.ToolHeads.First());
                }
                else
                    await Machine.MoveToToolHeadAsync(MachineConfiguration.ToolHeads.First());
            }
            else
                await Machine.MoveToToolHeadAsync(MachineConfiguration.ToolHeads.First());


            Machine.SendCommand(CurrentPartLocation.ToGCode());
            Machine.SetRelativeMode();
            Machine.SendCommand(_feederOffset.ToGCode());
            Machine.SetAbsoluteMode();

            if (PickHeight.HasValue)
                Machine.SetToolHeadHeight(PickHeight.Value);

            Machine.VacuumPump = true;

            Machine.Dwell(100);

            var beforePick = await _machineUtilitiesViewModel.ReadVacuumAsync();
            Machine.SendSafeMoveHeight();
            Machine.Dwell(100);
            var afterPick = await _machineUtilitiesViewModel.ReadVacuumAsync();
            StatusMessage = $"Part Picked - Vacuum {beforePick} - {afterPick}";

            _pickStates = PickStates.Picked;

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> InspectCurrentPartAsync()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current part selected, can not inspect.");
                return InvokeResult.FromError("No current part selected, can not inspect.");
            }

            if (_pickStates == PickStates.Idle)
                await PickCurrentPartAsync();

            if (_pickStates == PickStates.Picked)
            {
                Machine.SendSafeMoveHeight();
                await Machine.GoToPartInspectionCameraAsync();
                _pickStates = PickStates.Inspecting;
            }

            if (CurrentComponentPackage!.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.ComponentPackage, CurrentComponentPackage.Id, CurrentComponentPackage!.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> InspectPartAsync(Component component)
        {
            CurrentComponent = component;
            return await InspectCurrentPartAsync();
        }

        public async Task<InvokeResult> PickPartAsync(Component component)
        {
            CurrentComponent = component;
            return await PickCurrentPartAsync();
        }

        public async Task<InvokeResult> RecyclePartAsync(Component component)
        {
            CurrentComponent = component;
            return await RecycleCurrentPartAsync();
        }

        public async Task<InvokeResult> RecycleCurrentPartAsync()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current part selected, can not recycle");
                return InvokeResult.FromError("No current part selected, can not recycle");
            }

            var beforePick = await _machineUtilitiesViewModel.ReadVacuumAsync();
            Machine.SendSafeMoveHeight();
            Machine.SendCommand(CurrentPartLocation.ToGCode());
            Machine.SetToolHeadHeight(PickHeight.Value);
            Machine.VacuumPump = false;
            Machine.Dwell(100);
            Machine.SendSafeMoveHeight();
            var afterPick = await _machineUtilitiesViewModel.ReadVacuumAsync();

            _pickStates = PickStates.Idle;

            StatusMessage = $"Part Picked - Vacuum {beforePick} - {afterPick}";

            await Machine.MoveToCameraAsync();

            return InvokeResult.Success;
        }

        public async void LoadComponent(string componentId)
        {
            if (!componentId.HasValidId())
            {
                CurrentComponent = null;
                SelectedCategoryKey = StringExtensions.NotSelectedId;
            }
            else
            {

                var result = await _restClient.GetAsync<DetailResponse<Manufacturing.Models.Component>>($"/api/mfg/component/{componentId}?loadcomponent=true");
                if (result.Successful)
                {
                    if (result.Result.Successful)
                    {
                        CurrentComponent = result.Result.Model;

                        if (CurrentComponent.ComponentType.Key != _selectedCategoryKey)
                        {
                            _selectedCategoryKey = CurrentComponent.ComponentType.Key;
                            await LoadComponentsByCategory(_selectedCategoryKey);
                            RaisePropertyChanged(nameof(SelectedCategoryKey));
                        }

                        _selectedComponentSummaryId = componentId;
                        RaisePropertyChanged(SelectedComponentSummaryId);
                    }
                }
                else
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }

        public async Task LoadComponentsByCategory(string categoryKey)
        {
            var components = await _restClient.GetListResponseAsync<ComponentSummary>($"/api/mfg/components?componentType={categoryKey}");
            Components = new ObservableCollection<ComponentSummary>(components.Model);
            Components.Insert(0, new ComponentSummary() { Id = StringExtensions.NotSelectedId, Name = "-select component-" });
            SelectedComponentSummaryId = StringExtensions.NotSelectedId;
        }

        public Task<InvokeResult> MoveToPartInFeederAsync()
        {
            _feederOffset = new Point2D<double>();

            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current part selected, can not inspect.");
                return Task.FromResult(InvokeResult.FromError("No current part selected, can not inspect."));
            }

            Machine.SendSafeMoveHeight();
            Machine.SendSafeMoveHeight();
            Machine.SendCommand(CurrentPartLocation.ToGCode());

            if (CurrentComponentPackage == null && CurrentComponent.ComponentPackage != null)
            {
                CurrentComponentPackage = CurrentComponent.ComponentPackage.Value;
            }

            if(CurrentComponent.PartInTapeVisionProfile != null)
            {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfileSource.Component, CurrentComponent.Id, CurrentComponent.PartInTapeVisionProfile);
            }
            else if(CurrentComponentPackage?.PartInTapeVisionProfile != null)
            {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfileSource.ComponentPackage, CurrentComponentPackage.Id, CurrentComponentPackage.PartInTapeVisionProfile);
            }
            else if (!EntityHeader.IsNullOrEmpty(CurrentComponent.TapeColor))
            {
                switch (CurrentComponent.TapeColor.Value)
                {
                    case TapeColors.Clear: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInClearTape); break;
                    case TapeColors.White: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape); break;
                    case TapeColors.Black: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInBlackTape); break;

                }
            }
            else if (CurrentComponentPackage != null)
            {
                switch (CurrentComponentPackage.TapeColor.Value)
                {
                    case TapeColors.Clear: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInClearTape); break;
                    case TapeColors.White: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape); break;
                    case TapeColors.Black: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInBlackTape); break;

                }
            }
            else
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape);

            return Task.FromResult(InvokeResult.Success);
        }

        public Task<InvokeResult> MoveToPartInFeederAsync(Component component)
        {
            if (CurrentComponent != component)
                CurrentComponent = component;

            return MoveToPartInFeederAsync();
        }
        
        public virtual Task<InvokeResult> CenterOnPartAsync(Component component)
        {
            return Task.FromResult(InvokeResult.Success);
        }

        public virtual Task<InvokeResult> CenterOnPartAsync()
        {
            return Task.FromResult(InvokeResult.Success);
        }


        ObservableCollection<EntityHeader> _componentCategories;
        public ObservableCollection<EntityHeader> ComponentCategories
        {
            get => _componentCategories;
            set => Set(ref _componentCategories, value);
        }

        string _selectedCategoryKey = StringExtensions.NotSelectedId;
        public string SelectedCategoryKey
        {
            get => _selectedCategoryKey;
            set
            {
                if (_selectedCategoryKey != value)
                {
                    _selectedCategoryKey = value;
                    if (_selectedCategoryKey.HasValidId())
                    {
                        LoadComponentsByCategory(value);
                    }
                    else
                    {
                        value = StringExtensions.NotSelectedId;
                        Components = new ObservableCollection<ComponentSummary>();
                        Components.Insert(0, new ComponentSummary() { Id = StringExtensions.NotSelectedId, Name = "-select component category first-" });
                        SelectedComponentSummaryId = StringExtensions.NotSelectedId;
                    }
                }
            }
        }

        string _selectedComponentSummaryId = StringExtensions.NotSelectedId;
        public string SelectedComponentSummaryId
        {
            get => _selectedComponentSummaryId;
            set
            {
                if (value != _selectedComponentSummaryId)
                {
                    Set(ref _selectedComponentSummaryId, value);
                    if (value.HasValidId())
                        LoadComponent(_selectedComponentSummaryId);
                }
            }
        }

        ObservableCollection<ComponentSummary> _components;
        public ObservableCollection<ComponentSummary> Components
        {
            get => _components;
            set => Set(ref _components, value);
        }


        Component _currentComponent;
        public Component CurrentComponent
        {
            get => _currentComponent;
            set
            {
                if (_currentComponent?.Id != value?.Id)
                {
                    _currentComponent = value;
                    if(value != null)
                    {
                        _selectedComponentSummaryId = value.Id;
                        RaisePropertyChanged(SelectedComponentSummaryId);
                        CurrentComponentPackage = CurrentComponent.ComponentPackage?.Value;
                        _currentComponent = value;
                    }

                    RaiseCanExecuteChanged();
                }

            }
        }

        ComponentPackage _currentComponentPackage;
        public ComponentPackage CurrentComponentPackage
        {
            get => _currentComponentPackage;
            set => Set(ref _currentComponentPackage, value);
        }


        protected EntityHeader<TapeSizes> _tapeSize;
        public EntityHeader<TapeSizes> TapeSize
        {
            get => (CurrentComponentPackage != null) ? CurrentComponentPackage.TapeSize : EntityHeader<TapeSizes>.Create(TapeSizes.EightMM);
        }

        protected EntityHeader<TapePitches> _tapePitch;
        public EntityHeader<TapePitches> TapePitch
        {
            get
            {
                return (CurrentComponentPackage != null) ? CurrentComponentPackage.TapePitch : EntityHeader<TapePitches>.Create(TapePitches.EightMM);
            }
        }


        private bool _useCalculted;
        public bool UseCalculated
        {
            get => _useCalculted;
            set => Set(ref _useCalculted, value);
        }

        public abstract int AvailableParts { get; }

        public abstract Task<InvokeResult> NextPartAsync();

        public void RectangleLocated(MVLocatedRectangle rectangleLocated)
        {
            if (rectangleLocated.Centered)
            {
                if (rectangleLocated.Stabilized)
                {
                    _locatorViewModel.UnregisterRectangleLocatedHandler(this);
                    _waitForCenter.Set();
                }
            }
            else
            {
                if (rectangleLocated.Stabilized)
                {
                    Machine.SetRelativeMode();
                    var offset = new Point2D<double>(rectangleLocated.OffsetMM.X * 0.8, rectangleLocated.OffsetMM.Y * 0.75);
                    _feederOffset -= offset;
                    Machine.SendCommand($"G0  X{-offset.X},Y{-offset.Y}");
                    Machine.SetAbsoluteMode();
                    rectangleLocated.Reset();
                }
            }
        }

        protected void SetTapeHoleProfile()
        {
            if (CurrentComponent == null)
            {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape);
            }
            else if (!EntityHeader.IsNullOrEmpty(CurrentComponent.TapeColor))
            {
                switch (CurrentComponent.TapeColor.Value)
                {
                    case TapeColors.Clear: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_TapeHoleClearTape); break;
                    case TapeColors.White: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_TapeHoleWhiteTape); break;
                    case TapeColors.Black: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_TapeHoleBlackTape); break;

                }
            }
            else if (CurrentComponentPackage != null)
            {
                switch (CurrentComponentPackage.TapeColor.Value)
                {
                    case TapeColors.Clear: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_TapeHoleClearTape); break;
                    case TapeColors.White: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_TapeHoleWhiteTape); break;
                    case TapeColors.Black: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_TapeHoleBlackTape); break;

                }
            }
            else
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape);
        }

        protected void SetPartInTapeHoleProfile()
        {

            if (CurrentComponent == null)
            {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape);
            }
            else if(CurrentComponent.PartInTapeVisionProfile != null)
            {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfileSource.Component, CurrentComponent.Id, CurrentComponent.PartInTapeVisionProfile);
            }
            else if (CurrentComponentPackage?.PartInTapeVisionProfile != null) {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfileSource.Component, CurrentComponentPackage.Id, CurrentComponentPackage.PartInTapeVisionProfile);
            }
            else if (!EntityHeader.IsNullOrEmpty(CurrentComponent.TapeColor))
            {
                switch (CurrentComponent.TapeColor.Value)
                {
                    case TapeColors.Clear: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInClearTape); break;
                    case TapeColors.White: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape); break;
                    case TapeColors.Black: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInBlackTape); break;

                }
            }
            else if (CurrentComponentPackage != null)
            {
                switch (CurrentComponentPackage.TapeColor.Value)
                {
                    case TapeColors.Clear: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInClearTape); break;
                    case TapeColors.White: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape); break;
                    case TapeColors.Black: Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInBlackTape); break;

                }
            }
            else
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartInWhiteTape);
        }

        public void RectangleLocatorTimeout()
        {
            _locatorViewModel.UnregisterRectangleLocatedHandler(this);
        }

        public void RectangleLocatorAborted()
        {
            _locatorViewModel.UnregisterRectangleLocatedHandler(this);
        }

        public RelayCommand PickCurrentPartCommand { get; }

        public RelayCommand InspectCurrentPartCommand { get; }

        public RelayCommand RecycleCurrentPartCommand { get; }
        public RelayCommand SaveComponentPackageCommand { get; }

        public RelayCommand ShowDataSheetCommand { get; }
        public RelayCommand ShowComponentDetailCommand { get; }
        public RelayCommand ShowComponentPackageDetailCommand { get; }

        public RelayCommand NextPartCommand { get; }

        public RelayCommand CenterOnPartCommand { get; }

        public abstract Point2D<double> CurrentPartLocation { get; }

        public abstract double? PickHeight { get; }

        string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }
    }
}
