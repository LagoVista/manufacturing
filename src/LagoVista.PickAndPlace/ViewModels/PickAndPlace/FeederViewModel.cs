using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.ViewModels.Machine;
using RingCentral;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public abstract class FeederViewModel : MachineViewModelBase, IFeederViewModel
    {

        private enum PickStates
        {
            Idle,
            Picked,
            Inspecting,
            TrialPlaced,
        }

        private PickStates _pickStates = PickStates.Idle;

        private readonly IRestClient _restClient;
        private readonly IMachineUtilitiesViewModel _machineUtilitiesViewModel;

        protected FeederViewModel(IMachineRepo machineRepo, IRestClient resteClient, IMachineUtilitiesViewModel machineUtilitiesViewModel) : base(machineRepo)
        {
            _restClient = resteClient ?? throw new ArgumentNullException(nameof(resteClient));
            _machineUtilitiesViewModel = machineUtilitiesViewModel ?? throw new ArgumentNullException(nameof(machineUtilitiesViewModel));
            PickCurrentPartCommand = new RelayCommand(async () => await PickCurrentPartAsync(), () => CurrentComponent != null);
            RecycleCurrentPartCommand = new RelayCommand(async () => await RecycleCurrentPartAsync(), () => CurrentComponent != null);
            InspectCurrentPartCommand = new RelayCommand(async () => await InspectCurrentPartAsync(), () => CurrentComponent != null);
            SaveComponentPackageCommand = CreatedCommand(SaveComponentPackage, () => CurrentComponentPackage != null);
        }

        public override async Task InitAsync()
        {
            var componentCategories = await _restClient.GetListResponseAsync<EntityBase>("/api/categories/component");
            ComponentCategories = new ObservableCollection<EntityHeader>(componentCategories.Model.Select(c => EntityHeader.Create(c.Id, c.Key, c.Name)));
            ComponentCategories.Insert(0, new EntityHeader() { Id = StringExtensions.NotSelectedId, Key = StringExtensions.NotSelectedId, Text = "-select component category-" });
            Components = new ObservableCollection<ComponentSummary>();
            Components.Insert(0, new ComponentSummary() { Id = StringExtensions.NotSelectedId, Key=StringExtensions.NotSelectedId, Name = "-select component category first-" });
            SelectedCategoryKey = StringExtensions.NotSelectedId;
            SelectedComponentSummaryId = StringExtensions.NotSelectedId;
      
            await base.InitAsync();
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
            if(CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current part selected, can not pick.");
                return InvokeResult.FromError("No current part selected, can not pick.");
            }

            var currentLocation = CurrentPartLocation;
            if (currentLocation == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not identify location of current part.");
                _pickStates = PickStates.Idle;
                return InvokeResult.FromError("Could not identify location of current part.");
            }            

            Machine.SendSafeMoveHeight();
            if(CurrentComponent.ComponentPackage.HasValue)
            {
                if(!EntityHeader.IsNullOrEmpty(CurrentComponent.ComponentPackage.Value.NozzleTip))
                {
                    var toolHead = MachineConfiguration.ToolHeads.FirstOrDefault(th => th.CurrentNozzle?.Id == CurrentComponent.ComponentPackage.Value.NozzleTip.Id);
                    if(toolHead != null)
                        await Machine.MoveToToolHeadAsync(toolHead);
                    else
                        await Machine.MoveToToolHeadAsync(MachineConfiguration.ToolHeads.First());
                }
                else
                    await Machine.MoveToToolHeadAsync(MachineConfiguration.ToolHeads.First());
            }
            else
                await Machine.MoveToToolHeadAsync( MachineConfiguration.ToolHeads.First());
            
            Machine.SendCommand(CurrentPartLocation.ToGCode());

            if(PickHeight.HasValue)
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
                SelectedComponent = null;
                SelectedCategoryKey = StringExtensions.NotSelectedId;
            }
            else
            {
                
                var component = await _restClient.GetAsync<DetailResponse<Manufacturing.Models.Component>>($"/api/mfg/component/{componentId}");
                SelectedComponent = component.Result.Model;
                
                if (SelectedComponent.ComponentType.Key != _selectedCategoryKey)
                {
                    _selectedCategoryKey = SelectedComponent.ComponentType.Key;
                    await LoadComponentsByCategory(_selectedCategoryKey);
                    RaisePropertyChanged(nameof(SelectedCategoryKey));
                }

                SelectedComponentSummaryId = componentId;
            }
        }

        public async Task LoadComponentsByCategory(string categoryKey)
        {
            var components = await _restClient.GetListResponseAsync<ComponentSummary>($"/api/mfg/components?componentType={categoryKey}");
            Components = new ObservableCollection<ComponentSummary>(components.Model);
            Components.Insert(0, new ComponentSummary() {Id = StringExtensions.NotSelectedId, Name = "-select component-" });
            SelectedComponentSummaryId = StringExtensions.NotSelectedId;
        }

        public Task<InvokeResult> MoveToCurrentPartInFeederAsync()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current part selected, can not inspect.");
                return Task.FromResult( InvokeResult.FromError("No current part selected, can not inspect."));
            }

            Machine.SendSafeMoveHeight();
            Machine.SendSafeMoveHeight();
            Machine.SendCommand(CurrentPartLocation.ToGCode());

            return Task.FromResult(InvokeResult.Success);
        }

        public Task<InvokeResult> MoveToPartInFeederAsync(Component component)
        {
            CurrentComponent = component;
            return MoveToCurrentPartInFeederAsync();
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
                if (value == null)
                    value = StringExtensions.NotSelectedId;
                Set(ref _selectedCategoryKey, value);

                if (SelectedCategoryKey.HasValidId())
                    LoadComponentsByCategory(value);
                else
                {
                    Components = new ObservableCollection<ComponentSummary>();
                    Components.Insert(0, new ComponentSummary() { Id = StringExtensions.NotSelectedId, Name = "-select component category first-" });
                    SelectedComponentSummaryId = StringExtensions.NotSelectedId;
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
                    else
                        SelectedComponent = null;
                }
            }
        }
   
        Manufacturing.Models.Component _selectedComponent;
        public Manufacturing.Models.Component SelectedComponent
        {
            get => _selectedComponent;
            set => Set(ref _selectedComponent, value);
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
                Set(ref _currentComponent, value);
                if(value != null)
                {
                    SelectedComponentSummaryId = value.Id;
                }
                PickCurrentPartCommand.RaiseCanExecuteChanged();
                RecycleCurrentPartCommand.RaiseCanExecuteChanged();
                InspectCurrentPartCommand.RaiseCanExecuteChanged();
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

        public RelayCommand PickCurrentPartCommand { get; }

        public RelayCommand InspectCurrentPartCommand { get; }

        public RelayCommand RecycleCurrentPartCommand { get; }
        public RelayCommand SaveComponentPackageCommand { get; }


        public abstract Point2D<double> CurrentPartLocation { get;  }

        public abstract double? PickHeight { get;  }

        string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }
    }
}
