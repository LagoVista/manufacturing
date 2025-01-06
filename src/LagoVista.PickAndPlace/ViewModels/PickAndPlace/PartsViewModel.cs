﻿using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using Microsoft.Extensions.Configuration;
using NLog.Config;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PartsViewModel : MachineViewModelBase, IPartsViewModel
    {
        
        ObservableCollection<StripFeeder> _stripFeeders;
        ObservableCollection<AutoFeeder> _autoFeeders;

        private readonly IRestClient _restClient;
        private readonly ILocatorViewModel _locatorViewModel;

        StagingPlateUtils _stagingPlateUtils = null;
        StripFeederService _sfLocator;

        PickAndPlaceJob _job;




        public PartsViewModel(IMachineRepo machineRepo,  ILocatorViewModel locatorViewModel, IRestClient restClient, ILogger logger) : base(machineRepo)
        {
            _restClient = restClient;
            _locatorViewModel = locatorViewModel;
            _sfLocator = new StripFeederService(machineRepo.CurrentMachine.Settings);
       
            RefreshBoardCommand = new RelayCommand(() => RefreshAsync());
            RefreshConfigurationPartsCommand = new RelayCommand(RefreshConfigurationParts);

            GoToFeederOriginCommand  = CreatedMachineConnectedCommand(() => GoTo(LocationType.FeederOrigin), () => CurrentStripFeeder != null);
            GoToFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.FeederReferenceHole), () => CurrentStripFeeder != null);

            SetFeederOriginCommand = CreatedMachineConnectedCommand(() => SetLocation(SetTypes.FeederOrigin), () => CurrentStripFeeder != null || CurrentAutoFeeder != null);
            SetFeederFiducialLocationCommand = CreatedMachineConnectedCommand(() => SetLocation(SetTypes.FeederFiducial), () => CurrentAutoFeeder != null);
            SetPartPickLocationCommand = CreatedMachineConnectedCommand(() => SetLocation(SetTypes.FeederPickLocation), () => CurrentAutoFeeder != null);
            SetFirstFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => SetLocation(SetTypes.FirstFeederRowReferenceHole), () => CurrentStripFeederRow != null);
            SetLastFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => SetLocation(SetTypes.LastFeederRowReferenceHole), () => CurrentStripFeederRow != null);

            GoToFirstFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.FirstFeederRowReferenceHole), () => CurrentStripFeederRow != null);
            GoToLastFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.LastFeederRowReferenceHole), () => CurrentStripFeederRow != null);

            GoToFirstPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.FirstPart), () => CurrentStripFeederRow != null);
            GoToCurrentPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.CurrentPart), () => CurrentStripFeederRow != null);
            GoToLastPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.LastPart), () => CurrentStripFeederRow != null);
            GoToNextPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.NextPart), () => CurrentStripFeederRow != null);
            GoToPreviousPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.PreviousPart), () => CurrentStripFeederRow != null);
        }

        private void GoTo(LocationType moveTypes)
        {

        }

        private void SetLocation(SetTypes setType)
        {

        }

        
        private void GoToFeederReferenceHole()
        {
            var coord =     _stagingPlateUtils.ResolveStagePlateWorkSpaceLocation(CurrentStripFeeder.ReferenceHoleColumn.Text, CurrentStripFeeder.ReferenceHoleRow.Text);
            Machine.GotoPoint(coord);
        }
        

        protected async override void MachineChanged(IMachine machine)
        {
            if (_machineRepo.HasValidMachine)
            {
                await LoadFeeders();
            }
            base.MachineChanged(machine);
        }

        private async Task LoadFeeders()
        {
            var autoFeeders = await _restClient.GetListResponseAsync<AutoFeeder>($"/api/mfg/machine/{_machineRepo.CurrentMachine.Settings.Id}/autofeeders?loadcomponents=true");
            var stripFeeders = await _restClient.GetListResponseAsync<StripFeeder>($"/api/mfg/machine/{_machineRepo.CurrentMachine.Settings.Id}/stripfeeders?loadcomponents=true");

            var componentCategories = await _restClient.GetListResponseAsync<EntityBase>("/api/categories/component");
            ComponentCategories = new ObservableCollection<EntityHeader>(componentCategories.Model.Select(c => EntityHeader.Create(c.Id, c.Key, c.Name)));

            _stripFeeders = new ObservableCollection<StripFeeder>(stripFeeders.Model);
            _autoFeeders = new ObservableCollection<AutoFeeder>(autoFeeders.Model);

            RaisePropertyChanged(nameof(StripFeeders));
            RaisePropertyChanged(nameof(AutoFeeders));
        }

        public async Task<InvokeResult> SaveCurrentFeederAsync()
        {
            if (_currentAutoFeeder != null)
                return await _restClient.PutAsync("/api/mfg/stripfeeder", CurrentStripFeeder);
            else if (_currentAutoFeeder != null)
                return await _restClient.PutAsync("/api/mfg/autofeeder", CurrentAutoFeeder);

            return InvokeResult.FromError("No current feeder.");
        }

        public async void LoadComponent(string componentId)
        {
            var component = await _restClient.GetAsync<DetailResponse<Manufacturing.Models.Component>>($"/api/mfg/component/{componentId}");
            SelectedComponent = component.Result.Model;
        }

        public async void LoadComponentsByCategory(string categoryKey)
        {
            var components = await _restClient.GetListResponseAsync<ComponentSummary>($"/api/mfg/components?componentType={categoryKey}");
            Components = new ObservableCollection<ComponentSummary>(components.Model);
        }

        public Task RefreshAsync()
        {
            return InitAsync();
        }

        ObservableCollection<ComponentSummary> _components;
        public ObservableCollection<ComponentSummary> Components
        {
            get => _components;
            set => Set(ref _components, value);
        }

        ObservableCollection<EntityHeader> _componentCategories;
        public ObservableCollection<EntityHeader> ComponentCategories
        {
            get => _componentCategories;
            set => Set(ref _componentCategories, value);
        }

        string _selectedCategoryKey;
        public string SelectedCategoryKey
        {
            get => _selectedCategoryKey;
            set
            {
                if(value == _selectedCategoryKey) return;
                {
                    Set(ref _selectedCategoryKey, value);
                    if (!string.IsNullOrEmpty(value)) 
                        LoadComponentsByCategory(value);
                }
            }
        }

        string _selectedComponentSummaryId;
        public string SelectedComponentSummaryId
        {
            get => _selectedComponentSummaryId;
            set 
            {
                if (value != _selectedComponentSummaryId)
                {
                    Set(ref _selectedComponentSummaryId, value);
                    if (!string.IsNullOrEmpty(value))
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

        public ObservableCollection<MachineStagingPlate> StagingPlates { get => _machineRepo.CurrentMachine.Settings.StagingPlates; }
        public ObservableCollection<MachineFeederRail> FeederRails { get => _machineRepo.CurrentMachine.Settings.FeederRails; }

        public ObservableCollection<StripFeeder> StripFeeders { get => _stripFeeders; }
        public ObservableCollection<AutoFeeder> AutoFeeders { get => _autoFeeders; }

        public InvokeResult<PlaceableParts> ResolvePart(Manufacturing.Models.Component part)
        {
            return InvokeResult<PlaceableParts>.Create(null);
        }

        StripFeeder _currentStripFeeder;
        public StripFeeder CurrentStripFeeder 
        { 
            get => _currentStripFeeder;
            set
            {  
                Set(ref _currentStripFeeder, value);               
                RaiseCanExecuteChanged();
                if (value != null)
                    _stagingPlateUtils = new StagingPlateUtils(StagingPlates.First(sp => sp.Id == value.StagingPlate.Id));
                else
                    _stagingPlateUtils = null;

            }
        }

        StripFeederRow _currentStripFeederRow;
        public StripFeederRow CurrentStripFeederRow
        {
            get => _currentStripFeederRow;
            set
            {
                Set(ref _currentStripFeederRow, value);
                RaiseCanExecuteChanged();
            }
        }

        AutoFeeder _currentAutoFeeder;
        public AutoFeeder CurrentAutoFeeder 
        { 
            get => _currentAutoFeeder;
            set
            {
                Set(ref _currentAutoFeeder, value);
                RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<PlaceableParts> ConfigurationParts { get; private set; } = new ObservableCollection<PlaceableParts>();


        ComponentPackage _currentPackage;
        public ComponentPackage CurrentPackage { get => _currentPackage; }

        PlaceableParts _currentPlaceableParts;
        public PlaceableParts CurrentPlaceableParts { get => _currentPlaceableParts; }

        Manufacturing.Models.Component _componentToBePlaced;
        public Manufacturing.Models.Component CurrentComponentToBePlaced { get => _componentToBePlaced; }


        public bool CanMoveToNextInTape()
        {
            return false;
        }

        public bool CanMoveToPrevInTape()
        {
            return false;
        }
        public bool CanMoveToReferenceHoleInTape()
        {
            return false;
        }

        public bool CanMoveToFirstPartInTape()
        {
            return false;
        }

        public bool CanMoveToCurrentPartInTape()
        {
            return false;
        }

        public void RefreshConfigurationParts()
        {
            ConfigurationParts.Clear();
            var commonParts = _job.BoardRevision.PcbComponents.Where(prt => prt.Included).GroupBy(prt => prt.PackageAndValue.ToLower());

            foreach (var entry in commonParts)
            {
                var part = new PlaceableParts()
                {
                    Value = entry.First().Value.ToUpper(),
                    PackageId = entry.First().Package?.Id,
                    PackageName = entry.First().PackageName.ToUpper(),
                    ComponentName = entry.First().Component?.Text,
                };

                part.Parts = new ObservableCollection<PcbComponent>();

                foreach (var specificPart in entry)
                {
                    var placedPart = _job.BoardRevision.PcbComponents.Where(cmp => cmp.Name == specificPart.Name && cmp.Key == specificPart.Key).FirstOrDefault();
                    if (placedPart != null)
                    {
                        part.Parts.Add(placedPart);
                    }
                }

                ConfigurationParts.Add(part);
            }
        }

        public RelayCommand RefreshBoardCommand { get; }
        public RelayCommand RefreshConfigurationPartsCommand { get; }

        public RelayCommand SetFeederOriginCommand { get; set; }
        public RelayCommand SetPartPickLocationCommand { get; set; }
        public RelayCommand SetFeederFiducialLocationCommand { get; set; }
        public RelayCommand GoToFeederOriginCommand { get; set; }
        public RelayCommand GoToFeederReferenceHoleCommand { get; set; }


        public RelayCommand SetFirstFeederReferenceHoleCommand { get; }
        public RelayCommand SetLastFeederReferenceHoleCommand { get; }
        public RelayCommand GoToFirstFeederReferenceHoleCommand { get; }
        public RelayCommand GoToLastFeederReferenceHoleCommand { get; }

        
        public RelayCommand GoToFirstPartCommand { get; set; }
        public RelayCommand GoToLastPartCommand { get; set; }
        public RelayCommand GoToCurrentPartCommand { get; set; }
        public RelayCommand GoToNextPartCommand { get; set; }
        public RelayCommand GoToPreviousPartCommand { get; set; }

    }
}
