using LagoVista.Core.Commanding;
using LagoVista.GCode;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Managers;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.GCode;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.GCode
{
    public class GCodeViewModel : MachineViewModelBase, IGCodeViewModel
    {
        IMruManager _mruManager;

        public GCodeViewModel(IMachineRepo machineRepo, IMruManager mruManager) : base(machineRepo)
        {
            _mruManager = mruManager ?? throw new ArgumentNullException(nameof(mruManager));

            OpenHeightMapCommand = new RelayCommand(OpenHeightMapFile, CanPerformFileOperation);
            OpenGCodeCommand = new RelayCommand(OpenGCodeFile, CanPerformFileOperation);
            ClearGCodeCommand = new RelayCommand(CloseFile, CanPerformFileOperation);
            ArcToLineCommand = new RelayCommand(ArcToLine, CanConvertArcToLine);

            SaveHeightMapCommand = new RelayCommand(SaveHeightMap, CanSaveHeightMap);

            ApplyHeightMapCommand = new RelayCommand(ApplyHeightMap, CanApplyHeightMap);
            ClearHeightMapCommand = new RelayCommand(ClearHeightMap, CanClearHeightMap);

            SaveModifiedGCodeCommamnd = new RelayCommand(SaveModifiedGCode, CanSaveModifiedGCode);

            StartProbeHeightMapCommand = new RelayCommand(ProbeHeightMap);

            ShowCutoutMillingGCodeCommand = new RelayCommand(GenerateMillingGCode, CanGenerateGCode);
            ShowDrillGCodeCommand = new RelayCommand(GenerateDrillGCode, CanGenerateGCode);
            ShowHoldDownGCodeCommand = new RelayCommand(GenerateHoldDownGCode, CanGenerateGCode);

            ShowTopEtchingGCodeCommand = new RelayCommand(ShowTopEtchingGCode, CanGenerateTopEtchingGCode);
            ShowBottomEtchingGCodeCommand = new RelayCommand(ShowBottomEtchingGCode, CanGenerateBottomEtchingGCode);

            OpenEagleBoardFileCommand = new RelayCommand(OpenEagleBoardFile, CanOpenEagleBoard);
            CloseEagleBoardFileCommand = new RelayCommand(CloseEagleBoardFile, CanCloseEagleBoard);

            Machine.GCodeFileManager.PropertyChanged += GCodeFileManager_PropertyChanged;
            Machine.PropertyChanged += _machine_PropertyChanged;
            Machine.PCBManager.PropertyChanged += PCBManager_PropertyChanged;
            Machine.HeightMapManager.PropertyChanged += HeightMapManager_PropertyChanged;

            OpenEagleBoardFileCommand = new RelayCommand(OpenEagleBoardFile, CanOpenEagleBoard);
            CloseEagleBoardFileCommand = new RelayCommand(CloseEagleBoardFile, CanCloseEagleBoard);
        }

        private void GCodeFileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.GCodeFileManager.HasValidFile))
            {
                DispatcherServices.Invoke(() =>
                {
                    ApplyHeightMapCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private void HeightMapManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.HeightMapManager.HasHeightMap) ||
               e.PropertyName == nameof(Machine.HeightMapManager.Status) ||
               e.PropertyName == nameof(Machine.HeightMapManager.HeightMapDirty))
            {
                DispatcherServices.Invoke(() =>
                {
                    ApplyHeightMapCommand.RaiseCanExecuteChanged();
                    SaveHeightMapCommand.RaiseCanExecuteChanged();
                    StartProbeHeightMapCommand.RaiseCanExecuteChanged();
                    ClearHeightMapCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private void PCBManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.PCBManager.HasBoard) ||
                e.PropertyName == nameof(Machine.PCBManager.HasProject) ||
                e.PropertyName == nameof(Machine.PCBManager.HasTopEtching) ||
                e.PropertyName == nameof(Machine.PCBManager.HasBottomEtching))
            {
                DispatcherServices.Invoke(() =>
                {
                    ApplyHeightMapCommand.RaiseCanExecuteChanged();
                    ShowHoldDownGCodeCommand.RaiseCanExecuteChanged();
                    ShowDrillGCodeCommand.RaiseCanExecuteChanged();
                    ShowCutoutMillingGCodeCommand.RaiseCanExecuteChanged();
                    ShowTopEtchingGCodeCommand.RaiseCanExecuteChanged();
                    ShowBottomEtchingGCodeCommand.RaiseCanExecuteChanged();
                });
            }
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.Mode))
            {
                DispatcherServices.Invoke(() =>
                {
                    OpenEagleBoardFileCommand.RaiseCanExecuteChanged();
                    CloseEagleBoardFileCommand.RaiseCanExecuteChanged();
                    OpenHeightMapCommand.RaiseCanExecuteChanged();
                    OpenGCodeCommand.RaiseCanExecuteChanged();
                    ClearGCodeCommand.RaiseCanExecuteChanged();
                    ShowBottomEtchingGCodeCommand.RaiseCanExecuteChanged();
                    ShowTopEtchingGCodeCommand.RaiseCanExecuteChanged();
                    ApplyHeightMapCommand.RaiseCanExecuteChanged();
                });
            }

            if (e.PropertyName == nameof(Machine.GCodeFileManager.HasValidFile))
            {
                ArcToLineCommand.RaiseCanExecuteChanged();
                ApplyHeightMapCommand.RaiseCanExecuteChanged();
            }
        }


        public bool CanOpenEagleBoard()
        {
            return (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanCloseEagleBoard()
        {
            return Machine.PCBManager.HasBoard && !Machine.PCBManager.HasProject && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }


        public bool CanGenerateGCode()
        {
            return Machine.PCBManager.HasBoard && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanGenerateTopEtchingGCode()
        {
            return Machine.PCBManager.HasTopEtching && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanGenerateBottomEtchingGCode()
        {

            return Machine.PCBManager.HasBottomEtching && (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanSetPositionMode()
        {
            return Machine.Mode == OperatingMode.Manual && Machine.Connected;
        }

        public bool CanConvertArcToLine()
        {
            return Machine.GCodeFileManager.HasValidFile;
        }

        public bool CanChangeUnits()
        {
            return Machine.Mode == OperatingMode.Manual && Machine.Connected;
        }

        public bool CanApplyHeightMap()
        {
            return Machine.GCodeFileManager.HasValidFile &&
                   Machine.HeightMapManager.HasHeightMap &&
                  Machine.HeightMapManager.HeightMap.Status == Models.HeightMapStatus.Populated;
        }

        public bool CanSaveModifiedGCode()
        {
            return Machine.GCodeFileManager.HasValidFile &&
                   Machine.GCodeFileManager.IsDirty;
        }

        public bool CanSaveHeightMap()
        {
            return Machine.HeightMapManager.HasHeightMap &&
                  Machine.HeightMapManager.HeightMap.Status == Models.HeightMapStatus.Populated;
        }

        private bool CanPerformFileOperation(Object instance)
        {
            return (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }

        public bool CanClearHeightMap()
        {
            return Machine.HeightMapManager.HasHeightMap &&
                  Machine.HeightMapManager.HeightMap.Status == Models.HeightMapStatus.Populated;
        }


        private void ButtonArcPlane_Click()
        {
            if (Machine.Mode != OperatingMode.Manual)
                return;

            if (Machine.Plane != ArcPlane.XY)
                Machine.SendCommand("G17");
        }



        public async void OpenEagleBoardFile()
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!String.IsNullOrEmpty(file))
            {
                if (await Machine.PCBManager.OpenFileAsync(file))
                {
                }

            }
        }

        public void CloseEagleBoardFile()
        {

        }


        public void ClearHeightMap()
        {
            Machine.HeightMapManager.CloseHeightMap();
        }

        public async void ArcToLine()
        {
            if (Machine.GCodeFileManager.HasValidFile)
            {
                var result = await Popups.PromptForDoubleAsync("Convert Line to Arch", Machine.Settings.ArcToLineSegmentLength, "Enter Arc Width", true);
                if (result.HasValue)
                    Machine.GCodeFileManager.ArcToLines(result.Value);
            }
        }


        public async void OpenGCodeFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(file))
            {
                if (await Machine.GCodeFileManager.OpenFileAsync(file))
                {
                }
            }
        }


        public void CloseFile(object instance)
        {
            Machine.GCodeFileManager.CloseFileAsync();
        }

        public void ProbeHeightMap()
        {

        }

        public async void OpenHeightMapFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterHeightMap);
            if (!String.IsNullOrEmpty(file))
            {
                await Machine.HeightMapManager.OpenHeightMapAsync(file);
            }
        }

        public async void SaveHeightMap()
        {
            var file = await Popups.ShowSaveFileAsync(Constants.FileFilterHeightMap);
            if (!String.IsNullOrEmpty(file))
            {
                await Machine.HeightMapManager.SaveHeightMapAsync(file);
            }
        }

        public async void ApplyHeightMap()
        {
            if (CanApplyHeightMap())
            {
                if (Machine.GCodeFileManager.HeightMapApplied)
                {
                    if (await Popups.ConfirmAsync("Height Map", "Height Map has already been applied, doing so again will likely produce incorrect results.\r\n\r\nYou should reload the original GCode File then re-apply the height map.\r\n\r\nContinue? "))
                    {
                        Machine.GCodeFileManager.ApplyHeightMap(Machine.HeightMapManager.HeightMap);
                    }
                }
                else
                    Machine.GCodeFileManager.ApplyHeightMap(Machine.HeightMapManager.HeightMap);
            }
        }

        public void SaveModifiedGCode()
        {

        }

        public async void GenerateHoldDownGCode()
        {
            var drillIntoUnderlayment = await Popups.ConfirmAsync("Drill Holes In Underlayment?", "Would you also like to drill the holes in the underlayment?  You only need to use this once when setting up a fixture.  After that you should use the holes that were already created.");
            Machine.GCodeFileManager.SetGCode(GCodeEngine.CreateHoldDownGCode(Machine.PCBManager.Board, Machine.PCBManager.Project, drillIntoUnderlayment));
        }

        public void GenerateMillingGCode()
        {
            Machine.GCodeFileManager.SetGCode(GCodeEngine.CreateCutoutMill(Machine.PCBManager.Board, Machine.PCBManager.Project));
        }

        public void GenerateDrillGCode()
        {
            Machine.GCodeFileManager.SetGCode(GCodeEngine.CreateDrillGCode(Machine.PCBManager.Board, Machine.PCBManager.Project));
        }

        public void ShowTopEtchingGCode()
        {
            Machine.GCodeFileManager.OpenFileAsync(Machine.PCBManager.Project.TopEtchingFileLocalPath);
            Machine.GCodeFileManager.ApplyOffset(Machine.PCBManager.Project.ScrapSides, Machine.PCBManager.Project.ScrapTopBottom, 0);
        }

        public void ShowBottomEtchingGCode()
        {
            Machine.GCodeFileManager.OpenFileAsync(Machine.PCBManager.Project.BottomEtchingFileLocalPath);
            Machine.GCodeFileManager.ApplyOffset((Machine.PCBManager.Project.ScrapSides) + Machine.PCBManager.Board.Width, Machine.PCBManager.Project.ScrapTopBottom, 0);
        }


        PcbMillingProject _project;
        public PcbMillingProject Project
        {
            get { return _project; }
            set
            {
                _project = value;
                Machine.PCBManager.Project = value;
                RaisePropertyChanged();
            }
        }

        public async Task AddProjectFileMRUAsync(string projectFile)
        {
            if (projectFile == _mruManager.Files.ProjectFiles.FirstOrDefault())
            {
                return;
            }

            var existingProjectItem = _mruManager.Files.ProjectFiles.IndexOf(projectFile);
            if (existingProjectItem > -1)
            {
                _mruManager.Files.ProjectFiles.RemoveAt(existingProjectItem);
            }

            _mruManager.Files.ProjectFiles.Insert(0, projectFile);
            if (_mruManager.Files.ProjectFiles.Count > 10)
            {
                _mruManager.Files.ProjectFiles.RemoveAt(10);
            }

            await _mruManager.SaveAsync();
        }

        public async Task<bool> OpenProjectAsync(string projectFile)
        {
            try
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IMruManager MruManager => _mruManager;

        public RelayCommand ArcToLineCommand { get; private set; }

        public RelayCommand OpenEagleBoardFileCommand { get; private set; }
        public RelayCommand CloseEagleBoardFileCommand { get; private set; }
        
        public RelayCommand OpenHeightMapCommand { get; private set; }
        public RelayCommand SaveHeightMapCommand { get; private set; }
        public RelayCommand ApplyHeightMapCommand { get; private set; }
        public RelayCommand ClearHeightMapCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }

        public RelayCommand OpenGCodeCommand { get; private set; }
        public RelayCommand SaveModifiedGCodeCommamnd { get; private set; }
        public RelayCommand ClearGCodeCommand { get; private set; }

        public RelayCommand ShowTopEtchingGCodeCommand { get; private set; }
        public RelayCommand ShowBottomEtchingGCodeCommand { get; private set; }
        public RelayCommand ShowCutoutMillingGCodeCommand { get; private set; }
        public RelayCommand ShowDrillGCodeCommand { get; private set; }
        public RelayCommand ShowHoldDownGCodeCommand { get; private set; }
    }
}
