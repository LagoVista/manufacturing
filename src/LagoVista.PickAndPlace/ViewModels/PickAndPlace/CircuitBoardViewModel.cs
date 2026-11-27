// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ce182bdf13e6fe6d7067ee796ac1e01c34de101ad33adf2a1c468a0653af9045
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Client.Core.Exceptions;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using LagoVista.PickAndPlace.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class CircuitBoardViewModel : MachineViewModelBase, ICircuitBoardViewModel
    {
        IJobManagementViewModel _jobManager;
        ILocatorViewModel _locatorViewModel;

        public CircuitBoardViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel, IJobManagementViewModel jobManager) : base(machineRepo)
        {
            _jobManager = jobManager ?? throw new ArgumentNullException(nameof(jobManager));
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            Job = _job;
            _jobManager.PropertyChanged += _jobManager_PropertyChanged;

            GoToExpectedFiducialCommand = CreatedMachineConnectedCommand(GoToExpectedFiducial, () => SelectedFiducial != null && _alignment == null && !_tuningExposure);
            GoToActualFiducialCommand = CreatedMachineConnectedCommand(GoToActualFiducial, () => SelectedFiducial != null && SelectedFiducial.AbsoluteActual != null && IsBoardAligned && _alignment == null && !_tuningExposure);
            AlignCommand = CreatedMachineConnectedCommand(async () => await AlignBoardAsync(), () => Job != null && Job.BoardFiducials?.Count >= 2 && _alignment == null && !_tuningExposure);
            AlignFiducialCommand = CreatedMachineConnectedCommand(async () => await RunAlignmentAsync(false), () => SelectedFiducial != null && _alignment == null && !_tuningExposure);
            CancelAlignmentCommand = CreateCommand(() => _alignment?.Cancel(), () => _alignment != null);
            TuneBoardExposureCommand = CreatedMachineConnectedCommand(async () => await TuneBoardExposureAsync(),
                () => !_tuningExposure && _alignment == null);
        }

        private CancellationTokenSource _alignment;
        private bool _tuningExposure;
        private IMachine _watchedAlignmentMachine;
        private BoardRigidTransform _transform;
        private PickAndPlaceJob _alignedJob;
        private IMachine _alignedMachine;
        private Point2D<double> _alignedOrigin;
        private Point2D<double>[] _alignedExpected;
        private object _alignedRevision;

        public void GoToExpectedFiducial() => NavigateFiducial(false);
        public void GoToActualFiducial() => NavigateFiducial(true);

        private async void NavigateFiducial(bool actual)
        {
            if (_alignment != null || _tuningExposure || SelectedFiducial == null) return;
            var operation = new CancellationTokenSource();
            _alignment = operation;
            RaiseCanExecuteChanged();
            var machine = Machine;
            var fid = SelectedFiducial;
            var job = Job;
            var origin = MachineConfiguration?.DefaultWorkOrigin;
            var point = actual ? fid.AbsoluteActual :
                BoardRigidTransform.Valid(origin) && BoardRigidTransform.Valid(fid.Expected)
                    ? origin + fid.Expected : null;
            try
            {
                RequireMachine(machine, job, operation.Token, true);
                if (actual && !IsBoardAligned)
                    throw new InvalidOperationException("No valid current board alignment; use the expected fiducial.");
                RequireTarget(machine, point);
                await ConfirmIdleAsync(machine, job, operation.Token);
                machine.SendSafeMoveHeight();
                await ConfirmIdleAsync(machine, job, operation.Token);
                await machine.MoveToCameraAsync();
                await ConfirmIdleAsync(machine, job, operation.Token);
                if (machine.CurrentMachineToolHead != null) throw new InvalidOperationException("Camera navigation was not established.");
                machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_BoardFiducial);
                operation.Token.ThrowIfCancellationRequested();
                machine.GotoPoint(point);
                await ConfirmIdleAsync(machine, job, operation.Token);
            }
            catch (Exception ex)
            {
                machine?.AddStatusMessage(StatusMessageTypes.Warning, ex.Message);
            }
            finally
            {
                _alignment = null;
                operation.Dispose();
                RaiseCanExecuteChanged();
            }
        }

        private void _jobManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IJobManagementViewModel.Job)) Job = _jobManager.Job;
        }

        private async Task TuneBoardExposureAsync()
        {
            if (_tuningExposure || _alignment != null) return;
            _tuningExposure = true;
            RaiseCanExecuteChanged();
            var machine = Machine;
            try
            {
                RequireMachine(machine, Job, CancellationToken.None, true);
                var capture = machine.PositionImageCaptureService;
                if (machine.Busy || machine.CurrentMachineToolHead != null ||
                    capture?.Profile?.Key != VisionProfile.VisionProfile_BoardFiducial)
                    throw new InvalidOperationException("Go to the expected board fiducial, leave the camera idle, then tune exposure.");
                var result = await capture.AutoTuneExposureAsync();
                if (!result.Successful) throw new InvalidOperationException(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                machine?.AddStatusMessage(StatusMessageTypes.Warning, ex.Message);
            }
            finally
            {
                _tuningExposure = false;
                RaiseCanExecuteChanged();
            }
        }

        public Task<InvokeResult> AlignBoardAsync() => RunAlignmentAsync(true);

        private async Task<InvokeResult> RunAlignmentAsync(bool entireBoard)
        {
            if (_alignment != null || _tuningExposure) return InvokeResult.FromError("Board alignment or exposure tuning is already in progress.");
            var operation = new CancellationTokenSource();
            _alignment = operation;
            var token = operation.Token;
            var machine = Machine;
            var job = Job;
            var sw = Stopwatch.StartNew();
            var selectedBoardProfile = false;
            IsBoardAligned = false;
            RaiseCanExecuteChanged();
            try
            {
                RequireMachine(machine, job, token, true);
                var origin = machine.Settings.DefaultWorkOrigin;
                if (!BoardRigidTransform.Valid(origin)) throw new InvalidOperationException("Set a valid coarse board origin.");
                origin = new Point2D<double>(origin.X, origin.Y);
                var revision = job.BoardRevision;
                var fiducials = entireBoard ? job.BoardFiducials?.ToArray() :
                    SelectedFiducial == null ? null : new[] { SelectedFiducial };
                if (fiducials == null || fiducials.Length < (entireBoard ? 2 : 1))
                    throw new InvalidOperationException("Select at least two distinct board fiducials for alignment.");
                var expected = fiducials.Select(f => BoardRigidTransform.Valid(f.Expected)
                    ? new Point2D<double>(f.Expected.X, f.Expected.Y) : null).ToArray();
                foreach (var point in expected)
                {
                    if (!BoardRigidTransform.Valid(point)) throw new InvalidOperationException("Invalid expected fiducial.");
                    RequireTarget(machine, origin + point);
                }
                var capture = machine.PositionImageCaptureService;
                if (capture?.Camera?.CameraType?.Value != CameraTypes.Position)
                    throw new InvalidOperationException("Position camera is unavailable.");
                await ConfirmIdleAsync(machine, job, token);
                selectedBoardProfile = true;
                machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_BoardFiducial);
                var ready = await capture.EnsureCaptureReadyAsync();
                if (!ready.Successful) throw new InvalidOperationException(ready.ErrorMessage);
                var profile = capture.Profile;
                var scale = capture.Camera.EffectivePixelsPerMM;
                if (profile?.Key != VisionProfile.VisionProfile_BoardFiducial ||
                    !profile.PerformShapeDetection || !profile.FindCircles ||
                    !BoardRigidTransform.Finite(scale) || scale <= 0 ||
                    !BoardRigidTransform.Finite(profile.ErrorToleranceMM) || profile.ErrorToleranceMM <= 0 ||
                    !BoardRigidTransform.Finite(profile.TargetImageRadius) || profile.TargetImageRadius <= 0 ||
                    !BoardRigidTransform.Finite(profile.HoughCirclesMinRadius) ||
                    !BoardRigidTransform.Finite(profile.HoughCirclesMaxRadius) ||
                    profile.HoughCirclesMinRadius <= 0 || profile.HoughCirclesMaxRadius <= profile.HoughCirclesMinRadius ||
                    profile.StabilizationCount < 2)
                    throw new InvalidOperationException("Board Fiducial profile needs calibrated scale, valid circle geometry, stabilization and centering tolerance.");
                var tolerance = profile.ErrorToleranceMM;
                var searchRadius = profile.TargetImageRadius;
                var minRadius = profile.HoughCirclesMinRadius;
                var maxRadius = profile.HoughCirclesMaxRadius;
                var count = profile.StabilizationCount;
                var auto = profile.AutoThreshold;
                var exposure = capture.Camera.EffectiveExposure;
                var threshold = profile.PrimaryThreshold;
                var invert = profile.Invert;
                var camera = capture.Camera;
                void Check()
                {
                    RequireMachine(machine, job, token, false);
                    if (!ReferenceEquals(job.BoardRevision, revision) ||
                        !BoardRigidTransform.Valid(machine.Settings.DefaultWorkOrigin) ||
                        BoardRigidTransform.Distance(origin, machine.Settings.DefaultWorkOrigin) > 0.000001 ||
                        !ReferenceEquals(machine.PositionImageCaptureService, capture) ||
                        !ReferenceEquals(capture.Profile, profile) || !ReferenceEquals(capture.Camera, camera) || !capture.IsCapturing ||
                        Math.Abs(capture.Camera.EffectivePixelsPerMM - scale) > 0.0001 ||
                        profile.ErrorToleranceMM != tolerance || profile.TargetImageRadius != searchRadius ||
                        profile.HoughCirclesMinRadius != minRadius || profile.HoughCirclesMaxRadius != maxRadius ||
                        profile.StabilizationCount != count || profile.AutoThreshold != auto ||
                        profile.PrimaryThreshold != threshold || profile.Invert != invert ||
                        capture.Camera.EffectiveExposure != exposure ||
                        (entireBoard && !job.BoardFiducials.SequenceEqual(fiducials)) ||
                        fiducials.Where((f, i) => !BoardRigidTransform.Valid(f.Expected) ||
                            BoardRigidTransform.Distance(f.Expected, expected[i]) > 0.000001).Any())
                        throw new InvalidOperationException("Board, origin, camera or vision settings changed; restart alignment.");
                }
                Check();
                if (entireBoard)
                {
                    // Preflight geometry before any physical movement.
                    if (!BoardRigidTransform.TryFit(expected, expected, tolerance, out _, out var geometry))
                        throw new InvalidOperationException(geometry);
                }
                await ConfirmIdleAsync(machine, job, token);
                Check();
                machine.SendSafeMoveHeight();
                await ConfirmIdleAsync(machine, job, token);
                Check();
                await machine.MoveToCameraAsync();
                await ConfirmIdleAsync(machine, job, token);
                Check();
                if (machine.CurrentMachineToolHead != null)
                    throw new InvalidOperationException("Could not establish camera navigation.");
                var observed = new List<Point2D<double>>();
                for (int i = 0; i < fiducials.Length; i++)
                {
                    Check();
                    SelectedFiducial = fiducials[i];
                    var approach = origin + expected[i];
                    machine.AddStatusMessage(StatusMessageTypes.Info,
                        $"Board fiducial {i + 1}/{fiducials.Length}: {fiducials[i].Name}; scale {scale:F3} px/mm.");
                    machine.GotoPoint(approach);
                    var settled = await ConfirmIdleAsync(machine, job, token);
                    Point2D<double> measured = null;
                    for (int attempt = 0; attempt <= FeederAutoCornerJogMath.MaxAutomaticJogs; attempt++)
                    {
                        Check();
                        if (machine.CurrentMachineToolHead != null || machine.Busy)
                            throw new InvalidOperationException("Camera navigation or idle state changed.");
                        var observation = await ObserveAsync(capture, profile, scale, settled, token);
                        Check();
                        if (observation == null) throw new InvalidOperationException("No fresh unique stable board circle; check exposure, threshold and radius settings.");
                        if (machine.Busy || machine.IsLocating || _locatorViewModel.IsLocating)
                            throw new InvalidOperationException("Machine or locator state changed after observation.");
                        var position = await machine.GetCurrentLocationAsync(5000);
                        Check();
                        if (!position.Successful || !BoardRigidTransform.Valid(position.Result) || machine.Busy ||
                            machine.IsLocating || _locatorViewModel.IsLocating || machine.CurrentMachineToolHead != null)
                            throw new InvalidOperationException("Could not read a fresh idle camera position.");
                        if (BoardRigidTransform.Distance(position.Result, approach) > searchRadius ||
                            observation.RadiusMm < minRadius || observation.RadiusMm > maxRadius)
                            throw new InvalidOperationException("Fiducial observation is outside the configured acquisition geometry.");
                        var dx = observation.OffsetXpx / scale;
                        var dy = -observation.OffsetYpx / scale;
                        if (!FeederAutoCornerJogMath.TryTarget(position.Result, dx, dy, machine.Settings.WorkAreaSize, out var target) ||
                            BoardRigidTransform.Distance(target, approach) > searchRadius)
                            throw new InvalidOperationException("Board fiducial correction exceeds the search radius or safe travel.");
                        var radial = Math.Sqrt(dx * dx + dy * dy);
                        machine.AddStatusMessage(StatusMessageTypes.Info,
                            $"Fiducial {fiducials[i].Name}: X={dx:+0.000;-0.000;0.000} Y={dy:+0.000;-0.000;0.000} R={radial:F3} mm, radius={observation.RadiusMm:F3} mm.");
                        if (radial <= tolerance)
                        {
                            // Retain subpixel residual rather than rounding the
                            // observed target to the camera's final jog position.
                            measured = target;
                            break;
                        }
                        if (!FeederAutoCornerJogMath.CanJogAgain(attempt))
                            throw new InvalidOperationException("Fiducial did not center within the bounded correction attempts.");
                        Check();
                        machine.GotoPoint(new Point2D<double>(dx, dy), relativeMove: true);
                        settled = await ConfirmIdleAsync(machine, job, token);
                    }
                    if (measured == null) throw new InvalidOperationException("Fiducial centering failed.");
                    observed.Add(measured);
                }
                Check();
                if (!entireBoard)
                {
                    machine.AddStatusMessage(StatusMessageTypes.Info,
                        $"Single fiducial measured at {observed[0]}; run Align Board to validate and commit all measurements.");
                    return InvokeResult.Success;
                }
                if (!BoardRigidTransform.TryFit(expected, observed, tolerance, out var fit, out var diagnostic))
                    throw new InvalidOperationException(diagnostic);
                Check();
                // Commit the candidate as a unit only after all observations and
                // the rigid-fit checks passed. Failed runs retain old readings
                // for diagnosis, but cannot authorize the current setup.
                for (int i = 0; i < fiducials.Length; i++)
                {
                    fiducials[i].AbsoluteActual = observed[i];
                    fiducials[i].Actual = observed[i] - origin;
                }
                _transform = fit;
                _alignedJob = job;
                _alignedMachine = machine;
                WatchAlignmentMachine(machine);
                _alignedOrigin = origin;
                _alignedExpected = expected;
                _alignedRevision = revision;
                AngleOffset = fit.RotationDegrees;
                ScalingError = new Point2D<double>(0, 0); // legacy diagnostic; no scale is applied
                IsBoardAligned = true;
                machine.AddStatusMessage(StatusMessageTypes.Info, diagnostic);
                machine.DebugWriteLine($"Board alignment completed in {sw.Elapsed.TotalMilliseconds:F0} ms: {diagnostic}");
                return InvokeResult.Success;
            }
            catch (Exception ex)
            {
                IsBoardAligned = false;
                var reason = ex is OperationCanceledException ? "Board alignment cancelled." : ex.Message;
                machine?.AddStatusMessage(StatusMessageTypes.Warning, reason);
                return InvokeResult.FromError(reason);
            }
            finally
            {
                if (selectedBoardProfile && machine?.Connected == true)
                {
                    try { machine.TopLightOn = false; }
                    catch (Exception ex) { machine.AddStatusMessage(StatusMessageTypes.Warning, "Top light OFF command failed: " + ex.Message); }
                }
                _alignment = null;
                operation.Dispose();
                RaiseCanExecuteChanged();
            }
        }

        private void RequireMachine(IMachine machine, PickAndPlaceJob job, CancellationToken token, bool requireNoLocator)
        {
            token.ThrowIfCancellationRequested();
            if (machine == null || !ReferenceEquals(machine, Machine) ||
                !ReferenceEquals(machine, MachineRepo.CurrentMachine) || job == null || !ReferenceEquals(job, Job) ||
                machine.Settings == null || !machine.Connected || !machine.WasMachineHomed ||
                !machine.WasMachinOriginCalibrated || machine.Mode != OperatingMode.Manual ||
                !BoardRigidTransform.Valid(machine.Settings.MachineFiducial) ||
                machine.Settings.MachineFiducial.IsOrigin() ||
                machine.LeftVacuumPump || machine.RightVacuumPump ||
                (requireNoLocator && (machine.IsLocating || _locatorViewModel.IsLocating)))
                throw new InvalidOperationException("Board calibration requires the current job, connected homed origin-calibrated Manual machine, configured machine fiducial, vacuum off, and no other locator.");
        }

        private static void RequireTarget(IMachine machine, Point2D<double> point)
        {
            if (!BoardRigidTransform.InWorkArea(point, machine.Settings.WorkAreaSize))
                throw new InvalidOperationException("Fiducial target is outside the machine work area.");
        }

        private async Task<DateTimeOffset> ConfirmIdleAsync(IMachine machine, PickAndPlaceJob job, CancellationToken token)
        {
            RequireMachine(machine, job, token, true);
            var result = await machine.SpinUntilIdleAsync(10000);
            if (!result.Successful) throw new InvalidOperationException("Motion not acknowledged: " + result.ErrorMessage);
            // Controller acknowledgement plus a full second of quiet status
            // prevents mechanical ringing and delayed Busy delivery becoming evidence.
            int quiet = 0;
            for (int i = 0; i < 40; i++)
            {
                RequireMachine(machine, job, token, true);
                quiet = machine.Busy ? 0 : quiet + 1;
                if (quiet >= 11) return DateTimeOffset.UtcNow;
                await Task.Delay(100, token);
            }
            throw new InvalidOperationException("Machine did not settle after motion.");
        }

        private async Task<FiducialCircleObservation> ObserveAsync(IImageCaptureService capture,
            VisionProfile profile, double scale, DateTimeOffset settled, CancellationToken token)
        {
            capture.ResetDetections();
            var handler = new BoardObservationHandler(capture, profile, scale, settled);
            _locatorViewModel.RegisterCircleLocatedHandler(handler, 10000);
            try
            {
                var completed = await Task.WhenAny(handler.Completion.Task, Task.Delay(11000, token));
                token.ThrowIfCancellationRequested();
                return completed == handler.Completion.Task ? await handler.Completion.Task : null;
            }
            finally
            {
                _locatorViewModel.UnregisterCircleLocatedHandler(handler);
            }
        }

        private sealed class BoardObservationHandler : ICircleLocatedHandler
        {
            private readonly IImageCaptureService _capture;
            private readonly VisionProfile _profile;
            private readonly double _scale;
            private readonly DateTimeOffset _settled;
            internal readonly TaskCompletionSource<FiducialCircleObservation> Completion =
                new TaskCompletionSource<FiducialCircleObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal BoardObservationHandler(IImageCaptureService capture, VisionProfile profile, double scale, DateTimeOffset settled)
            { _capture = capture; _profile = profile; _scale = scale; _settled = settled; }

            public void CircleLocated(MVLocatedCircle circle)
            {
                if (Completion.Task.IsCompleted || circle == null || !circle.Stabilized || !circle.PositionStable ||
                    !ReferenceEquals(_capture.Profile, _profile)) return;
                var center = circle.CenterPixels;
                var offset = circle.OffsetPixels;
                var snapshot = new FiducialCircleObservation
                {
                    TrackId = circle.Id, CameraType = circle.CameraType, ProfileKey = _profile.Key,
                    ObservedAtUtc = DateTimeOffset.UtcNow, FrameAtUtc = _capture.LastProcessedFrameUtc,
                    PixelsPerMillimeter = circle.PixelsPerMM, FrameWidth = _capture.CaptureFrameSize.Width,
                    FrameHeight = _capture.CaptureFrameSize.Height, Stabilized = circle.Stabilized,
                    PositionStable = circle.PositionStable, Centered = circle.Centered, FoundCount = circle.FoundCount,
                    CenterXpx = center.X, CenterYpx = center.Y, OffsetXpx = offset.X, OffsetYpx = offset.Y,
                    RadiusPx = circle.RadiusPixels, RadiusMm = circle.RadiusPixels / _scale,
                    ErrorToleranceMm = _profile.ErrorToleranceMM
                };
                if (FiducialCalibrationGate.TryQualify(snapshot, VisionProfile.VisionProfile_BoardFiducial,
                    _scale, _settled, DateTimeOffset.UtcNow, out _))
                    Completion.TrySetResult(snapshot);
            }
            public void CircleLocatorTimeout() => Completion.TrySetResult(null);
            public void CircleLocatorAborted() => Completion.TrySetResult(null);
        }

        double _angleOffset;
        public double AngleOffset
        {
            get => _angleOffset;
            private set => Set(ref _angleOffset, value);
        }

        Point2D<double> _scalingError;
        public Point2D<double> ScalingError
        {
            get => _scalingError;
            private set => Set(ref _scalingError, value);
        }

        public Point2D<double> Adjust(Point2D<double> point)
        {
            if (!IsBoardAligned) throw new InvalidOperationException("Please calibrate board first.");
            return _transform.Rotate(point);
        }

        public Point2D<double> GetWorkSpaceLocation(PickAndPlaceJobPlacement placement)
        {
            if (!IsBoardAligned || !BoardRigidTransform.Valid(placement?.PCBLocation))
                throw new InvalidOperationException("Please calibrate the current board first.");
            return _transform.Apply(placement.PCBLocation);
        }

        public Task<InvokeResult> GoToPartOnBoardAsync(PartsGroup part, PickAndPlaceJobPlacement placement)
        {
            if (!IsBoardAligned)
            {
                return Task.FromResult(InvokeResult.FromError("Please calibrate board first"));
            }

            Machine.SendSafeMoveHeight();
            var boardLocation = GetWorkSpaceLocation(placement);
            if(placement.PickErrorOffset != null)
                boardLocation += placement.PickErrorOffset;
            if (!BoardRigidTransform.InWorkArea(boardLocation, MachineConfiguration.WorkAreaSize))
                return Task.FromResult(InvokeResult.FromError("Board location outside work area."));
            Machine.GotoPoint(boardLocation);

            Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartOnBoard);

            return Task.FromResult( InvokeResult.Success);
        }

        public async Task<InvokeResult> InspectPartOnboardAsync(Component component, PickAndPlaceJobPlacement placement)
        {
            if (!IsBoardAligned)
            {
                return InvokeResult.FromError("Please calibrate board first");
            }

            await Machine.MoveToCameraAsync();
            if (!IsBoardAligned) return InvokeResult.FromError("Board alignment changed.");
            var boardLocation = GetWorkSpaceLocation(placement);
            if (!BoardRigidTransform.InWorkArea(boardLocation, MachineConfiguration.WorkAreaSize))
                return InvokeResult.FromError("Board location outside work area.");
            Machine.GotoPoint(boardLocation);
            if (component.PartOnBoardVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfileSource.Component, component.Id, component.PartOnBoardVisionProfile);
            else if (component.ComponentPackage.Value.PartOnBoardVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfileSource.ComponentPackage, component.ComponentPackage.Id, component.ComponentPackage.Value.PartOnBoardVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_PartOnBoard);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> PlacePartOnboardAsync(Component component, PickAndPlaceJobPlacement placement)
        {
            if (!IsBoardAligned)
            {
                return InvokeResult.FromError("Please calibrate board first");
            }
          
            var move = await GoToPartOnBoardAsync(null, placement);
            if (!move.Successful) return move;
            var idle = await Machine.SpinUntilIdleAsync();
            if (!idle.Successful || !IsBoardAligned) return InvokeResult.FromError("Board move not verified.");
            placement.State = EntityHeader<PnPStates>.Create(PnPStates.AtPlaceLocation);

            Machine.SetToolHeadHeight(MachineConfiguration.WorkOriginZ + component.ComponentPackage.Value.Height);
            await Machine.SpinUntilIdleAsync();
            placement.State = EntityHeader<PnPStates>.Create(PnPStates.OnBoard);
            Machine.VacuumPump = false;
            Machine.Dwell(250);            
            Machine.SendSafeMoveHeight();
            await Machine.SpinUntilIdleAsync();
            placement.State = EntityHeader<PnPStates>.Create(PnPStates.Placed);

            return InvokeResult.Success;
        }

        public Task<InvokeResult> PickPartFromBoardAsync(Component component, PickAndPlaceJobPlacement placement)
        {
            if (!IsBoardAligned)
            {
                return Task.FromResult( InvokeResult.FromError("Please calibrate board first"));
            }

            throw new NotImplementedException();
        }

        PickAndPlaceJob _job;
        public PickAndPlaceJob Job
        {
            get => _job;
            set
            {
                if (!ReferenceEquals(_job, value))
                {
                    _alignment?.Cancel();
                    IsBoardAligned = false;
                }
                Set(ref _job, value);
                RaiseCanExecuteChanged();
            }
        }

        BoardFiducial _selectedFiducial;
        public BoardFiducial SelectedFiducial
        {
            get => _selectedFiducial;
            set
            {
                Set(ref _selectedFiducial, value);
                RaiseCanExecuteChanged();
            }
        }

        bool _isBoardAligned;
        public bool IsBoardAligned
        {
            get => _isBoardAligned && _transform != null && ReferenceEquals(Job, _alignedJob) &&
                ReferenceEquals(Machine, _alignedMachine) && Machine.Connected && Machine.WasMachineHomed &&
                Machine.WasMachinOriginCalibrated && ReferenceEquals(Job.BoardRevision, _alignedRevision) &&
                BoardRigidTransform.Valid(MachineConfiguration?.DefaultWorkOrigin) &&
                BoardRigidTransform.Distance(_alignedOrigin, MachineConfiguration.DefaultWorkOrigin) <= 0.000001 &&
                Job.BoardFiducials != null && _alignedExpected != null && Job.BoardFiducials.Count == _alignedExpected.Length &&
                Job.BoardFiducials.Select((f, i) => BoardRigidTransform.Valid(f.Expected) &&
                    BoardRigidTransform.Distance(f.Expected, _alignedExpected[i]) <= 0.000001).All(v => v);
            private set => Set(ref _isBoardAligned, value);
        }

        protected override void OnMachineDisconnected()
        {
            _alignment?.Cancel();
            IsBoardAligned = false;
        }

        protected override void MachineChanged(IMachine machine)
        {
            _alignment?.Cancel();
            IsBoardAligned = false;
            WatchAlignmentMachine(machine);
        }

        private void WatchAlignmentMachine(IMachine machine)
        {
            if (ReferenceEquals(_watchedAlignmentMachine, machine)) return;
            if (_watchedAlignmentMachine != null) _watchedAlignmentMachine.PropertyChanged -= AlignmentMachineChanged;
            _watchedAlignmentMachine = machine;
            if (machine != null) machine.PropertyChanged += AlignmentMachineChanged;
        }

        private void AlignmentMachineChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(sender, _watchedAlignmentMachine)) return;
            if (e.PropertyName == nameof(IMachine.WasMachineHomed) ||
                e.PropertyName == nameof(IMachine.WasMachinOriginCalibrated) ||
                e.PropertyName == nameof(IMachine.Connected))
            {
                _alignment?.Cancel();
                IsBoardAligned = false;
            }
        }

        public RelayCommand CancelAlignmentCommand { get; }
        public RelayCommand TuneBoardExposureCommand { get; }
        public RelayCommand AlignCommand { get; }

        public RelayCommand GoToExpectedFiducialCommand { get; }
        public RelayCommand GoToActualFiducialCommand { get; }

        public RelayCommand AlignFiducialCommand { get; }
    }
}
