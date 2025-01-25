using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using System;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class ToolAlignmentViewModel : ViewModelBase, IToolAlignmentViewModel
    {
        private readonly ILogger _logger;
        private readonly IMachineRepo _machineRepo;
        private readonly ILocatorViewModel _locatorViewModel;

        public ToolAlignmentViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel, ILogger logger)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SetToolOneMovePositionCommand = new RelayCommand(SetTool1MovePosition, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);
            SetToolOnePickPositionCommand = new RelayCommand(SetTool1PickPosition, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);
            SetToolOnePlacePositionCommand = new RelayCommand(SetTool1PlacePosition, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);

            SetToolOneLocationCommand = new RelayCommand(SetTool1Location, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.Settings.KnownCalibrationPoint != null && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);
            SetToolTwoLocationCommand = new RelayCommand(SetTool2Location, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.Settings.KnownCalibrationPoint != null && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);

            MarkTool1LocationCommand = new RelayCommand(MarkTool1Location, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);
            GoToMarkTool1LocationCommand = new RelayCommand(GoToMarkTook1Location, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);
            SetTopCameraLocationCommand = new RelayCommand(SetTopCameraLocation, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);
            GoToBottomCameraLocationCommand = new RelayCommand(GotoBottomCameraLocation, () => _machineRepo.CurrentMachine.Connected);
            SetBottomCameraLocationCommand = new RelayCommand(SetBottomCameraLocation, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);
            GoToolOneLocationCommand = new RelayCommand(GoToolOneLocation, () => _machineRepo.CurrentMachine.Connected);

            SetAbsoluteWorkSpaceHomeCommand = new RelayCommand(_machineRepo.CurrentMachine.SetAbsoluteWorkSpaceHome, () => _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera);

            SaveCalibrationCommand = new RelayCommand(SaveCalibration, () => IsDirty);

            _machineRepo.MachineChanged += _machineRepo_MachineChanged;
        }

        private void _machineRepo_MachineChanged(object sender, IMachine e)
        {
            e.MachineConnected += (e, m) => RaiseCanExecuteChanged();
            e.MachineDisconnected += (e,m) => RaiseCanExecuteChanged();
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();
        }

        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                Set(ref _isDirty, value);
                SaveCalibrationCommand.RaiseCanExecuteChanged();
            }
        }


        private void RaiseCanExecuteChanged()
        {
            MarkTool1LocationCommand.RaiseCanExecuteChanged();
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
            SetTopCameraLocationCommand.RaiseCanExecuteChanged();
            SetBottomCameraLocationCommand.RaiseCanExecuteChanged();
            SetToolOneMovePositionCommand.RaiseCanExecuteChanged();
            SetToolOnePickPositionCommand.RaiseCanExecuteChanged();
            SetToolOnePlacePositionCommand.RaiseCanExecuteChanged();
        }


        public void SetTopCameraLocation()
        {
            if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M75");
            }

            TopCameraLocation = new Point2D<double>(_machineRepo.CurrentMachine.MachinePosition.X, _machineRepo.CurrentMachine.MachinePosition.Y);
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
        }

        public void GotoBottomCameraLocation()
        {
            //_machineRepo.CurrentMachine.SendCommand($"G0 Z{_machineRepo.CurrentMachine.Settings.SafMoveHeight} F5000");
            //_machineRepo.CurrentMachine.GotoPoint(_machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.X, _machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.Y, _machineRepo.CurrentMachine.Settings.PartInspectionCamera.FocusHeight, true);
            //_machineRepo.CurrentMachine.ViewType = ViewTypes.Tool;
        }

        public void SetBottomCameraLocation()
        {
            if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M71");
            }

            //_machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition = _machineRepo.CurrentMachine.MachinePosition.ToPoint2D();
            //_machineRepo.CurrentMachine.Settings.PartInspectionCamera.FocusHeight = _machineRepo.CurrentMachine.ToolCommonZ;
            //BottomCameraLocation = new Point3D<double>()
            //{
            //    X = _machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.X,
            //    Y = _machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.Y,
            //    Z = _machineRepo.CurrentMachine.Settings.PartInspectionCamera.FocusHeight
            //};

            IsDirty = true;
        }

        public void SetTool1MovePosition()
        {            
            if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M72");
            }

            IsDirty = true;
        }


        public void SetTool1PickPosition()
        {
//            _machineRepo.CurrentMachine.Settings.ToolPickHeight = _machineRepo.CurrentMachine.ToolCommonZ;

            if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M73");
            }

            IsDirty = true;
        }

        public void SetTool1PlacePosition()
        {
  //          _machineRepo.CurrentMachine.Settings.ToolBoardHeight = _machineRepo.CurrentMachine.ToolCommonZ;

            if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M74");
            }

            IsDirty = true;
        }

        public void GoToMarkTook1Location()
        {
            _machineRepo.CurrentMachine.SendCommand($"G0 Z{_machineRepo.CurrentMachine.Settings.SafMoveHeight} F5000");
            _machineRepo.CurrentMachine.GotoPoint(_machineRepo.CurrentMachine.Settings.KnownCalibrationPoint);
        }

        public void GoToolOneLocation()
        {
            _machineRepo.CurrentMachine.SendCommand($"G0 Z{_machineRepo.CurrentMachine.Settings.SafMoveHeight} F5000");
            _machineRepo.CurrentMachine.GotoPoint(_machineRepo.CurrentMachine.MachinePosition.X - _machineRepo.CurrentMachine.Settings.Tool1Offset.X, _machineRepo.CurrentMachine.MachinePosition.Y - _machineRepo.CurrentMachine.Settings.Tool1Offset.Y);
        }

        public void MarkTool1Location()
        {

            _machineRepo.CurrentMachine.Settings.KnownCalibrationPoint = new Point2D<double>()
            {
                X = Math.Round(_machineRepo.CurrentMachine.MachinePosition.X, 2),
                Y = Math.Round(_machineRepo.CurrentMachine.MachinePosition.Y, 2),
            };

            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
        }

        public void SetTool1Location()
        {
            if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M76");
            }

            _machineRepo.CurrentMachine.Settings.Tool1Offset = new Point2D<double>()
            {
                X = -Math.Round(_machineRepo.CurrentMachine.MachinePosition.X - _machineRepo.CurrentMachine.Settings.KnownCalibrationPoint.X, 2),
                Y = -Math.Round(_machineRepo.CurrentMachine.MachinePosition.Y - _machineRepo.CurrentMachine.Settings.KnownCalibrationPoint.Y, 2),
            };

            IsDirty = true;
        }

        public void SetTool2Location()
        {
            if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M77");
            }

            _machineRepo.CurrentMachine.Settings.Tool2Offset = new Point2D<double>()
            {
                X = -_machineRepo.CurrentMachine.Settings.KnownCalibrationPoint.X - _machineRepo.CurrentMachine.MachinePosition.X,
                Y = -_machineRepo.CurrentMachine.Settings.KnownCalibrationPoint.Y - _machineRepo.CurrentMachine.MachinePosition.Y,
            };

            IsDirty = true;
        }

        public void SaveCalibration()
        {

        }

        public void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDev)
        {
            _machineRepo.CurrentMachine.BoardAlignmentManager.CircleLocated(point);
        }

        public void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            _machineRepo.CurrentMachine.BoardAlignmentManager.CornerLocated(point);
        }

        public RelayCommand SetBottomCameraLocationCommand { get; private set; }

        public RelayCommand GoToBottomCameraLocationCommand { get; private set; }


        public RelayCommand SetToolOneLocationCommand { get; private set; }
        public RelayCommand SetToolTwoLocationCommand { get; private set; }
        public RelayCommand SetTopCameraLocationCommand { get; private set; }

        public RelayCommand SetToolOnePlacePositionCommand { get; private set; }
        public RelayCommand SetToolOneMovePositionCommand { get; private set; }
        public RelayCommand SetToolOnePickPositionCommand { get; private set; }
        public RelayCommand SaveCalibrationCommand { get; private set; }
        public RelayCommand MarkTool1LocationCommand { get; private set; }
        public RelayCommand GoToMarkTool1LocationCommand { get; private set; }
        public RelayCommand GoToolOneLocationCommand { get; private set; }

        public RelayCommand SetAbsoluteWorkSpaceHomeCommand { get; private set; }

        Point2D<double> _topCameraLocation;
        public Point2D<double> TopCameraLocation
        {
            get { return _topCameraLocation; }
            set { Set(ref _topCameraLocation, value); }
        }

        Point3D<double> _bottomCameraLocation;
        public Point3D<double> BottomCameraLocation
        {
            get { return _bottomCameraLocation; }
            set { Set(ref _bottomCameraLocation, value); }
        }
    }
}
