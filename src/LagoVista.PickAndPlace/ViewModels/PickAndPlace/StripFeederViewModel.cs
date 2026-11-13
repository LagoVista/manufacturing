// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1cb4c400cba65a0eb8c2dd1f9d553473a7903dd479049c9a27fcfe157d79c7a7
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Client.Core;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        FindLastFeederReferenceHole,
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
        private bool _barcodeScanInProgress;
        private string _barcodeScanStatus = "Frame Barcode, then select Scan Barcode.";
        private string _barcodeScanDetails = String.Empty;
        private bool _barcodeScanVerified;

        public string FeederBarcodeScanStatus
        {
            get => _barcodeScanStatus;
            private set => Set(ref _barcodeScanStatus, value);
        }

        public string FeederBarcodeScanDetails
        {
            get => _barcodeScanDetails;
            private set => Set(ref _barcodeScanDetails, value);
        }

        public bool FeederBarcodeScanVerified
        {
            get => _barcodeScanVerified;
            private set => Set(ref _barcodeScanVerified, value);
        }

        private StripFeederCalibrationRunner _feederCalibration;
        private bool _calibrationHistoryWriting;
        private bool _calibrationAutoRunEnabled;
        private bool _calibrationAutoRunAdvancing;
        private MachineOperationRunSnapshot _pendingHistorySnapshot;
        private MachineOperationDefinition _pendingHistoryDefinition;
        private string _calibrationHistoryStatus = "Run history will be recorded as calibration progresses.";

        public string FeederCalibrationHistoryStatus
        {
            get => _calibrationHistoryStatus;
            private set => Set(ref _calibrationHistoryStatus, value);
        }

        private ObservableCollection<FeederCalibrationStepDisplay> _feederCalibrationSteps =
            new ObservableCollection<FeederCalibrationStepDisplay>();

        public ObservableCollection<FeederCalibrationStepDisplay> FeederCalibrationSteps
        {
            get => _feederCalibrationSteps;
            private set => Set(ref _feederCalibrationSteps, value);
        }

        public string FeederCalibrationStatus => _feederCalibration?.StatusText ??
            "Select an existing saved strip feeder and a row to begin guided calibration.";

        public string FeederCalibrationMeasurement => _feederCalibration?.MeasurementText ??
            "No calibration measurement recorded.";

        public string FeederCalibrationActionLabel => _feederCalibration?.NextActionLabel ??
            "Select a feeder and row";

        public bool FeederCalibrationCanContinue =>
            CalibrationSelectionReady && !_calibrationHistoryWriting &&
            !_calibrationAutoRunAdvancing &&
            _feederCalibration?.CanExecuteNext == true;
        public bool FeederCalibrationCanRetry =>
            CalibrationSelectionReady && !_calibrationHistoryWriting &&
            !_calibrationAutoRunAdvancing &&
            _feederCalibration?.CanRetry == true;
        public bool FeederCalibrationCanToggleAutoRun =>
            CalibrationSelectionReady && !_calibrationHistoryWriting &&
            !_calibrationAutoRunAdvancing &&
            (_calibrationAutoRunEnabled || _feederCalibration?.CanAutoAdvance == true);
        public string FeederCalibrationAutoRunLabel =>
            _calibrationAutoRunEnabled ? "Switch to Manual" : "Run Guided Sequence";
        public string FeederCalibrationAutoRunStatus =>
            _calibrationAutoRunAdvancing
                ? "Guided sequence is running. Stay with the machine; Cancel stops future steps."
                : _calibrationAutoRunEnabled
                    ? "Paused for operator: inspect the camera, jog if necessary, and use the primary action to confirm. Guided sequence then resumes."
                    : "Guided mode automatically moves and verifies where safe, but always pauses before saving settings or confirming parts.";
        public bool FeederCalibrationCanCancel => _feederCalibration?.CanCancel == true;

        private bool CalibrationSelectionReady =>
            _isEditing && Current != null && CurrentRow != null &&
            Current.Rows != null && Current.Rows.Contains(CurrentRow) &&
            !_barcodeScanInProgress && Machine != null && Machine.Connected;

        private void OnFeederCalibrationChanged(object sender, EventArgs e)
        {
            if (ReferenceEquals(sender, _feederCalibration))
                RefreshFeederCalibration();
        }

        private void RefreshFeederCalibration()
        {
            var definition = _feederCalibration?.Definition ??
                StripFeederCalibrationDefinition.Create();
            var runSteps = _feederCalibration?.Current?.Steps;
            FeederCalibrationSteps = new ObservableCollection<FeederCalibrationStepDisplay>(
                definition.Steps.Select((step, index) =>
                {
                    var snapshot = runSteps != null ? runSteps[index] : null;
                    return new FeederCalibrationStepDisplay
                    {
                        Number = index + 1,
                        Title = $"{index + 1}. {step.Title}",
                        Status = snapshot?.Status.ToString() ?? "Pending",
                        IsActive = snapshot?.Status == MachineOperationStepStatus.Active,
                        Evidence = snapshot?.Error ??
                            snapshot?.Evidence.LastOrDefault()?.Value ?? String.Empty
                    };
                }));
            RaisePropertyChanged(nameof(FeederCalibrationStatus));
            RaisePropertyChanged(nameof(FeederCalibrationMeasurement));
            RaisePropertyChanged(nameof(FeederCalibrationActionLabel));
            RaisePropertyChanged(nameof(FeederCalibrationCanContinue));
            RaisePropertyChanged(nameof(FeederCalibrationCanRetry));
            RaisePropertyChanged(nameof(FeederCalibrationCanCancel));
            RaisePropertyChanged(nameof(FeederCalibrationHistoryStatus));
            RaisePropertyChanged(nameof(FeederCalibrationAutoRunLabel));
            RaisePropertyChanged(nameof(FeederCalibrationAutoRunStatus));
            RaisePropertyChanged(nameof(FeederCalibrationCanToggleAutoRun));
            RaiseCanExecuteChanged();
        }

        private void ResetFeederCalibration()
        {
            _calibrationAutoRunEnabled = false;
            if (_feederCalibration != null)
            {
                _feederCalibration.StateChanged -= OnFeederCalibrationChanged;
                _feederCalibration.Dispose();
                _feederCalibration = null;
            }

            if (CalibrationSelectionReady && _locatorVM != null)
            {
                var machine = Machine;
                var feeder = Current;
                var row = CurrentRow;
                _feederCalibration = new StripFeederCalibrationRunner(
                    machine, _locatorVM, feeder, row,
                    () => FindLocation(StripFeederLocationTypes.FeederOrigin),
                    () => FindLocation(StripFeederLocationTypes.FirstFeederRowReferenceHole, true),
                    () => TapePitch?.ToDouble() ?? 0,
                    index => ResolvePocketForCalibration(index),
                    item => _restClient.PutAsync("/api/mfg/stripfeeder", item),
                    () => ReferenceEquals(Machine, machine) &&
                          ReferenceEquals(Current, feeder) &&
                          ReferenceEquals(CurrentRow, row));
                _feederCalibration.StateChanged += OnFeederCalibrationChanged;
            }

            RefreshFeederCalibration();
        }

        private async Task RunFeederCalibrationNextAsync()
        {
            var operation = _feederCalibration;
            if (operation == null || _calibrationAutoRunAdvancing)
                return;
            var result = operation.Current.ActiveStepKey == "save-origin"
                ? await operation.ConfirmAsync(System.Threading.CancellationToken.None)
                : await operation.ExecuteNextAsync(System.Threading.CancellationToken.None);
            if (!result.Successful && !String.IsNullOrEmpty(result.ErrorMessage))
                Machine?.AddStatusMessage(StatusMessageTypes.Warning, result.ErrorMessage);
            await SaveFeederCalibrationRunHistoryAsync(operation);
            if (ReferenceEquals(operation, _feederCalibration))
            {
                RefreshFeederCalibration();
                if (result.Successful && _calibrationAutoRunEnabled &&
                    _pendingHistorySnapshot == null)
                    await AdvanceFeederCalibrationAutoRunAsync(operation);
            }
        }

        private async Task ToggleFeederCalibrationAutoRunAsync()
        {
            if (_calibrationAutoRunEnabled)
            {
                // Do not cancel a machine move; stop issuing subsequent steps.
                _calibrationAutoRunEnabled = false;
                RefreshFeederCalibration();
                return;
            }
            var operation = _feederCalibration;
            if (!CalibrationSelectionReady || operation?.CanAutoAdvance != true)
                return;
            _calibrationAutoRunEnabled = true;
            RefreshFeederCalibration();
            await AdvanceFeederCalibrationAutoRunAsync(operation);
        }

        private async Task AdvanceFeederCalibrationAutoRunAsync(StripFeederCalibrationRunner operation)
        {
            if (_calibrationAutoRunAdvancing || !_calibrationAutoRunEnabled)
                return;

            _calibrationAutoRunAdvancing = true;
            RefreshFeederCalibration();
            try
            {
                // At most one pass through the finite seven-step definition.
                // Every move retains the runner's homing, bounds, camera and
                // freshness checks; operator-only approvals are never invoked.
                for (var transitions = 0; transitions < 12 && _calibrationAutoRunEnabled; transitions++)
                {
                    if (!ReferenceEquals(operation, _feederCalibration) ||
                        !CalibrationSelectionReady || _calibrationHistoryWriting ||
                        !operation.CanAutoAdvance || !operation.CanExecuteNext)
                        break;

                    var result = await operation.ExecuteAutoNextAsync(
                        System.Threading.CancellationToken.None);
                    if (!result.Successful)
                    {
                        if (!String.IsNullOrEmpty(result.ErrorMessage))
                            Machine?.AddStatusMessage(StatusMessageTypes.Warning, result.ErrorMessage);
                        break;
                    }

                    await SaveFeederCalibrationRunHistoryAsync(operation);
                    // Never hide a history failure behind additional motion.
                    if (_pendingHistorySnapshot != null)
                        break;
                }

                if (ReferenceEquals(operation, _feederCalibration) &&
                    operation.Current.Status == MachineOperationRunStatus.Completed)
                    _calibrationAutoRunEnabled = false;
            }
            finally
            {
                _calibrationAutoRunAdvancing = false;
                RefreshFeederCalibration();
            }
        }

        private async Task RetryFeederCalibrationAsync()
        {
            var operation = _feederCalibration;
            if (operation == null || _calibrationAutoRunAdvancing)
                return;
            var result = await operation.RetryAsync(System.Threading.CancellationToken.None);
            if (!result.Successful && !String.IsNullOrEmpty(result.ErrorMessage))
                Machine?.AddStatusMessage(StatusMessageTypes.Warning, result.ErrorMessage);
            await SaveFeederCalibrationRunHistoryAsync(operation);
            if (ReferenceEquals(operation, _feederCalibration))
            {
                RefreshFeederCalibration();
                if (result.Successful && _calibrationAutoRunEnabled &&
                    _pendingHistorySnapshot == null)
                    await AdvanceFeederCalibrationAutoRunAsync(operation);
            }
        }

        private async Task CancelFeederCalibrationAsync()
        {
            _calibrationAutoRunEnabled = false;
            var operation = _feederCalibration;
            if (operation == null)
                return;
            await operation.CancelAsync(System.Threading.CancellationToken.None);
            await SaveFeederCalibrationRunHistoryAsync(operation);
            if (ReferenceEquals(operation, _feederCalibration))
                RefreshFeederCalibration();
        }

        // The server stores evidence only; this path NEVER writes a feeder
        // offset or changes the physical operation's Saved indicator.
        private async Task SaveFeederCalibrationRunHistoryAsync(
            StripFeederCalibrationRunner operation)
        {
            if (operation == null || operation.Current.Status == MachineOperationRunStatus.Pending)
                return;

            // Clone the evidence snapshot so a Cancel or Retry cannot mutate
            // the body of an in-flight REST request.
            _pendingHistorySnapshot =
                Newtonsoft.Json.JsonConvert.DeserializeObject<MachineOperationRunSnapshot>(
                    Newtonsoft.Json.JsonConvert.SerializeObject(operation.Current));
            _pendingHistoryDefinition = operation.Definition;
            await RetryFeederCalibrationHistoryAsync();
        }

        private async Task RetryFeederCalibrationHistoryAsync()
        {
            if (_calibrationHistoryWriting || _pendingHistorySnapshot == null ||
                _pendingHistoryDefinition == null)
                return;

            var snapshot = _pendingHistorySnapshot;
            var definition = _pendingHistoryDefinition;
            _calibrationHistoryWriting = true;
            RefreshFeederCalibration();
            try
            {
                var result = await _restClient.PutAsync("/api/mfg/calibration/run",
                    new { Snapshot = snapshot, Definition = definition }, waitCursor: false);
                if (result.Successful)
                {
                    if (ReferenceEquals(_pendingHistorySnapshot, snapshot))
                    {
                        _pendingHistorySnapshot = null;
                        _pendingHistoryDefinition = null;
                    }
                    FeederCalibrationHistoryStatus =
                        $"Run history recorded: {snapshot.RunId} ({snapshot.Status}).";
                }
                else
                {
                    FeederCalibrationHistoryStatus =
                        "Run history was NOT saved. Use Retry History Save. " + result.ErrorMessage;
                    Machine?.AddStatusMessage(StatusMessageTypes.Warning, FeederCalibrationHistoryStatus);
                }
            }
            catch (Exception ex)
            {
                FeederCalibrationHistoryStatus =
                    "Run history was NOT saved. Use Retry History Save. " + ex.Message;
                Machine?.AddStatusMessage(StatusMessageTypes.Warning, FeederCalibrationHistoryStatus);
            }
            finally
            {
                _calibrationHistoryWriting = false;
                RefreshFeederCalibration();
            }
        }

        public StripFeederViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorVM, IRestClient restClient, IMachineUtilitiesViewModel machineUtilitiesViewModel) : base(machineRepo, restClient, locatorVM, machineUtilitiesViewModel)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _locatorVM = locatorVM ?? throw new ArgumentNullException(nameof(locatorVM));

            SetFeederOriginCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(StripFeederSetTypes.FeederOrigin), () => Current != null);
            SetFirstFeederReferenceHoleCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(StripFeederSetTypes.FirstFeederRowReferenceHole), () => CurrentRow != null);
            SetLastFeederReferenceHoleCommand = CreatedMachineConnectedSettingsCommand(() => SetLocation(StripFeederSetTypes.LastFeederRowReferenceHole), () => CurrentRow != null);

            GoToFeederOriginCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FeederOrigin), () => Current != null);
            FrameFeederBarcodeCommand = CreatedMachineConnectedCommand(FrameFeederBarcode,
                () => Current != null && !_barcodeScanInProgress);
            ScanFeederBarcodeCommand = CreatedMachineConnectedCommand(ScanFeederBarcode,
                () => Current != null && !_barcodeScanInProgress);
            GoToFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FeederReferenceHole), () => Current != null);

            GoToFirstFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FirstFeederRowReferenceHole), () => CurrentRow != null);
            GoToLastFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.LastFeederRowReferenceHole), () => CurrentRow != null);
            FindLastFeederReferenceHoleCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FindLastFeederReferenceHole), () => CurrentRow != null);

            GoToFirstPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.FirstPart), () => CurrentRow != null && CurrentPartIndex > 1);
            GoToCurrentPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.CurrentPart), () => CurrentRow != null);
            GoToLastPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.LastPart), () => CurrentRow != null);
            GoToNextPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.NextPart), () => (CurrentRow != null && CurrentPartIndex < TotalPartsInFeederRow));
            GoToPreviousPartCommand = CreatedMachineConnectedCommand(() => GoTo(StripFeederLocationTypes.PreviousPart), () => (CurrentRow != null && CurrentPartIndex > 1));

            FeederCalibrationNextCommand = CreateCommand(async () => await RunFeederCalibrationNextAsync(),
                () => FeederCalibrationCanContinue);
            FeederCalibrationAutoRunCommand = CreateCommand(
                async () => await ToggleFeederCalibrationAutoRunAsync(),
                () => FeederCalibrationCanToggleAutoRun);
            FeederCalibrationRetryCommand = CreateCommand(async () => await RetryFeederCalibrationAsync(),
                () => FeederCalibrationCanRetry);
            FeederCalibrationCancelCommand = CreateCommand(async () => await CancelFeederCalibrationAsync(),
                () => FeederCalibrationCanCancel);
            FeederCalibrationNewRunCommand = CreateCommand(ResetFeederCalibration,
                () => CalibrationSelectionReady && !_calibrationHistoryWriting &&
                    !_calibrationAutoRunAdvancing &&
                    _feederCalibration?.IsBusy != true);
            FeederCalibrationRetryHistoryCommand = CreateCommand(
                async () => await RetryFeederCalibrationHistoryAsync(),
                () => !_calibrationHistoryWriting && _pendingHistorySnapshot != null);

            RemoveCurrentCommand = CreateCommand(RemoveCurrent, () => Current != null);
            

            AddCommand = CreateCommand(Add, () => machineRepo.HasValidMachine && SelectedTemplateId.HasValidId());
            SaveCommand = CreateCommand(async () => await SaveAsync(false), () => Current != null);
            SaveAndCloseCommand = CreateCommand(async () => await SaveAsync(true), () => Current != null);
            CancelCommand = CreateCommand(Cancel, () => Current != null);
            DoneRowCommand = CreateCommand(() => DoneRow(), () => CurrentRow != null);
            CancelRowCommand = CreateCommand(() => CurrentRow = null, () => CurrentRow != null);
            ClearPartCommand = CreateCommand(ClearPart, () => CurrentRow != null && CurrentRow.Component != null);
            SetComponentCommand = CreateCommand(SetComponent, () => CurrentRow != null && CurrentRow.Component == null);

            RefreshTemplatesCommand = CreateCommand(async () => await LoadTemplates());

            ReloadFeederCommand = CreateCommand(ReloadFeeder, () => Current != null);

            SetCurrentPartIndexOnRowCommand = CreateCommand(() => CurrentRow.CurrentPartIndex = CurrentPartIndex, () => CurrentRow != null);
        }

        public async override Task InitAsync()
        {
            await LoadTemplates();
            await base.InitAsync();
        }

        private async Task LoadTemplates()
        {
            var result = await _restClient.GetListResponseAsync<StripFeederTemplateSummary>("/api/mfg/stripfeeder/templates");
            if (result.Successful)
            {
                var feeders = result.Model.ToList();
                feeders.Insert(0, new StripFeederTemplateSummary() { Id = Guid.Empty.ToId(), Name = "-select feeder template-" });
                Templates = new ObservableCollection<StripFeederTemplateSummary>(feeders);
            }

            SelectedTemplateId = StringExtensions.NotSelectedId;
        }

        protected async override void MachineChanged(IMachine machine)
        {
            ResetFeederCalibration();
            await LoadFeeders();
            base.MachineChanged(machine);
            ResetFeederCalibration();
        }

        protected override void OnMachineConnected()
        {
            base.OnMachineConnected();
            ResetFeederCalibration();
        }

        protected override void OnMachineDisconnected()
        {
            ResetFeederCalibration();
            base.OnMachineDisconnected();
        }

        private async void Add()
        {
            var result = await _restClient.GetAsync<DetailResponse<StripFeeder>>($"/api/mfg/stripfeeder/template/{SelectedTemplateId}/factory");
            if (result.Successful)
            {
                Current = result.Result.Model;
                Current.Machine = MachineConfiguration.ToEntityHeader();
                Current.Key = Current.Id.Value.ToLower();
                _isEditing = false;
                ResetFeederCalibration();
            }
        }

        private async void ReloadFeeder()
        {
            if (Current == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Can not reload feeder, no feeder selected.");
            }
            else
            {
                var result = await _restClient.GetAsync<DetailResponse<StripFeeder>>($"/api/mfg/stripfeeder/{Current.Id}");
                if (result.Successful)
                {
                    var existing = Feeders.Remove(Current);
                    Current = result.Result.Model;
                    Feeders.Add(Current);

                }
            }
        }

        private async void ClearPart()
        {
            CurrentRow.Component = null;
            SelectedCategoryKey = StringExtensions.NotSelectedId;
            CurrentComponent = null;
            CurrentComponentPackage = null;
            CurrentRow.CurrentPartIndex = 0;
            CurrentRow.PartCapacity = 0;
            await SaveAsync(false);
        }


        private void Cancel()
        {
            Current = null;
        }

        private void GoTo(StripFeederLocationTypes locationType)
        {
            if (locationType == StripFeederLocationTypes.FeederOrigin &&
                Machine?.PositionImageCaptureService != null)
            {
                Machine.PositionImageCaptureService.FeederBarcodeVertical =
                    Current.Orientation.Value == FeederOrientations.Vertical;
            }

            var result = FindLocation(locationType);
            if (!result.Successful)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, result.ErrorMessage);
                return;
            }

            if (locationType == StripFeederLocationTypes.FeederOrigin)
            {
                // Reuse the existing camera corner detector and the second
                // Play (CenterFoundItem) command. The Feeder Origin profile
                // controls search radius, threshold and camera settings.
                var profile = Machine.PositionImageCaptureService?.Profile;
                if (profile != null && profile.Key == VisionProfile.VisionProfile_FeederOrigin)
                {
                    profile.PerformShapeDetection = true;
                    profile.FindCorners = true;
                    profile.FindCircles = false;
                    profile.FindRectangles = false;
                    profile.FindLines = false;
                    profile.ShowCrossHairs = true;
                }
                Machine.SendSafeMoveHeight();
                Machine.AddStatusMessage(StatusMessageTypes.Info,
                    $"Moving to CALCULATED feeder origin for row {Current.ReferenceHoleRow}, " +
                    $"column {Current.ReferenceHoleColumn} (saved OriginOffset ignored).");
            }

            Machine.GotoPoint(result.Result);
        }

        // This is a camera preview move, not a feeder-origin calibration.
        // Preserve GoToFeederOriginCommand for callers that need the true origin
        // centered in the image; frame the barcode with more of its length visible.
        private void FrameFeederBarcode()
        {
            if (_barcodeScanInProgress)
                return;

            var capture = Machine?.PositionImageCaptureService;
            if (capture?.Camera == null || capture.CaptureFrameSize.Width <= 0 ||
                capture.CaptureFrameSize.Height <= 0)
            {
                Machine?.AddStatusMessage(StatusMessageTypes.Warning,
                    "Start the position camera before framing the feeder barcode.");
                return;
            }

            var feederVertical = Current.Orientation.Value == FeederOrientations.Vertical;
            capture.FeederBarcodeVertical = feederVertical;
            // FindLocation(FeederOrigin) intentionally returns the uncorrected
            // row/column approach, so the white-corner detector can locate the
            // real feeder. Barcode framing is different: after Set Feeder Origin
            // we must start from that calibrated position, including OriginOffset.
            var calculatedOrigin = FindLocation(StripFeederLocationTypes.FeederOrigin);
            if (!calculatedOrigin.Successful)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, calculatedOrigin.ErrorMessage);
                return;
            }
            var calibratedOrigin = ResolveCalibratedFeederOrigin(calculatedOrigin.Result, Current);

            Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_FeederBarcode);
            var pixelsPerMm = capture.Camera.EffectivePixelsPerMM;
            if (pixelsPerMm <= 0 || double.IsNaN(pixelsPerMm) || double.IsInfinity(pixelsPerMm))
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning,
                    "Cannot frame barcode: calibrate position-camera pixels per mm.");
                return;
            }

            // The shape detector uses the actual captured frame, not the Camera.ImageSize
            // configuration (the position camera may capture at a higher resolution).
            var pixels = feederVertical ? capture.CaptureFrameSize.Height : capture.CaptureFrameSize.Width;
            // Leave more room above/left of the expected start bar; the prior
            // 20% framing clipped the marker on the real feeder photograph.
            var shiftMm = pixels * 0.15 / pixelsPerMm;
            // Avoid an unexpected long machine move when the camera size or
            // pixels/mm configuration is wrong. This button is only for framing.
            if (double.IsNaN(shiftMm) || double.IsInfinity(shiftMm) ||
                shiftMm <= 0 || shiftMm > 12)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning,
                    $"Barcode framing shift {shiftMm:F2} mm is outside the expected 0-12 mm range; check camera calibration.");
                return;
            }

            // The calibrated feeder origin is the upper-left WHITE cut corner
            // after Set Feeder Origin. For a vertical label, shift 1 mm right
            // to put the 10-pixel barcode sampling band near the middle of
            // the 2 mm-wide printed strip. Keep the existing along-marker
            // framing shift and horizontal geometry unchanged.
            const double verticalBarcodeCenterOffsetMm = 1.0;
            var framed = feederVertical
                ? new Point2D<double>(calibratedOrigin.X + verticalBarcodeCenterOffsetMm, calibratedOrigin.Y - shiftMm)
                : new Point2D<double>(calibratedOrigin.X + shiftMm, calibratedOrigin.Y);

            Machine.SendSafeMoveHeight();
            Machine.GotoPoint(framed);
            Machine.AddStatusMessage(StatusMessageTypes.Info,
                $"Framing {(feederVertical ? "vertical" : "horizontal")} feeder barcode: " +
                $"camera offset {shiftMm:F2} mm along marker" +
                (feederVertical ? $", +{verticalBarcodeCenterOffsetMm:F2} mm X to barcode center" : "") +
                $"; calibrated origin correction X={Current.OriginOffset.X:+0.00;-0.00;0.00}, " +
                $"Y={Current.OriginOffset.Y:+0.00;-0.00;0.00} mm.");
        }


        // The operator explicitly starts this bounded scan after framing START.
        // No feeder coordinates, identity or calibration settings are persisted.
        private async void ScanFeederBarcode()
        {
            if (_barcodeScanInProgress || Current == null)
                return;

            _barcodeScanInProgress = true;
            FeederBarcodeScanVerified = false;
            FeederBarcodeScanDetails = String.Empty;
            FeederBarcodeScanStatus = "Scanning barcode...";
            var feeder = Current;
            try
            {
                var capture = Machine?.PositionImageCaptureService;
                if (capture?.Camera == null ||
                    capture.Profile?.Key != VisionProfile.VisionProfile_FeederBarcode)
                {
                    FeederBarcodeScanStatus = "Select Frame Barcode with the position camera running first.";
                    Machine?.AddStatusMessage(StatusMessageTypes.Warning, FeederBarcodeScanStatus);
                    return;
                }
                if (Machine.Mode != OperatingMode.Manual)
                {
                    FeederBarcodeScanStatus = "Barcode scan requires Manual machine mode.";
                    Machine.AddStatusMessage(StatusMessageTypes.Warning, FeederBarcodeScanStatus);
                    return;
                }

                var vertical = feeder.Orientation.Value == FeederOrientations.Vertical;
                capture.FeederBarcodeVertical = vertical;
                var scan = new FeederBarcodeScan();
                var totalMovedMm = 0.0;
                const int maxCaptures = 7;
                const double maximumTotalMoveMm = 18.0;

                for (var captureNumber = 1; captureNumber <= maxCaptures; captureNumber++)
                {
                    if (!Machine.Connected || Current != feeder)
                    {
                        FeederBarcodeScanStatus = "Scan stopped: feeder selection or machine connection changed.";
                        break;
                    }

                    var idle = await Machine.SpinUntilIdleAsync(6000);
                    if (!idle.Successful)
                    {
                        FeederBarcodeScanStatus = $"Scan stopped: machine did not become idle ({idle.ErrorMessage}).";
                        break;
                    }

                    // Sample only after motion has finished, allowing the live
                    // camera to settle before requesting a fresh processed frame.
                    await Task.Delay(180);
                    var location = await Machine.GetCurrentLocationAsync(5000);
                    if (!location.Successful)
                    {
                        FeederBarcodeScanStatus = $"Scan stopped: machine position unavailable ({location.ErrorMessage}).";
                        break;
                    }

                    var frame = await capture.CaptureFeederBarcodeSignalAsync();
                    if (!frame.Successful)
                    {
                        FeederBarcodeScanStatus = $"Scan stopped: {frame.ErrorMessage}";
                        break;
                    }

                    // A feeder can be changed or its orientation edited while
                    // waiting for video/motion. Never move based on stale context.
                    if (!Machine.Connected || Current != feeder ||
                        (feeder.Orientation.Value == FeederOrientations.Vertical) != vertical)
                    {
                        FeederBarcodeScanStatus = "Scan stopped: feeder selection, orientation or machine connection changed.";
                        break;
                    }

                    var machineAxisMm = vertical ? -location.Result.Y : location.Result.X;
                    scan.AddCapture(frame.Result, machineAxisMm, capture.Camera.EffectivePixelsPerMM);
                    if (scan.IsComplete)
                    {
                        // Keep the full diagnostic in machine history but show a
                        // readable two-line summary in the constrained editor.
                        FeederBarcodeScanStatus =
                            $"BARCODE VERIFIED: ID {scan.FeederId.Value:X4}  |  11/11 bars  |  CRC-2 OK";
                        FeederBarcodeScanDetails =
                            $"{scan.CaptureCount} captures  |  {scan.LastOverlapCount} matching bars  |  " +
                            $"alignment {scan.LastRegistrationCorrectionMm:+0.00;-0.00;0.00} mm";
                        FeederBarcodeScanVerified = true;
                        Machine.AddStatusMessage(StatusMessageTypes.Info, scan.Status);
                        return;
                    }

                    FeederBarcodeScanStatus = scan.Status;
                    if (!scan.HasVerifiedStart || scan.ObservedBars >= 11 ||
                        scan.Status.StartsWith("OVERLAP", StringComparison.Ordinal))
                        break;

                    if (captureNumber == maxCaptures)
                        break;

                    // Advance a short distance to keep at least two common
                    // bars between frames. The overlap registration checks
                    // this rather than trusting machine mm for deduplication.
                    var frameMm = frame.Result.Length / scan.PixelsPerMmFromStart;
                    var stepMm = Math.Min(3.0, frameMm * 0.25);
                    if (double.IsNaN(stepMm) || double.IsInfinity(stepMm) ||
                        stepMm < 0.5 || totalMovedMm + stepMm > maximumTotalMoveMm)
                    {
                        FeederBarcodeScanStatus = "Scan stopped: next camera move is outside the 18 mm scan limit.";
                        break;
                    }

                    var target = vertical
                        ? new Point2D<double>(location.Result.X, location.Result.Y - stepMm)
                        : new Point2D<double>(location.Result.X + stepMm, location.Result.Y);
                    if (target.X < 0 || target.Y < 0 ||
                        target.X > Machine.Settings.WorkAreaSize.X ||
                        target.Y > Machine.Settings.WorkAreaSize.Y)
                    {
                        FeederBarcodeScanStatus = "Scan stopped: the next position is outside the configured work area.";
                        break;
                    }

                    Machine.SendSafeMoveHeight();
                    Machine.GotoPoint(target);
                    totalMovedMm += stepMm;
                }

                Machine.AddStatusMessage(StatusMessageTypes.Warning, FeederBarcodeScanStatus);
            }
            catch (Exception ex)
            {
                FeederBarcodeScanStatus = $"Barcode scan failed: {ex.Message}";
                Machine?.AddStatusMessage(StatusMessageTypes.Warning, FeederBarcodeScanStatus);
            }
            finally
            {
                _barcodeScanInProgress = false;
                RaiseCanExecuteChanged();
            }
        }

        public async void RemoveCurrent()
        {
            Current.Machine = null;
            var result = await SaveAsync(true);
            if (result.Successful)
                Feeders.Remove(Current);
        }

        public async void SetComponent()
        {
            CurrentRow.Component = EntityHeader<Component>.Create(CurrentComponent);
            await SaveAsync(false);
        }


        private async Task LoadFeeders()
        {
            var stripFeeders = await _restClient.GetListResponseAsync<StripFeeder>($"/api/mfg/machine/{MachineRepo.CurrentMachine.Settings.Id}/stripfeeders?loadcomponents=true");
            if (stripFeeders.Successful)
            {
                _stripFeeders = new ObservableCollection<StripFeeder>(stripFeeders.Model);
                RaisePropertyChanged(nameof(Feeders));
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, stripFeeders.ErrorMessage);
            }
        }

        private async void DoneRow()
        {
            if (CurrentComponent != null)
            {
                CurrentRow.Component = EntityHeader<Component>.Create(CurrentComponent.Id, CurrentComponent.Key, CurrentComponent.Name);
                CurrentRow.Component.Value = CurrentComponent;
                if (CurrentRow.Component.Value.ComponentPackage != null)
                {
                    var result = await _restClient.GetAsync<DetailResponse<ComponentPackage>>($"/api/mfg/component/package/{CurrentRow.Component.Value.ComponentPackage.Id}");
                    CurrentRow.Component.Value.ComponentPackage.Value = result.Result.Model;
                }
            }

            CurrentRow = null;
        }

        public async Task<InvokeResult> SaveAsync(bool close)
        {
            if (Current != null)
            {
                var result = _isEditing ? await _restClient.PutAsync("/api/mfg/stripfeeder", Current) :
                                await _restClient.PostAsync("/api/mfg/stripfeeder", Current);
                if (result.Successful)
                {
                    if (!_isEditing)
                        Feeders.Add(Current);

                    _isEditing = true;
                    if (!close)
                        ResetFeederCalibration();

                    if (close)
                    {
                        Current = null;
                        SelectedTemplateId = StringExtensions.NotSelectedId;
                    }
                }
                else
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
                }

                return result;
            }

            return InvokeResult.FromError("No current feeder");
        }


        // Calculated position from the staging-plate row/column and mounting
        // geometry. Use for the first optical corner approach, not for
        // calibrated pick locations.
        private static Point2D<double> ResolveCalculatedFeederOrigin(Point2D<double> mountingHole,
            StripFeeder feeder)
        {
            var vertical = feeder.Orientation.Value == FeederOrientations.Vertical;
            return mountingHole.SubtractWithConditionalSwap(vertical, feeder.MountingHoleOffset);
        }

        private static Point2D<double> ResolveCalibratedFeederOrigin(Point2D<double> calculatedOrigin,
            StripFeeder feeder)
        {
            return calculatedOrigin + feeder.OriginOffset;
        }

        private static Point2D<double> ResolveFeederOrigin(Point2D<double> mountingHole, StripFeeder feeder)
        {
            return ResolveCalibratedFeederOrigin(
                ResolveCalculatedFeederOrigin(mountingHole, feeder), feeder);
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
            var feederOrigin = ResolveFeederOrigin(stagePlateReferenceLocation, Current);

            switch (setType)
            {
                case StripFeederSetTypes.FeederOrigin:
                    Current.OriginOffset = (Current.OriginOffset + Machine.MachinePosition.ToPoint2D() - feederOrigin).Round(2);
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
            => FindLocation(moveType, false);

        // Guided calibration must approach the computed first tape hole even
        // when an old saved row offset exists. Do not change the manual control
        // or the operator's UseCalculated selection.
        private InvokeResult<Point2D<double>> FindLocation(
            StripFeederLocationTypes moveType, bool forceCalculated)
        {
            if (Current.StagingPlate == null)
            {
                return InvokeResult<Point2D<double>>.FromError("Could not move, staging plate not set on feeder.");
            }

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

            var feederIsVertical = Current.Orientation.Value == FeederOrientations.Vertical;
            if (moveType == StripFeederLocationTypes.FeederReferenceHole)
            {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_StagingPlateHole);
                return InvokeResult<Point2D<double>>.Create(stagePlateReferenceLocation);
            }

            var feederOrigin = ResolveFeederOrigin(stagePlateReferenceLocation, Current);
            if (moveType == StripFeederLocationTypes.FeederOrigin)
            {
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_FeederOrigin);
                // This navigation command intentionally uses only the selected
                // staging-plate row/column and mounting-hole geometry. The
                // measured OriginOffset remains for calibrated picking and
                // SetFeederOrigin, but must NOT bias the initial corner search.
                var calculated = ResolveCalculatedFeederOrigin(stagePlateReferenceLocation, Current);
                return InvokeResult<Point2D<double>>.Create(calculated);
            }

            // rest of methods are looking at a feeder row or part within a row...make sure we have one.
            if (CurrentRow == null)
            {
                return InvokeResult<Point2D<double>>.FromError("Can not move to row, No feeder row selected.");
            }

            var deltaY = Current.BottomLeftRow1Margin.Y + Current.TapeReferenceHoleOffset.Y;
            deltaY += ((CurrentRow.RowIndex - 1) * Current.RowWidth) + (Current.TapeHolesOnTop ? (TapeSize.ToDouble() - 1.75) : 1.75);
            var deltaX = Current.BottomLeftRow1Margin.X + Current.TapeReferenceHoleOffset.X;

            var calculateFirstReferneceHole = feederIsVertical ? new Point2D<double>(deltaY, -deltaX) : new Point2D<double>(deltaX, deltaY);
            var firstReferenceHole = (CurrentRow.FirstTapeHoleOffset.IsOrigin() ? calculateFirstReferneceHole : CurrentRow.FirstTapeHoleOffset).Clone();
            CurrentRow.PartCapacity = TotalPartsInFeederRow;
            switch (moveType)
            {
                case StripFeederLocationTypes.FirstFeederRowReferenceHole:
                    {
                        SetTapeHoleProfile();
                        if (UseCalculated || forceCalculated)
                            return InvokeResult<Point2D<double>>.Create(feederOrigin + calculateFirstReferneceHole);
                        else
                            return InvokeResult<Point2D<double>>.Create(feederOrigin.AddWithConditionalSwap(feederIsVertical, firstReferenceHole));
                    }

                case StripFeederLocationTypes.FindLastFeederReferenceHole:
                    SetTapeHoleProfile();
                    var partCount = Math.Floor(Current.Length / TapePitch.ToDouble());
                    var lastPartX = TapePitch.ToDouble() * (partCount - 1);
                    var calculatedLastPoint = firstReferenceHole.AddToX(lastPartX);
                    var newLastPoint = feederOrigin.AddWithConditionalSwap(feederIsVertical, calculatedLastPoint);
                    return InvokeResult<Point2D<double>>.Create(newLastPoint);

                case StripFeederLocationTypes.LastFeederRowReferenceHole:
                    SetTapeHoleProfile();
                    if (!UseCalculated && !CurrentRow.LastTapeHoleOffset.IsOrigin())
                        return InvokeResult<Point2D<double>>.Create(feederOrigin.AddWithConditionalSwap(feederIsVertical, CurrentRow.LastTapeHoleOffset));

                    var deltaEndX = (TotalPartsInFeederRow - 1) * TapePitch.ToDouble();
                    var lastPoint = (feederIsVertical ? firstReferenceHole.SubtractFromY(deltaX) : firstReferenceHole.AddToX(deltaX));
                    return InvokeResult<Point2D<double>>.Create(feederOrigin + lastPoint);


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

            SetPartInTapeHoleProfile();
            RaiseCanExecuteChanged();
            return ResolvePartInTape(feederOrigin);
        }

        // Read-only pocket geometry: does NOT change CurrentPartIndex or inventory.
        private InvokeResult<Point2D<double>> ResolvePocketForCalibration(int index)
        {
            if (Current == null || CurrentRow == null || Current.StagingPlate == null)
                return InvokeResult<Point2D<double>>.FromError(
                    "Select an existing feeder, staging plate, and row.");
            if (index < 1 || index > TotalPartsInFeederRow)
                return InvokeResult<Point2D<double>>.FromError("Pocket index is outside the feeder capacity.");

            var plate = MachineConfiguration.StagingPlates.SingleOrDefault(
                sp => sp.Id == Current.StagingPlate.Id);
            if (plate == null)
                return InvokeResult<Point2D<double>>.FromError("Configured staging plate not found.");
            var anchor = StagingPlateUtils.ResolveStagePlateWorkSpakeLocation(
                plate, Current.ReferenceHoleColumn, Current.ReferenceHoleRow);
            if (anchor == null)
                return InvokeResult<Point2D<double>>.FromError("Staging-plate reference row/column is missing.");

            SetPartInTapeHoleProfile();
            return ResolvePartInTape(ResolveFeederOrigin(anchor, Current), index);
        }

        private InvokeResult<Point2D<double>> ResolvePartInTape(Point2D<double> feederOrigin,
            int? indexOverride = null)
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

            Machine.DebugWriteLine("----------------------------");



            var index = Math.Max(1, Math.Min(indexOverride ?? CurrentPartIndex,
                TotalPartsInFeederRow));
            if (!indexOverride.HasValue)
                CurrentPartIndex = index;

            Machine.DebugWriteLine($"  RESOLVE LOCATION {index}, FEED LOCATION: {Current.FeedDirection.Value}");

            if (CurrentComponentPackage == null)
                return InvokeResult<Point2D<double>>.FromError("No component package, can not move to part.");

            var pickLocation = LagoVista.Manufacturing.Models.StripFeederPickupCalculator.Calculate(
                feederOrigin,
                CurrentRow.FirstTapeHoleOffset,
                CurrentRow.LastTapeHoleOffset,
                Current.Orientation.Value == FeederOrientations.Vertical,
                Current.TapeHolesOnTop,
                Current.FeedDirection.Value == FeedDirections.Forward,
                index,
                CurrentComponentPackage.CustomTapePitch ?? TapePitch.ToDouble(),
                TapeSize.ToDouble(),
                CurrentComponentPackage.XOffsetFromReferenceHole,
                CurrentComponentPackage.YOffsetFromReferenceHole,
                CurrentComponentPackage.HolesOnBothSideOfTape);

            Machine.DebugWriteLine($"  PICK LOCATION {pickLocation}");
            Machine.DebugWriteLine("----------------------------");


            return InvokeResult<Point2D<double>>.Create(pickLocation);

        }

        private Point2D<double> Extrapolate(Point2D<double> actual, Point2D<double> expected, double ratio, Point2D<double> point)
        {
            var errorX = expected.X - actual.X;
            var errorY = expected.Y - actual.Y;
            var correction = new Point2D<double>(errorX * ratio, errorY * ratio);
            var result = LagoVista.Manufacturing.Models.FeederPositionCorrection.Apply(actual, expected, ratio, point);

            Machine.DebugWriteLine($"  EXTRAPOLATE: Ratio: {ratio} ErrX:{errorX} ErrY:{errorY} Correction: {correction}  Point: {point} Result: {result}");

            return result;
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

        ObservableCollection<StripFeederTemplateSummary> _templates;
        public ObservableCollection<StripFeederTemplateSummary> Templates
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
                _isEditing = true;
                Set(ref _current, value);
                FeederBarcodeScanStatus = "Frame Barcode, then select Scan Barcode.";
                FeederBarcodeScanDetails = String.Empty;
                FeederBarcodeScanVerified = false;
                if (value != null)
                {
                    _tapeSize = _current.TapeSize;
                }
                else
                {
                    _tapeSize = null;
                }

                RaisePropertyChanged(nameof(TapeSize));
                if (Machine?.PositionImageCaptureService != null)
                    Machine.PositionImageCaptureService.FeederBarcodeVertical =
                        value == null ? (bool?)null : value.Orientation.Value == FeederOrientations.Vertical;
                ResetFeederCalibration();
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

                    TotalPartsInFeederRow = Convert.ToInt32(Math.Floor(Current.Length / (TapePitch != null ? TapePitch.ToDouble() : 4)));
                    CurrentRow.PartCapacity = TotalPartsInFeederRow;
                    AvailablePartsInFeederRow = TotalPartsInFeederRow - CurrentRow.CurrentPartIndex;
                }

                RaisePropertyChanged(nameof(CurrentComponent));
                RaisePropertyChanged(nameof(TapePitch));
                RaisePropertyChanged(nameof(CurrentComponentPackage));
                ResetFeederCalibration();
                RaiseCanExecuteChanged();
            }
        }

        private Point2D<double> _currentPartLocation;
        public override Point2D<double> CurrentPartLocation
        {
            get
            {
                var result = FindLocation(StripFeederLocationTypes.CurrentPart);
                if (result.Successful)
                    _currentPartLocation = new Point2D<double>(result.Result.X, result.Result.Y);

                return _currentPartLocation;
            }
        }

        public override double? PickHeight => _current?.PickHeight;

        public override int AvailableParts
        {
            get
            {
                if (CurrentRow == null)
                    return 0;

                return TotalPartsInFeederRow - CurrentRow.CurrentPartIndex;
            }
        }

        public override async Task<InvokeResult> NextPartAsync()
        {
            var nextPartIndex = CurrentRow.CurrentPartIndex + 1;

            var result = await _restClient.PutAsync($"/api/mfg/stripfeeder/{Current.Id}/row/{CurrentRow.RowIndex}/partindex/{nextPartIndex}", new { }, waitCursor: false);

            if (result.Successful) 
            {
                CurrentRow.CurrentPartIndex = nextPartIndex;

                GoTo(StripFeederLocationTypes.CurrentPart);
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }

            return result;
        }

        public ObservableCollection<MachineStagingPlate> StagingPlates { get => MachineRepo.CurrentMachine.Settings.StagingPlates; }

        public ObservableCollection<StripFeeder> Feeders { get => _stripFeeders; }

        public RelayCommand FeederCalibrationNextCommand { get; }
        public RelayCommand FeederCalibrationAutoRunCommand { get; }
        public RelayCommand FeederCalibrationRetryCommand { get; }
        public RelayCommand FeederCalibrationCancelCommand { get; }
        public RelayCommand FeederCalibrationNewRunCommand { get; }
        public RelayCommand FeederCalibrationRetryHistoryCommand { get; }

        public RelayCommand SetFeederOriginCommand { get; }
        public RelayCommand GoToFeederOriginCommand { get; }
        public RelayCommand FrameFeederBarcodeCommand { get; }
        public RelayCommand ScanFeederBarcodeCommand { get; }
        public RelayCommand GoToFeederReferenceHoleCommand { get; }


        public RelayCommand SetFirstFeederReferenceHoleCommand { get; }
        public RelayCommand SetLastFeederReferenceHoleCommand { get; }

        public RelayCommand GoToFirstFeederReferenceHoleCommand { get; }
        public RelayCommand GoToLastFeederReferenceHoleCommand { get; }
        public RelayCommand FindLastFeederReferenceHoleCommand { get; }


        public RelayCommand GoToFirstPartCommand { get; }
        public RelayCommand GoToLastPartCommand { get; }
        public RelayCommand GoToCurrentPartCommand { get; }
        public RelayCommand GoToNextPartCommand { get; }
        public RelayCommand GoToPreviousPartCommand { get; }

        public RelayCommand RefreshTemplatesCommand { get; }

        public RelayCommand AddCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAndCloseCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RelayCommand DoneRowCommand { get; }
        public RelayCommand CancelRowCommand { get; }

        public RelayCommand ReloadFeederCommand { get; }

        public RelayCommand ClearPartCommand { get; }

        public RelayCommand SetCurrentPartIndexOnRowCommand { get; }

        public RelayCommand RemoveCurrentCommand { get; }

        public RelayCommand SetComponentCommand { get;  }
    }
}
