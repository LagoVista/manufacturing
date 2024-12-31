using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using System;
using System.Threading;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Managers;
using LagoVista.Manufacturing.Models;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Managers.PcbFab;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public class WorkAlignmentViewModel : ViewModelBase
    {
        IBoardAlignmentPositionManager _positionManager;

        enum BoardAlignmentState
        {
            Idle,
            FindingFiducialOne,
            MovingToSecondFiducial,
            FindingFiducialTwo,
            MovingToOrigin,
        }

        Timer _timer;

        BoardAlignmentState _boardAlignmentState = BoardAlignmentState.Idle;

        DateTime _lastActivity;
        Machine _machine;

        public WorkAlignmentViewModel(IMachine _machine)
        {
            AlignBoardCommand = new RelayCommand(AlignBoard, CanAlignBoard);
            CancelBoardAlignmentCommand = new RelayCommand(CancelBoardAlignment, CanCancelBoardAlignment);
            EnabledFiducialPickerCommand = new RelayCommand(() => _machine.PCBManager.IsSetFiducialMode = true);

            _positionManager = new BoardAlignmentPositionManager(_machine.PCBManager);
            _timer = new Timer(Timer_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Timer_Tick(object state)
        {
            switch(_boardAlignmentState)
            {
                case BoardAlignmentState.MovingToOrigin:
                    if(!_machine.Busy)
                    {
                        _machine.SendCommand("G10 L20 P0 X0 Y0");
                        _boardAlignmentState = BoardAlignmentState.Idle;
                        Status = "Completed";
                    }
                    break;
                case BoardAlignmentState.MovingToSecondFiducial:
                case BoardAlignmentState.Idle:
                    break;
                default:
                    if((DateTime.Now - _lastActivity).TotalSeconds > 10)
                    {
                        _boardAlignmentState = BoardAlignmentState.Idle;
                        Status = "Timeout";
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    break;

            }
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            _machine.PropertyChanged += _machine_PropertyChanged;
            _machine.PCBManager.PropertyChanged += PCBManager_PropertyChanged;
        }

        private void PCBManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AlignBoardCommand.RaiseCanExecuteChanged();
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AlignBoardCommand.RaiseCanExecuteChanged();
        }

        public bool CanAlignBoard()
        {
            return _machine.Mode == OperatingMode.Manual &&
                   _machine.PCBManager.HasBoard &&
                   _machine.PCBManager.FirstFiducial != null &&
                   _machine.PCBManager.SecondFiducial != null;
        }

        public bool CanCancelBoardAlignment()
        {
            return _boardAlignmentState != BoardAlignmentState.Idle;
        }

        public void CancelBoardAlignment()
        {
            _boardAlignmentState = BoardAlignmentState.Idle;
            Status = "Idle";
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void AlignBoard()
        {
            _lastActivity = DateTime.Now;

            _boardAlignmentState = BoardAlignmentState.FindingFiducialOne;
            Status = "Centering On First Fiducial";

            _timer.Change(0, 500);
        }

        public  void CircleLocated(Point2D<double> offset, double diameter, Point2D<double> stdDev)
        {
            if (!_machine.Busy)
            {
                _lastActivity = DateTime.Now;

                switch (_boardAlignmentState)
                {
                    case BoardAlignmentState.FindingFiducialOne:
                    case BoardAlignmentState.FindingFiducialTwo:
                        JogToLocation(offset);
                        break;
                    case BoardAlignmentState.MovingToSecondFiducial:
                        Status = "Searching for Second Fiducial";
                        _boardAlignmentState = BoardAlignmentState.FindingFiducialTwo;
                        break;
                }
            }
        }
       
        public  void CircleCentered(Point2D<double> point, double diameter)
        {
            if (!_machine.Busy)
            {
                switch (_boardAlignmentState)
                {
                    case BoardAlignmentState.FindingFiducialOne:
                        _lastActivity = DateTime.Now;
                        Status = "Moving to Second Fiducial";
                        _positionManager.FirstLocated = RequestedPosition;
                        _boardAlignmentState = BoardAlignmentState.MovingToSecondFiducial;

                        var fiducialX = RequestedPosition.X + (_machine.PCBManager.SecondFiducial.X - _machine.PCBManager.FirstFiducial.X);
                        var fiducialY = RequestedPosition.Y + (_machine.PCBManager.SecondFiducial.Y - _machine.PCBManager.FirstFiducial.Y);                        
                        RequestedPosition = new Point2D<double>(fiducialX, fiducialY);                        

                        _machine.GotoPoint(RequestedPosition);

                        break;
                    case BoardAlignmentState.FindingFiducialTwo:
                        _lastActivity = DateTime.Now;
                        _positionManager.SecondLocated = RequestedPosition;
                        _boardAlignmentState = BoardAlignmentState.MovingToOrigin;
                        _machine.PCBManager.SetMeasuredOffset(_positionManager.OffsetPoint, _positionManager.RotationOffset.ToDegrees());
                        _machine.GotoPoint(_positionManager.OffsetPoint);
                        Status = "Returning to Board Origin";
                        break;
                }
            }
        }

        public void JogToLocation(Point2D<double> offset)
        {

        }

        public  void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            _machine.BoardAlignmentManager.CornerLocated(point);
        }

        Point2D<double> _requestedPosition;
        public Point2D<double> RequestedPosition
        {
            get => _requestedPosition;
            set => Set(ref _requestedPosition, value);
        }

        public RelayCommand AlignBoardCommand { get; private set; }

        public RelayCommand CancelBoardAlignmentCommand { get; private set; }

        public RelayCommand EnabledFiducialPickerCommand { get; private set; }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }
        }

        string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }
    }
}
