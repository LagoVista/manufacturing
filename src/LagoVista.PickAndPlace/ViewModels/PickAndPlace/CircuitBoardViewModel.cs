using LagoVista.Client.Core.Exceptions;
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class CircuitBoardViewModel : MachineViewModelBase, ICircuitBoardViewModel, ICircleLocatedHandler
    {
        IJobManagementViewModel _jobManager;
        ILocatorViewModel _locatorViewModel;

        public CircuitBoardViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel, IJobManagementViewModel jobManager) : base(machineRepo)
        {
            _jobManager = jobManager ?? throw new ArgumentNullException(nameof(jobManager));
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            Job = _job;
            _jobManager.PropertyChanged += _jobManager_PropertyChanged;

            GoToExpectedFiducialCommand = CreatedMachineConnectedCommand(GoToExpectedFiducial, () => SelectedFiducial != null);
            GoToActualFiducialCommand = CreatedMachineConnectedCommand(GoToActualFiducial, () => SelectedFiducial != null && SelectedFiducial.Actual != null);
            AlignCommand = CreatedMachineConnectedCommand(async () => await AlignBoardAsync(), () => Job != null && Job.BoardFiducials?.Count > 0);
            AlignFiducialCommand = CreatedMachineConnectedCommand(() => AlignFiducial(), () => SelectedFiducial != null);
        }

        public void GoToExpectedFiducial()
        {
            if (SelectedFiducial == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No fiducial selected.");
            }
            else
            {
                Machine.GotoPoint(MachineConfiguration.DefaultWorkOrigin + SelectedFiducial.Expected);
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_BoardFiducial);
            }
        }

        public void GoToActualFiducial()
        {
            if (SelectedFiducial == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No fiducial selected.");
            }
            else
            {
                Machine.GotoPoint(MachineConfiguration.DefaultWorkOrigin + SelectedFiducial.Actual);
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_BoardFiducial);
            }
        }


        ManualResetEventSlim _completed = null;

        public void AlignFiducial()
        {
            if (SelectedFiducial == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No fiducial selected.");
            }
            else
            {
                Machine.GotoPoint(MachineConfiguration.DefaultWorkOrigin + SelectedFiducial.Expected);
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_BoardFiducial);
                _locatorViewModel.RegisterCircleLocatedHandler(this);
            }
        }
      

        private void _jobManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IJobManagementViewModel.Job))
            {
                Job = _jobManager.Job;
            }
        }

        public async Task<InvokeResult> AlignBoardAsync()
        {
            lock(this)
            {
                if(this._completed != null)
                {
                    return InvokeResult.FromError("Alignment in progress, please try again later.");
                }

                _completed = new ManualResetEventSlim();
            }

            Machine.SendSafeMoveHeight();
            await Machine.MoveToCameraAsync();
            Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_BoardFiducial);

            foreach (var fiducial in Job.BoardFiducials)
            {
                fiducial.Actual = null;
            }

            foreach (var fiducial in Job.BoardFiducials)
            {
                try
                {
                    _completed = new ManualResetEventSlim();
                    _completed.Reset();
                    SelectedFiducial = fiducial;
                    await Task.Run(async () =>
                    {
                        Machine.GotoPoint(MachineConfiguration.DefaultWorkOrigin + SelectedFiducial.Expected);
                        // Give it time to move so it's not picking up the previous found circle.
                        await Task.Delay(1000);
                        _locatorViewModel.RegisterCircleLocatedHandler(this);
                        int attemptCount = 0;

                        while (!_completed.IsSet && ++attemptCount < 200)
                            _completed.Wait(10);
                    });
                }
                catch(Exception ex)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Error aligning board {ex.Message}.");
                }
                finally {
                    _completed.Dispose();
                    _completed = null;
                }
            };

            RaiseCanExecuteChanged();

            var expectedDelta = Job.BoardFiducials[1].Expected - Job.BoardFiducials.First().Expected;
            var expectedTheta = Math.Atan(expectedDelta.Y / expectedDelta.X);
            var expectedDegrees = (180 / Math.PI) * expectedTheta;

            if (Job.BoardFiducials[1].Actual != null && Job.BoardFiducials.First().Actual != null)
            {
                var actualDelta = Job.BoardFiducials[1].Actual - Job.BoardFiducials.First().Actual;
                var actualTheta = Math.Atan(actualDelta.Y / actualDelta.X);
                var actualDegrees = (180 / Math.PI) * actualTheta;

                this.AngleOffset = expectedDegrees - actualDegrees;
                this.ScalingError = actualDelta - expectedDelta;

                Machine.AddStatusMessage(StatusMessageTypes.Info, "Board calibrated.");
                Machine.GotoPoint(MachineConfiguration.DefaultWorkOrigin);

                Debug.WriteLine($"Expected: {expectedDegrees}; Actual: {actualDegrees}, Scale Error: {this.ScalingError}");
                IsBoardAligned = true;
                return InvokeResult.Success;
            }
            else
            {
                IsBoardAligned = false;
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not identify all fiducials, board not calibrated.");
                foreach(var fiducial in Job.BoardFiducials)
                {
                    fiducial.Actual = null;
                }
                return InvokeResult.FromError("Could not find all fiducials and calibrate the board.");
            }            
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
            //return point;

            var delta = Job.BoardFiducials[1].Expected - Job.BoardFiducials[0].Expected;
            var xRatio = point.X / delta.X;
            var yRatio = point.Y / delta.Y;
            var correctedY = point.Y + (xRatio * ScalingError.Y);
            var correctedX = point.X + (yRatio * ScalingError.X);

            return new Point2D<double>(correctedX, correctedY);
        }

        public Point2D<double> GetWorkSpaceLocation(PickAndPlaceJobPlacement placement)
        {
            var positionOnBoard = placement.PCBLocation - Job.BoardFiducials.First().Expected;
            var adjustedPosition = Adjust(positionOnBoard);

            return adjustedPosition + Job.BoardFiducials.First().AbsoluteActual;
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
            var boardLocation = GetWorkSpaceLocation(placement);
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
          
            await GoToPartOnBoardAsync(null, placement);
            placement.State = EntityHeader<PnPStates>.Create(PnPStates.AtPlaceLocation);

            Machine.SetToolHeadHeight(MachineConfiguration.WorkOriginZ + component.ComponentPackage.Value.Height);
            placement.State = EntityHeader<PnPStates>.Create(PnPStates.OnBoard);
            Machine.VacuumPump = false;
            Machine.Dwell(250);            
            Machine.SendSafeMoveHeight();
            placement.State = EntityHeader<PnPStates>.Create(PnPStates.Placed);

            return InvokeResult.Success;
        }

        public Task<InvokeResult> PickPartFromBoardAsync(Component component, PickAndPlaceJobPlacement placement)
        {
            if (ScalingError == null)
            {
                return Task.FromResult( InvokeResult.FromError("Please calibrate board first"));
            }

            throw new NotImplementedException();
        }

        public async void CircleLocated(MVLocatedCircle circle)
        {
            if (circle.Stabilized && !Machine.Busy)
            {
                if (circle.Centered)
                {
                    _locatorViewModel.UnregisterCircleLocatedHandler(this);

                    var currentLocation = await Machine.GetCurrentLocationAsync();
                    var selected = SelectedFiducial;
                    DispatcherServices.Invoke(() =>
                    {
                        var message = $"Found fiducial {selected.Name}; Expected: {selected.Expected}; Actual: {selected.Actual}";
                        Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.Info, message);
                        selected.Actual = (currentLocation - MachineConfiguration.DefaultWorkOrigin).Round(3);
                        selected.AbsoluteActual = currentLocation;
                        _completed?.Set();
                    });
                    
                }
                else
                {
                    Machine.GotoPoint(circle.OffsetMM, relativeMove: true);
                }

                circle.Reset();
            }
        }

        public void CircleLocatorTimeout()
        {
            _locatorViewModel.UnregisterCircleLocatedHandler(this);
        }

        public void CircleLocatorAborted()
        {
            _locatorViewModel.UnregisterCircleLocatedHandler(this);
        }

        PickAndPlaceJob _job;
        public PickAndPlaceJob Job
        {
            get => _job;
            set
            {
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
            get => _isBoardAligned;
            set => Set(ref _isBoardAligned, value);
        }

        public RelayCommand AlignCommand { get; }

        public RelayCommand GoToExpectedFiducialCommand { get; }
        public RelayCommand GoToActualFiducialCommand { get; }

        public RelayCommand AlignFiducialCommand { get; }
    }
}
