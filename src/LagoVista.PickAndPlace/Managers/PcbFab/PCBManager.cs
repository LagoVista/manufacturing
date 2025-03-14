using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Managers;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.Managers
{


    public class PCBManager : MachineViewModelBase, IPCBManager
    {
        IGCodeFileManager _gcodeFileManager;


        public PCBManager(IMachineRepo machineRepo, IGCodeFileManager gcodeFileManager) : base(machineRepo) 
        {
            _gcodeFileManager = gcodeFileManager ?? throw new ArgumentNullException(nameof(gcodeFileManager));

            ShowCutoutMillingGCodeCommand = new RelayCommand(GenerateMillingGCode, CanGenerateGCode);
            ShowDrillGCodeCommand = new RelayCommand(GenerateDrillGCode, CanGenerateGCode);
            ShowHoldDownGCodeCommand = new RelayCommand(GenerateHoldDownGCode, CanGenerateGCode);

            ShowTopEtchingGCodeCommand = new RelayCommand(ShowTopEtchingGCode, CanGenerateTopEtchingGCode);
            ShowBottomEtchingGCodeCommand = new RelayCommand(ShowBottomEtchingGCode, CanGenerateBottomEtchingGCode);

            OpenEagleBoardFileCommand = new RelayCommand(OpenEagleBoardFile, CanOpenEagleBoard);
            CloseEagleBoardFileCommand = new RelayCommand(CloseEagleBoardFile, CanCloseEagleBoard);

            OpenEagleBoardFileCommand = new RelayCommand(OpenEagleBoardFile, CanOpenEagleBoard);
            CloseEagleBoardFileCommand = new RelayCommand(CloseEagleBoardFile, CanCloseEagleBoard);

        }

        public bool CanGenerateGCode()
        {
            return HasBoard && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }


        public bool CanOpenEagleBoard()
        {
            return (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanCloseEagleBoard()
        {
            return HasBoard && !HasProject && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }


        public bool CanGenerateTopEtchingGCode()
        {
            return HasTopEtching && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanGenerateBottomEtchingGCode()
        {

            return HasBottomEtching && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }


        public async void GenerateHoldDownGCode()
        {
            var drillIntoUnderlayment = await Popups.ConfirmAsync("Drill Holes In Underlayment?", "Would you also like to drill the holes in the underlayment?  You only need to use this once when setting up a fixture.  After that you should use the holes that were already created.");
            _gcodeFileManager.SetGCode(GCodeEngine.CreateHoldDownGCode(Board, Project, drillIntoUnderlayment));
        }

        public void GenerateMillingGCode()
        {
            _gcodeFileManager.SetGCode(GCodeEngine.CreateCutoutMill(Board, Project));
        }

        public void GenerateDrillGCode()
        {
            _gcodeFileManager.SetGCode(GCodeEngine.CreateDrillGCode(Board, Project));
        }

        public void ShowTopEtchingGCode()
        {
            _gcodeFileManager.OpenFileAsync(Project.TopEtchingFileLocalPath);
            _gcodeFileManager.ApplyOffset(Project.ScrapSides, Project.ScrapTopBottom, 0);
        }

        public void ShowBottomEtchingGCode()
        {
            _gcodeFileManager.OpenFileAsync(Project.BottomEtchingFileLocalPath);
            _gcodeFileManager.ApplyOffset((Project.ScrapSides) + Board.Width, Project.ScrapTopBottom, 0);
        }



        public Point2D<double> GetAdjustedPoint(Point2D<double> point)
        {
            return point.Rotate(-MeasuredOffsetAngle);
        }

        public void EnableFiducialPicker()
        {
            IsSetFiducialMode = true;
        }

        public Task<bool> OpenFileAsync(string path)
        {
            try
            {
                var doc = XDocument.Load(path);

                Board = EagleParser.ReadPCB(doc);

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> OpenProjectAsync(string projectFile)
        {
            try
            {
                //                Project = await PcbMillingProject.OpenAsync(projectFile);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public async void OpenEagleBoardFile()
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!String.IsNullOrEmpty(file))
            {
                if (await OpenFileAsync(file))
                {
                }

            }
        }

        public void CloseEagleBoardFile()
        {

        }

        private PrintedCircuitBoard _board;
        public PrintedCircuitBoard Board
        {
            get { return _board; }
            set
            {
                _board = value;
                if (_board != null && HasProject)
                {
                    DrillRack = LagoVista.PCB.Eagle.Managers.GCodeEngine.GetToolRack(_board, Project);
                }
                else
                {
                    DrillRack = null;
                }

                RaisePropertyChanged(nameof(DrillRack));

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasBoard));
            }
        }

        public bool HasBoard
        {
            get { return _board != null; }
        }


        PcbMillingProject _project;
        public PcbMillingProject Project
        {
            get { return _project; }
            set
            {
                _project = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasProject));
                if (_project != null)
                {
                    //if (_project != null && !String.IsNullOrEmpty(_project.EagleBRDFile))
                    //{
                    //    OpenFileAsync(_project.EagleBRDFile);
                    //}
                }
                else
                {
                    Board = null;
                }
            }
        }

        Point2D<double> _firstFiducial;
        public Point2D<double> FirstFiducial
        {
            get { return _firstFiducial; }
            set { Set(ref _firstFiducial, value); }
        }

        Point2D<double> _secondFiducial;
        public Point2D<double> SecondFiducial
        {
            get { return _secondFiducial; }
            set { Set(ref _secondFiducial, value); }
        }

        private Point2D<double> _measuredOffset;
        public Point2D<double> MeasuredOffset
        {
            get { return _measuredOffset; }
        }

        private double _measuredOffsetAngle;
        public double MeasuredOffsetAngle
        {
            get { return _measuredOffsetAngle; }
        }

        public void SetMeasuredOffset(Point2D<double> offset, double angleDegrees)
        {
            _measuredOffset = offset;
            _measuredOffsetAngle = angleDegrees;
            RaisePropertyChanged(nameof(HasMeasuredOffset));
            RaisePropertyChanged(nameof(MeasuredOffsetAngle));
            RaisePropertyChanged(nameof(MeasuredOffset));
        }

        public void ClearMeasuredOffset()
        {
            _measuredOffset = null;
            _measuredOffsetAngle = 0;

            RaisePropertyChanged(nameof(HasMeasuredOffset));
            RaisePropertyChanged(nameof(MeasuredOffsetAngle));
            RaisePropertyChanged(nameof(MeasuredOffset));
        }


        public bool HasMeasuredOffset
        {
            get { return _measuredOffset != null; }
        }



        public List<DrillRackInfo> DrillRack
        {
            get; private set;
        }

        public bool HasProject
        {
            get { return _project != null; }
        }

        public bool HasTopEtching
        {
            get { return _project != null && !EntityHeader.IsNullOrEmpty(_project.TopEtchingFile); }
        }

        public bool HasBottomEtching
        {
            get { return _project != null && !EntityHeader.IsNullOrEmpty(_project.BottomEtchingFile); }
        }

        public String ProjectFilePath
        {
            get; set;
        }

        private bool _issNavigationMode = true;
        public bool IsNavigationMode
        {
            get { return _issNavigationMode; }
            set
            {
                _isSetFiducialMode = !value;
                Set(ref _issNavigationMode, value);
                RaisePropertyChanged(nameof(IsSetFiducialMode));
            }
        }

        private bool _isSetFiducialMode = false;
        public bool IsSetFiducialMode
        {
            get { return _isSetFiducialMode; }
            set
            {
                _issNavigationMode = !value;
                Set(ref _isSetFiducialMode, value);
                RaisePropertyChanged(nameof(IsNavigationMode));
            }
        }

        private bool _cameraNavigation = true;
        public bool CameraNavigation
        {
            get { return _cameraNavigation; }
            set
            {
                _tool1Navigation = !value;
                _tool2Navigation = false;
                RaisePropertyChanged(nameof(Tool1Navigation));
                RaisePropertyChanged(nameof(Tool2Navigation));
                Set(ref _cameraNavigation, value);

                var currentPoint = new Point2D<double>()
                {
                    // X = Machine.NormalizedPosition.X,
                    // Y = Machine.NormalizedPosition.Y
                };

                Machine.GotoPoint(currentPoint);
            }
        }

        private bool _tool1Navigation = false;
        public bool Tool1Navigation
        {
            get { return _tool1Navigation; }
            set
            {
                _cameraNavigation = !value;
                _tool2Navigation = false;
                RaisePropertyChanged(nameof(CameraNavigation));
                RaisePropertyChanged(nameof(Tool2Navigation));
                Set(ref _tool1Navigation, value);

                //if (Machine.Settings.PositioningCamera != null &&
                //   Machine.Settings.PositioningCamera.Tool1Offset != null)
                //{
                //    var currentPoint = new Point2D<double>()
                //    {
                //       // X = Machine.NormalizedPosition.X - Machine.Settings.PositioningCamera.Tool1Offset.X,
                //       // Y = Machine.NormalizedPosition.Y - Machine.Settings.PositioningCamera.Tool1Offset.Y
                //    };

                //    Machine.GotoPoint(currentPoint);
                //}
            }
        }

        private bool _tool2Navigation = false;
        public bool Tool2Navigation
        {
            get { return _tool2Navigation; }
            set
            {
                _cameraNavigation = !value;
                _tool1Navigation = false;
                RaisePropertyChanged(nameof(CameraNavigation));
                RaisePropertyChanged(nameof(Tool1Navigation));
                Set(ref _tool2Navigation, value);


                //if (Machine.Settings.PositioningCamera != null &&
                //    Machine.Settings.PositioningCamera.Tool1Offset != null)
                //{
                //    var currentPoint = new Point2D<double>()
                //    {
                //   //     X = Machine.NormalizedPosition.X - Machine.Settings.PositioningCamera.Tool1Offset.X,
                //     //   Y = Machine.NormalizedPosition.Y - Machine.Settings.PositioningCamera.Tool1Offset.Y
                //    };

                //    Machine.GotoPoint(currentPoint);
                //}
            }
        }

        
        public RelayCommand EnabledFiducialPickerCommand { get; private set; }

        public RelayCommand OpenEagleBoardFileCommand { get; private set; }
        public RelayCommand CloseEagleBoardFileCommand { get; private set; }

        public RelayCommand ShowTopEtchingGCodeCommand { get; private set; }
        public RelayCommand ShowBottomEtchingGCodeCommand { get; private set; }
        public RelayCommand ShowCutoutMillingGCodeCommand { get; private set; }
        public RelayCommand ShowDrillGCodeCommand { get; private set; }
        public RelayCommand ShowHoldDownGCodeCommand { get; private set; }

    }
}
