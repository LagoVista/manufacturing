using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public enum StripFeederLocationTypes
    {
        FeederOrigin,
        FeederReferenceHole,
        FirstFeederRowReferenceHole,
        LastFeederRowReferenceHole,
        FirstPart,
        LastPart,
        NextPart,
        PreviousPart,
        CurrentPart,
    }

    public enum StripFeederSetTypes
    {
        FeederOrigin,
        FirstFeederRowReferenceHole,
        LastFeederRowReferenceHole,
        FeederFiducial,
        FeederPickLocation,
    }



    public class StripFeederViewModel : FeederViewModel, IStripFeederViewModel
    {
        ObservableCollection<StripFeeder> _stripFeeders;
        private readonly IRestClient _restClient;
        private readonly ILocatorViewModel _locatorVM;
        private bool _isEditing;

        public StripFeederViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorVM, IRestClient restClient) : base(machineRepo, restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _locatorVM = locatorVM ?? throw new ArgumentNullException(nameof(locatorVM));

            SetFeederOriginCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(StripFeederSetTypes.FeederOrigin), () => Current != null);
            SetFirstFeederReferenceHoleCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(StripFeederSetTypes.FirstFeederRowReferenceHole), () => CurrentRow != null);
            SetLastFeederReferenceHoleCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(StripFeederSetTypes.LastFeederRowReferenceHole), () => CurrentRow != null);

            GoToFeederOriginCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FeederOrigin), () => Current != null);
            GoToFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FeederReferenceHole), () => Current != null);

            GoToFirstFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FirstFeederRowReferenceHole), () => CurrentRow != null);
            GoToLastFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.LastFeederRowReferenceHole), () => CurrentRow != null);

            GoToFirstPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FirstPart), () => CurrentRow != null && CurrentPartIndex > 1);
            GoToCurrentPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.CurrentPart), () => CurrentRow != null);
            GoToLastPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.LastPart), () => CurrentRow != null);
            GoToNextPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.NextPart), () => (CurrentRow != null && CurrentPartIndex < TotalPartsInFeederRow));
            GoToPreviousPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.PreviousPart), () => (CurrentRow != null && CurrentPartIndex > 1));

            AddCommand = CreatedCommand(Add, () => machineRepo.HasValidMachine && SelectedTemplateId.HasValidId());
            SaveCommand = CreatedCommand(Save, () => Current != null);
            CancelCommand = CreatedCommand(Cancel, () => Current != null);
            DoneRowCommand = CreatedCommand(() => DoneRow(), () => CurrentRow != null);
            CancelRowCommand = CreatedCommand(() => CurrentRow = null, () => CurrentRow != null);

            RefreshTemplatesCommand = CreatedCommand(async () => await LoadTemplates()); 
        }

        public async override Task InitAsync()
        {
            await LoadTemplates();
            await base.InitAsync();
        }

        private async Task LoadTemplates()
        {
            var result = await _restClient.GetListResponseAsync<StripFeederTemplate>("/api/mfg/stripfeeder/templates");
            if(result.Successful)
            {
                var feeders = result.Model.ToList();
                feeders.Insert(0, new StripFeederTemplate() { Id = "-1", Name = "-select feeder template-" });
                Templates = new ObservableCollection<StripFeederTemplate>(feeders);
            }

            SelectedTemplateId = StringExtensions.NotSelectedId;
        }

        protected async override void MachineChanged(IMachine machine)
        {
            await LoadFeeders();
            base.MachineChanged(machine);
        }

        private async void Add()
        {
            var result = await _restClient.GetAsync<DetailResponse<StripFeeder>>($"/api/mfg/stripfeeder/template/{SelectedTemplateId}/factory");
            if (result.Successful)
            {
                _isEditing = false;
                Current = result.Result.Model;
                Current.Machine = MachineConfiguration.ToEntityHeader();
                Current.Key = Current.Id.ToLower();
            }
        }

        private void Cancel()
        {
            Current = null;
        }

        private void GoTo(StripFeederLocationTypes locationType)
        {
            var result = FindLocation(locationType);  
            if(result.Successful)
            {
                Machine.GotoPoint(result.Result);
            }            
        }

        private async Task LoadFeeders()
        {
            var stripFeeders = await _restClient.GetListResponseAsync<StripFeeder>($"/api/mfg/machine/{MachineRepo.CurrentMachine.Settings.Id}/stripfeeders?loadcomponents=true");

            _stripFeeders = new ObservableCollection<StripFeeder>(stripFeeders.Model);

            RaisePropertyChanged(nameof(Feeders));
        }

        private void DoneRow()
        {
            CurrentRow.Component = EntityHeader<Component>.Create(CurrentComponent.Id, CurrentComponent.Key, CurrentComponent.Name);
            Current = null;
        }

        public async void Save()
        {
            if (Current != null)
            {
                var result = _isEditing ? await _restClient.PutAsync("/api/mfg/stripfeeder", Current) : 
                                await _restClient.PostAsync("/api/mfg/stripfeeder", Current);
                if(result.Successful)
                {
                    if(!_isEditing)
                        Feeders.Add(Current);

                    Current = null;
                    SelectedTemplateId = StringExtensions.NotSelectedId;
                }
                else
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
                }
            }
        }       

        private void SetLocation(StripFeederSetTypes setType)
        {
            var stagePlate = MachineConfiguration.StagingPlates.Single(sp => sp.Id == Current.StagingPlate.Id);
            if (stagePlate == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not move, staging plate not set on feeder.");
                return;
            }

            var stagePlateReferenceLocation = StagingPlateUtils.ResolveStagePlateWorkSpakeLocation(stagePlate, Current.ReferenceHoleColumn, Current.ReferenceHoleRow);
            if (stagePlateReferenceLocation == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not move, reference hole or column not set on strip feeder.");
                return;
            }

            var feederIsVertical = Current.Orientation.Value == FeederOrientations.Vertical;
            var feederOrigin = stagePlateReferenceLocation.SubtractWithConditionalSwap(feederIsVertical, Current.ReferenceHoleOffset);

            switch (setType)
            {
                case StripFeederSetTypes.FeederOrigin:
                    Current.OriginOffset = (Machine.MachinePosition.ToPoint2D() - feederOrigin).Round(2);
                    break;
                case StripFeederSetTypes.FirstFeederRowReferenceHole:
                    {
                        var delta = Machine.MachinePosition.ToPoint2D() - feederOrigin;
                        delta.Round(3);
                        CurrentRow.FirstTapeHoleOffset = feederIsVertical ? new Point2D<double>(-delta.Y, delta.X) : delta;
                    }
                    break;
                case StripFeederSetTypes.LastFeederRowReferenceHole:
                    {
                        var delta = Machine.MachinePosition.ToPoint2D() - feederOrigin;
                        delta.Round(3);
                        CurrentRow.LastTapeHoleOffset = feederIsVertical ? new Point2D<double>(-delta.Y, delta.X) : delta;
                    }
                    break;
            }
        }

        public InvokeResult<Point2D<double>> FindLocation(StripFeederLocationTypes moveType)
        {
            var stagePlate = MachineConfiguration.StagingPlates.Single(sp => sp.Id == Current.StagingPlate.Id);
            if (stagePlate == null)
            {
                return InvokeResult<Point2D<double>>.FromError("Could not move, staging plate not set on feeder.");
            }

            if (Current == null)
            {
                return InvokeResult<Point2D<double>>.FromError("Could not move, no current strip feeder selected.");
            }

            var stagePlateReferenceLocation = StagingPlateUtils.ResolveStagePlateWorkSpakeLocation(stagePlate, Current.ReferenceHoleColumn, Current.ReferenceHoleRow);
            if (stagePlateReferenceLocation == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"Could not move, reference hole or column not set on strip feeder {Current.Name}.");
            }

