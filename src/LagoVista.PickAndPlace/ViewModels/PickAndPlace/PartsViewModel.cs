using LagoVista.Client.Core;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
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


        PickAndPlaceJob _job;

        public PartsViewModel(IMachineRepo machineRepo,  ILocatorViewModel locatorViewModel, IRestClient restClient, ILogger logger) : base(machineRepo)
        {
            _restClient = restClient;
            _locatorViewModel = locatorViewModel;
       
            RefreshBoardCommand = new RelayCommand(() => RefreshAsync());
            RefreshConfigurationPartsCommand = new RelayCommand(RefreshConfigurationParts);

            GoToFeederOriginCommand  = CreatedMachineConnectedCommand(() => GoTo(LocationType.FeederOrigin), () => CurrentStripFeeder != null);
            GoToFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.FeederReferenceHole), () => CurrentStripFeeder != null);

            SetFeederOriginCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(SetTypes.FeederOrigin), () => CurrentStripFeeder != null || CurrentAutoFeeder != null);
            SetFeederFiducialLocationCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(SetTypes.FeederFiducial), () => CurrentAutoFeeder != null);
            SetPartPickLocationCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(SetTypes.FeederPickLocation), () => CurrentAutoFeeder != null);
            SetFirstFeederReferenceHoleCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(SetTypes.FirstFeederRowReferenceHole), () => CurrentStripFeederRow != null);
            SetLastFeederReferenceHoleCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(SetTypes.LastFeederRowReferenceHole), () => CurrentStripFeederRow != null);

            GoToFirstFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.FirstFeederRowReferenceHole), () => CurrentStripFeederRow != null);
            GoToLastFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.LastFeederRowReferenceHole), () => CurrentStripFeederRow != null);

            GoToFirstPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.FirstPart), () => CurrentStripFeederRow != null);
            GoToCurrentPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.CurrentPart), () => CurrentStripFeederRow != null);
            GoToLastPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.LastPart), () => CurrentStripFeederRow != null && CurrentPartIndex < TotalPartsInFeederRow);
            GoToNextPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.NextPart), () => CurrentStripFeederRow != null);
            GoToPreviousPartCommand = CreatedMachineConnectedCommand(() => GoTo(LocationType.PreviousPart), () => CurrentStripFeederRow != null && CurrentPartIndex > 1);

            SaveCurrentFeederCommand = new RelayCommand(async () => await SaveCurrentFeederAsync(), () => CurrentAutoFeeder != null || CurrentStripFeeder != null);
            RegisterCommandHandler(SaveCurrentFeederCommand);
        }

        private void GoTo(LocationType moveType)
        {
            var stagePlate = MachineConfiguration.StagingPlates.Single(sp => sp.Id == CurrentStripFeeder.StagingPlate.Id);
            if(stagePlate == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not move, staging plate not set on feeder.");
                return;
            }

            var stagePlateReferenceLocation = StagingPlateUtils.ResolveStagePlateWorkSpakeLocation(stagePlate, CurrentStripFeeder.ReferenceHoleColumn, CurrentStripFeeder.ReferenceHoleRow);
            if (stagePlateReferenceLocation == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not move, reference hole or column not set on strip feeder.");
                return;
            }

            var component = CurrentStripFeederRow.Component ?? null;
            var package = component?.Value?.ComponentPackage?.Value;

            var feederIsVertical = CurrentStripFeeder.Orientation.Value == FeederOrientations.Vertical;

            var feederOrigin = stagePlateReferenceLocation.SubtractWithConditionalSwap(feederIsVertical, CurrentStripFeeder.ReferenceHoleOffset);            

            switch (moveType)
            {
                case LocationType.FeederReferenceHole: Machine.GotoPoint(stagePlateReferenceLocation); break;
                case LocationType.FeederOrigin: Machine.GotoPoint(feederOrigin); break;
                case LocationType.FirstFeederRowReferenceHole:
                    {
                        if (CurrentStripFeederRow == null)
                        {
                            Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Can not move to row, no row selected.");
                        }
                        else
                        {
                            if (UseCalculated)
                            {
                                var deltaY = ((CurrentStripFeederRow.RowIndex - 1) * CurrentStripFeeder.RowWidth) + (TapeSize.ToDouble() - 2);
                                var deltaX = 5;
                                var estimatedTapeHole = feederOrigin + (feederIsVertical ? new Point2D<double>(deltaY, -deltaX) : new Point2D<double>(deltaX, deltaY));
                                Machine.GotoPoint(estimatedTapeHole);
                            }
                            else
                            {
                                var tapeHoleOrigin = feederOrigin.AddWithConditionalSwap(feederIsVertical, CurrentStripFeederRow.FirstTapeHoleOffset);
                                Machine.GotoPoint(tapeHoleOrigin);
                            }
                        }
                    }
                        break;
                case LocationType.LastFeederRowReferenceHole:
                    {
                        if (CurrentStripFeederRow == null)
                        {
                            Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Can not move to row, no row selected.");
                        }
                        else
                        {
                            if (UseCalculated || (CurrentStripFeederRow.LastTapeHoleOffset.X == 0 && CurrentStripFeederRow.LastTapeHoleOffset.Y == 0))
                            {
                                if (CurrentStripFeederRow.FirstTapeHoleOffset.X == 0 && CurrentStripFeederRow.FirstTapeHoleOffset.Y == 0)
                                {

                                }
                                else
                                {
                                    var deltaY = ((CurrentStripFeederRow.RowIndex - 1) * CurrentStripFeeder.RowWidth) + (TapeSize.ToDouble() - 2);
                                    var deltaX = TotalPartsInFeederRow * TapePitch.ToDouble();

                                    var lastTapeHole = feederOrigin + (feederIsVertical ? new Point2D<double>(deltaY, -deltaX) : new Point2D<double>(deltaX, deltaY));
                                    Machine.GotoPoint(lastTapeHole);
                                }
                            }
                            else
                            {
                                var tapeHoleOrigin = feederOrigin.AddWithConditionalSwap(feederIsVertical, CurrentStripFeederRow.LastTapeHoleOffset);
                                Machine.GotoPoint(tapeHoleOrigin);
                            }
                        }
                    }
                    break;
                case LocationType.FirstPart:
                    if (CurrentStripFeederRow == null)
                    {
                        Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Can not move to row, no row selected.");
                    }
                    else
                    {
                        CurrentPartIndex = 1;
                        var partLocation = ResolvePartInTape(feederOrigin);
                        Machine.GotoPoint(partLocation.Result);
                    }
                    break;
                case LocationType.PreviousPart:
                    {
                        CurrentPartIndex--;
                        var partLocation = ResolvePartInTape(feederOrigin);
                        Machine.GotoPoint(partLocation.Result);
                    }
                    break;
                case LocationType.CurrentPart:
                    {
                        if (CurrentStripFeederRow == null)
                        {
                            Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Can not move to row, no row selected.");
                        }
                        else
                        {
                            CurrentPartIndex = CurrentStripFeederRow.CurrentPartIndex;
                            var partLocation = ResolvePartInTape(feederOrigin);
                            Machine.GotoPoint(partLocation.Result);
                        }
                    }
                    break;
                case LocationType.NextPart:
                    {
                        CurrentPartIndex++;
                        var partLocation = ResolvePartInTape(feederOrigin);
                        Machine.GotoPoint(partLocation.Result);
                    }
                    break;
                case LocationType.LastPart:
                    {
                        if (CurrentStripFeederRow == null)
                        {
                            Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Can not move to row, no row selected.");
                        }
                        else
                        {
                            CurrentPartIndex = TotalPartsInFeederRow;
                            var partLoction = ResolvePartInTape(feederOrigin);
                            Machine.GotoPoint(partLoction.Result);
                        }
                    }
                    break;

            }

            RaiseCanExecuteChanged();
        }

        

        private InvokeResult<Point2D<double>> ResolvePartInTape(Point2D<double> feederOrigin)
        {
            if(CurrentStripFeeder == null)
            {
                return InvokeResult<Point2D<double>>.FromError("No current strip feeder selected, can not calculate location.");
            }

            if (CurrentStripFeederRow == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"No selected row on strip feeder {CurrentStripFeeder.Name}, can not calculate location.");
            }


            if (CurrentStripFeederRow.FirstTapeHoleOffset == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"No first tape hole offset configured for {CurrentStripFeeder.Name}/{CurrentStripFeederRow.RowIndex}, can not calculate location.");
            }

            if (CurrentStripFeederRow.FirstTapeHoleOffset.X == 0 && CurrentStripFeederRow.FirstTapeHoleOffset.Y == 0)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, ($"First tape role index is (0,0) on {CurrentStripFeeder.Name}/{CurrentStripFeederRow.RowIndex} this is likely an error but will continue.");
            }

            var feederIsVertical = CurrentStripFeeder.Orientation.Value == FeederOrientations.Vertical;

            if(TapeSize == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"No tape size configured for {CurrentStripFeeder}/{CurrentStripFeederRow.RowIndex} - assuming 8mm tape.");
            }    

            if(TapePitch == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"No tape pitch configured for {CurrentStripFeeder}/{CurrentStripFeederRow.RowIndex} - assuming 4 mm tape pitch.");
            }

            var tapeSize = TapeSize.ToDouble();
            var tapePitch = TapePitch.ToDouble();

            var tapeHoleOrigin = feederOrigin.AddWithConditionalSwap(feederIsVertical, CurrentStripFeederRow.FirstTapeHoleOffset);
            if (feederIsVertical)
            {
                tapeHoleOrigin.SubtractFromX(((tapeSize - 1.5) / 2));
                tapeHoleOrigin.AddToY(tapePitch / 2);
            }
            else
            {
                tapeHoleOrigin.SubtractFromY(((tapeSize - 1.5) / 2));
                tapeHoleOrigin.AddToX(tapePitch / 2);
            }
            return InvokeResult<Point2D<double>>.Create(tapeHoleOrigin);
        }

        private void SetLocation(SetTypes setType)
        {
            var stagePlate = MachineConfiguration.StagingPlates.Single(sp => sp.Id == CurrentStripFeeder.StagingPlate.Id);
            if (stagePlate == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not move, staging plate not set on feeder.");
                return;
            }

            var stagePlateReferenceLocation = StagingPlateUtils.ResolveStagePlateWorkSpakeLocation(stagePlate, CurrentStripFeeder.ReferenceHoleColumn, CurrentStripFeeder.ReferenceHoleRow);
            if (stagePlateReferenceLocation == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not move, reference hole or column not set on strip feeder.");
                return;
            }


            var feederIsVertical = CurrentStripFeeder.Orientation.Value == FeederOrientations.Vertical;
            var feederOrigin = stagePlateReferenceLocation.SubtractWithConditionalSwap(feederIsVertical, CurrentStripFeeder.ReferenceHoleOffset);

            switch (setType)
            {
                case SetTypes.FirstFeederRowReferenceHole:
                    {
                        var delta = Machine.MachinePosition.ToPoint2D() - feederOrigin;
                        delta.Round(3);
                        CurrentStripFeederRow.FirstTapeHoleOffset = feederIsVertical ? new Point2D<double>(-delta.Y, delta.X) : delta;
                    }
                        break;
                case SetTypes.LastFeederRowReferenceHole:
                    {
                        var delta = Machine.MachinePosition.ToPoint2D() - feederOrigin;
                        delta.Round(3);
                        CurrentStripFeederRow.LastTapeHoleOffset = feederIsVertical ? new Point2D<double>(-delta.Y, delta.X) : delta;
                    }
                        break;
            }
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
            if (CurrentStripFeeder != null)
                return await _restClient.PutAsync("/api/mfg/stripfeeder", CurrentStripFeeder);
            else if (CurrentAutoFeeder != null)
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


        Component _currentComponent;
        public Component CurrentComponent
        {
            get => _currentComponent;
        }

        ComponentPackage _currentComponentPackage;
        public ComponentPackage CurrentComponentPackage
        {
            get => _currentComponentPackage;
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
                if(value != null)
                {
                    _tapeSize = _currentStripFeeder.TapeSize;                    
                }
                else
                {
                    _tapeSize = _currentStripFeeder.TapeSize;
                }

                RaisePropertyChanged(nameof(TapeSize));
                RaiseCanExecuteChanged();                
            }
        }

        EntityHeader<TapePitches> _tapePitch;
        public EntityHeader<TapePitches> TapePitch
        {
            get => _tapePitch;
        }

        EntityHeader<TapeSizes> _tapeSize;
        public EntityHeader<TapeSizes> TapeSize
        {
            get => _tapeSize;
        }

        StripFeederRow _currentStripFeederRow;
        public StripFeederRow CurrentStripFeederRow
        {
            get => _currentStripFeederRow;
            set
            {
                Set(ref _currentStripFeederRow, value);
                if (value == null)
                {
                    _currentComponent = null;
                    _tapePitch = null;
                    _currentComponentPackage = null;
                }
                else
                {
                    _currentComponent = value.Component?.Value;
                    CurrentPartIndex = value.CurrentPartIndex;

                    if (_currentComponent != null)
                    {
                        _currentComponentPackage = _currentComponent.ComponentPackage?.Value;
                        if (_currentComponentPackage != null)
                        {
                            _tapePitch = _currentComponentPackage.TapePitch;
                        }
                        else
                        {
                            _tapePitch = null;
                        }
                    }
                    else
                    {
                        _currentComponent = null;
                    }

                    TotalPartsInFeederRow = Convert.ToInt32(Math.Floor(CurrentStripFeeder.FeederLength /( TapePitch != null ?  TapePitch.ToDouble() : 4)));
                    AvailablePartsInFeederRow = TotalPartsInFeederRow - CurrentStripFeederRow.CurrentPartIndex;
                }

                RaisePropertyChanged(nameof(CurrentComponent));
                RaisePropertyChanged(nameof(TapePitch));
                RaisePropertyChanged(nameof(CurrentComponentPackage));

                
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
                if(value != null )
                {
                    _tapeSize = value.TapeSize;
                }
                else
                {
                    _tapeSize = null;
                }

                RaisePropertyChanged(nameof(TapeSize));
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

        private bool _useCalculted;
        public bool UseCalculated
        {
            get => _useCalculted;
            set => Set(ref _useCalculted, value);
        }

        private int _currentPartIndex;
        public int CurrentPartIndex
        {
            get => _currentPartIndex;
            set => Set(ref _currentPartIndex, value);
        }

        private int _totalPartsInFeederRow;
        public int TotalPartsInFeederRow
        {
            get => _totalPartsInFeederRow;
            set => Set(ref _totalPartsInFeederRow, value);
        }

        private int _availablePartsInFeederRow;
        public int AvailablePartsInFeederRow
        {
            get => _availablePartsInFeederRow;
            set => Set(ref _availablePartsInFeederRow, value);
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

        public RelayCommand SaveCurrentFeederCommand { get; set; }

    }
}
