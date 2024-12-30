using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public class ToolAlignmentViewModel : ViewModelBase, IToolAlignmentViewModel
    {
        private IMachine _machine;
        private ILocatorViewModel _locatorViewModel;
        private IVisionProfileManagerViewModel _visionManagerViewModel;

        public ToolAlignmentViewModel(IMachine machine, ILocatorViewModel locatorViewModel, IVisionProfileManagerViewModel visionManagerViewModel) 
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            _visionManagerViewModel = visionManagerViewModel ?? throw new ArgumentNullException(nameof(visionManagerViewModel));

            SetToolOneMovePositionCommand = new RelayCommand(SetTool1MovePosition, () => _machine.Connected && _machine.ViewType == ViewTypes.Camera);
            SetToolOnePickPositionCommand = new RelayCommand(SetTool1PickPosition, () => _machine.Connected && _machine.ViewType == ViewTypes.Camera);
            SetToolOnePlacePositionCommand = new RelayCommand(SetTool1PlacePosition, () => _machine.Connected && _machine.ViewType == ViewTypes.Camera);

            SetToolOneLocationCommand = new RelayCommand(SetTool1Location, () => _machine.Connected && _machine.Settings.KnownCalibrationPoint != null && _machine.ViewType == ViewTypes.Camera);
            SetToolTwoLocationCommand = new RelayCommand(SetTool2Location, () => _machine.Connected && _machine.Settings.KnownCalibrationPoint != null && _machine.ViewType == ViewTypes.Camera);

            MarkTool1LocationCommand = new RelayCommand(MarkTool1Location, () => _machine.Connected && _machine.ViewType == ViewTypes.Camera);
            GoToMarkTool1LocationCommand = new RelayCommand(GoToMarkTook1Location, () => _machine.Connected && _machine.ViewType == ViewTypes.Camera);
            SetTopCameraLocationCommand = new RelayCommand(SetTopCameraLocation, () => _machine.Connected && _machine.ViewType == ViewTypes.Camera);
            GoToBottomCameraLocationCommand = new RelayCommand(GotoBottomCameraLocation, () => _machine.Connected);
            SetBottomCameraLocationCommand = new RelayCommand(SetBottomCameraLocation, () => _machine.Connected && _machine.ViewType == ViewTypes.Camera);
            GoToolOneLocationCommand = new RelayCommand(GoToolOneLocation, () => _machine.Connected);

            SetAbsoluteWorkSpaceHomeCommand = new RelayCommand(_machine.SetAbsoluteWorkSpaceHome, () => _machine.Connected && _machine.ViewType == ViewTypes.Camera); 

            SaveCalibrationCommand = new RelayCommand(SaveCalibration, () => IsDirty);
        }


        public override async Task InitAsync()
        {
            await base.InitAsync();

            if (_machine.Settings.PartInspectionCamera != null &&
                _machine.Settings.PartInspectionCamera.AbsolutePosition != null)
            {
                BottomCameraLocation = new Point3D<double>()
                {
                    X = _machine.Settings.PartInspectionCamera.AbsolutePosition.X,
                    Y = _machine.Settings.PartInspectionCamera.AbsolutePosition.Y,
                    Z = _machine.Settings.PartInspectionCamera.FocusHeight
                };
            }
            //StartCapture();
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
            if (_machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machine.SendCommand("M75");
            }

            TopCameraLocation = new Point2D<double>(_machine.MachinePosition.X, _machine.MachinePosition.Y);
            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
        }

        public void GotoBottomCameraLocation()
        {
            _machine.SendCommand($"G0 Z{_machine.Settings.ToolSafeMoveHeight} F5000");
            _machine.GotoPoint(_machine.Settings.PartInspectionCamera.AbsolutePosition.X, _machine.Settings.PartInspectionCamera.AbsolutePosition.Y, _machine.Settings.PartInspectionCamera.FocusHeight, true);
            _machine.ViewType = ViewTypes.Tool1;
        }

        public void SetBottomCameraLocation()
        {
            if (_machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machine.SendCommand("M71");
            }

            _machine.Settings.PartInspectionCamera.AbsolutePosition = _machine.MachinePosition.ToPoint2D();
            _machine.Settings.PartInspectionCamera.FocusHeight = _machine.Tool0;
            BottomCameraLocation = new Point3D<double>()
            {
                X = _machine.Settings.PartInspectionCamera.AbsolutePosition.X,
                Y = _machine.Settings.PartInspectionCamera.AbsolutePosition.Y,
                Z = _machine.Settings.PartInspectionCamera.FocusHeight
            };

            IsDirty = true;
        }

        public void SetTool1MovePosition()
        {
            _machine.Settings.ToolSafeMoveHeight = _machine.Tool0;

            if (_machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machine.SendCommand("M72");
            }

            IsDirty = true;
        }


        public void SetTool1PickPosition()
        {
            _machine.Settings.ToolPickHeight = _machine.Tool0;

            if (_machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machine.SendCommand("M73");
            }

            IsDirty = true;
        }

        public void SetTool1PlacePosition()
        {
            _machine.Settings.ToolBoardHeight = _machine.Tool0;

            if (_machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machine.SendCommand("M74");
            }

            IsDirty = true;
        }

        public void GoToMarkTook1Location()
        {
            _machine.SendCommand($"G0 Z{_machine.Settings.ToolSafeMoveHeight} F5000");
            _machine.GotoPoint(_machine.Settings.KnownCalibrationPoint);
        }

        public void GoToolOneLocation()
        {
            _machine.SendCommand($"G0 Z{_machine.Settings.ToolSafeMoveHeight} F5000");
            _machine.GotoPoint(_machine.MachinePosition.X - _machine.Settings.Tool1Offset.X, _machine.MachinePosition.Y - _machine.Settings.Tool1Offset.Y);
        }

        public void MarkTool1Location()
        {

            _machine.Settings.KnownCalibrationPoint = new Point2D<double>()
            {
                X = Math.Round(_machine.MachinePosition.X, 2),
                Y = Math.Round(_machine.MachinePosition.Y, 2),
            };

            SetToolOneLocationCommand.RaiseCanExecuteChanged();
            SetToolTwoLocationCommand.RaiseCanExecuteChanged();
        }

        public void SetTool1Location()
        {
            if (_machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machine.SendCommand("M76");
            }

            _machine.Settings.Tool1Offset = new Point2D<double>()
            {
                X = -Math.Round(_machine.MachinePosition.X - _machine.Settings.KnownCalibrationPoint.X, 2),
                Y = -Math.Round(_machine.MachinePosition.Y - _machine.Settings.KnownCalibrationPoint.Y, 2),
            };

            IsDirty = true;
        }

        public void SetTool2Location()
        {
            if (_machine.Settings.MachineType == FirmwareTypes.LagoVista_PnP)
            {
                _machine.SendCommand("M77");
            }

            _machine.Settings.Tool2Offset = new Point2D<double>()
            {
                X = -_machine.Settings.KnownCalibrationPoint.X - _machine.MachinePosition.X,
                Y = -_machine.Settings.KnownCalibrationPoint.Y - _machine.MachinePosition.Y,
            };

            IsDirty = true;
        }

        public void SaveCalibration()
        {
            
        }

        public void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDev)
        {
            _machine.BoardAlignmentManager.CircleLocated(point);
        }

        public void CornerLocated(Point2D<double> point, Point2D<double> stdDev)
        {
            _machine.BoardAlignmentManager.CornerLocated(point);
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