//            var component = CurrentRow.Component ?? null;
            //var package = component?.Value?.ComponentPackage?.Value;

            var feederIsVertical = Current.Orientation.Value == FeederOrientations.Vertical;

            var feederOrigin = stagePlateReferenceLocation.SubtractWithConditionalSwap(feederIsVertical, Current.ReferenceHoleOffset) + Current.OriginOffset;

            if (moveType == StripFeederLocationTypes.FeederReferenceHole)
            {
                return InvokeResult<Point2D<double>>.Create(stagePlateReferenceLocation);
            }

            if (moveType == StripFeederLocationTypes.FeederOrigin)
            {
                return InvokeResult<Point2D<double>>.Create(feederOrigin);
            }

            // rest of methods are looking at a feeder row or part within a row...make sure we have one.
            if (CurrentRow == null)
            {
                return InvokeResult<Point2D<double>>.FromError("Can not move to row, No feeder row selected.");
            }

            var deltaY = ((CurrentRow.RowIndex - 1) * Current.RowWidth) + (TapeSize.ToDouble() - 2);
            var deltaX = 2;

            var calculateFirstReferneceHole = feederIsVertical ? new Point2D<double>(deltaY, -deltaX) : new Point2D<double>(deltaX, deltaY);
            var firstReferenceHole = CurrentRow.FirstTapeHoleOffset.IsOrigin() ? calculateFirstReferneceHole : CurrentRow.FirstTapeHoleOffset;

            switch (moveType)
            {
                case StripFeederLocationTypes.FirstFeederRowReferenceHole:
                    if (UseCalculated)
                        return InvokeResult<Point2D<double>>.Create(feederOrigin + calculateFirstReferneceHole);
                    else
                        return InvokeResult<Point2D<double>>.Create(feederOrigin.AddWithConditionalSwap(feederIsVertical, firstReferenceHole));

                case StripFeederLocationTypes.LastFeederRowReferenceHole:
                    if (!UseCalculated && !CurrentRow.LastTapeHoleOffset.IsOrigin())
                        return InvokeResult<Point2D<double>>.Create(feederOrigin.AddWithConditionalSwap(feederIsVertical, CurrentRow.LastTapeHoleOffset));

                    var deltaEndX = (TotalPartsInFeederRow - 1) * TapePitch.ToDouble();
                    return InvokeResult<Point2D<double>>.Create(feederOrigin + (feederIsVertical ? firstReferenceHole.SubtractFromY(deltaX) : firstReferenceHole.AddToX(deltaX)));
                case StripFeederLocationTypes.FirstPart:
                    CurrentPartIndex = 1;
                    break;
                case StripFeederLocationTypes.PreviousPart:
                    CurrentPartIndex--;
                    break;
                case StripFeederLocationTypes.CurrentPart:
                    CurrentPartIndex = CurrentRow.CurrentPartIndex;
                    break;
                case StripFeederLocationTypes.NextPart:
                    CurrentPartIndex++;
                    break;
                case StripFeederLocationTypes.LastPart:
                    CurrentPartIndex = TotalPartsInFeederRow;
                    break;
            }

            RaiseCanExecuteChanged();
            return ResolvePartInTape(feederOrigin);
        }

        private InvokeResult<Point2D<double>> ResolvePartInTape(Point2D<double> feederOrigin)
        {
            if (Current == null)
            {
                return InvokeResult<Point2D<double>>.FromError("No current strip feeder selected, can not calculate location.");
            }

            if (CurrentRow == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"No selected row on strip feeder {Current.Name}, can not calculate location.");
            }

            if (CurrentRow.FirstTapeHoleOffset == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"No first tape hole offset configured for {Current.Name}/{CurrentRow.RowIndex}, can not calculate location.");
            }

            if (CurrentRow.FirstTapeHoleOffset.X == 0 && CurrentRow.FirstTapeHoleOffset.Y == 0)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"First tape role index is (0,0) on {Current.Name}/{CurrentRow.RowIndex} this is likely an error but will continue.");
            }

            if (TapeSize == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"No tape size configured for {Current}/{CurrentRow.RowIndex} - assuming 8mm tape.");
            }

            if (TapePitch == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"No tape pitch configured for {Current}/{CurrentRow.RowIndex} - assuming 4 mm tape pitch.");
            }

            CurrentPartIndex = Math.Max(1, CurrentPartIndex);
            CurrentPartIndex = Math.Min(CurrentPartIndex, TotalPartsInFeederRow);

            var tapeSize = TapeSize.ToDouble();
            var tapePitch = TapePitch.ToDouble();
            var feederIsVertical = Current.Orientation.Value == FeederOrientations.Vertical;

            var tapeHoleOrigin = feederOrigin.AddWithConditionalSwap(feederIsVertical, CurrentRow.FirstTapeHoleOffset);
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

        ObservableCollection<StripFeederTemplate> _templates;
        public ObservableCollection<StripFeederTemplate> Templates
        {
            get => _templates;
            private set => Set(ref _templates, value);
        }

        StripFeeder _current;
        public StripFeeder Current
        {
            get => _current;
            set
            {
                Set(ref _current, value);
                if (value != null)
                {
                    _tapeSize = _current.TapeSize;
                }
                else
                {
                    _tapeSize = null;
                }

                RaisePropertyChanged(nameof(TapeSize));
                RaiseCanExecuteChanged();
            }
        }

        StripFeederRow _currentRow;
        public StripFeederRow CurrentRow
        {
            get => _currentRow;
            set
            {
                Set(ref _currentRow, value);
                if (value == null)
                {
                    CurrentComponent = null;
                    _tapePitch = null;
                    CurrentComponentPackage = null;
                }
                else
                {
                    CurrentPartIndex = value.CurrentPartIndex;

                    if (value.Component != null)
                    {
                        CurrentComponent = value.Component?.Value;
                        CurrentComponentPackage = CurrentComponent.ComponentPackage?.Value;
                        if (CurrentComponentPackage != null)
                        {
                            _tapePitch = CurrentComponentPackage.TapePitch;
                        }
                        else
                        {
                            _tapePitch = null;
                        }
                    }
                    else
                    {
                        SelectedCategoryKey = StringExtensions.NotSelectedId;
                        CurrentComponent = null;
                    }

                    TotalPartsInFeederRow = Convert.ToInt32(Math.Floor(Current.Size.X / (TapePitch != null ? TapePitch.ToDouble() : 4)));
                    AvailablePartsInFeederRow = TotalPartsInFeederRow - CurrentRow.CurrentPartIndex;
                }

                RaisePropertyChanged(nameof(CurrentComponent));
                RaisePropertyChanged(nameof(TapePitch));
                RaisePropertyChanged(nameof(CurrentComponentPackage));
                RaiseCanExecuteChanged();
            }
        }

        private Point3D<double> _currentPartLocation;
        public override Point3D<double> CurrentPartLocation { get => _currentPartLocation; }


        public ObservableCollection<MachineStagingPlate> StagingPlates { get => MachineRepo.CurrentMachine.Settings.StagingPlates; }

        public ObservableCollection<StripFeeder> Feeders { get => _stripFeeders; }

        public RelayCommand SetFeederOriginCommand { get; }
        public RelayCommand GoToFeederOriginCommand { get; }
        public RelayCommand GoToFeederReferenceHoleCommand { get; }


        public RelayCommand SetFirstFeederReferenceHoleCommand { get; }
        public RelayCommand SetLastFeederReferenceHoleCommand { get; }

        public RelayCommand GoToFirstFeederReferenceHoleCommand { get; }
        public RelayCommand GoToLastFeederReferenceHoleCommand { get; }


        public RelayCommand GoToFirstPartCommand { get; }
        public RelayCommand GoToLastPartCommand { get; }
        public RelayCommand GoToCurrentPartCommand { get; }
        public RelayCommand GoToNextPartCommand { get; }
        public RelayCommand GoToPreviousPartCommand { get; }

        public RelayCommand RefreshTemplatesCommand { get; }

        public RelayCommand AddCommand { get;  }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RelayCommand DoneRowCommand { get; }
        public RelayCommand CancelRowCommand { get; }
    }
}
