// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2d9f0b9dd75f63231de4c0751b788f04e13802bdf00727e6555a3588130b57f6
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Client.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.MediaServices.Models;
using LagoVista.PCB.Eagle.Managers;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using LagoVista.PickAndPlace.Interfaces.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Managers;

namespace LagoVista.PickAndPlace.ViewModels.PcbFab
{
    public class PcbMillingViewModel : MachineViewModelBase, IPcbMillingViewModel
    {
        private readonly IRestClient _restClient;
        private bool _isEditing;
        private readonly IPcb2GCodeService _pcb2GCodeService;

        public PcbMillingViewModel(IRestClient restClient, IGCodeFileManager gcodeFileManager, IPCBManager pcbManager, IHeightMapManager heightMapManager, IMachineRepo machineRepo, IPcb2GCodeService pcb2GCodeService) : base(machineRepo)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _pcb2GCodeService = pcb2GCodeService ?? throw new ArgumentNullException(nameof(pcb2GCodeService));
            GCodeFileManager = gcodeFileManager ?? throw new ArgumentNullException(nameof(gcodeFileManager));
            HeightMapManager = heightMapManager ?? throw new ArgumentNullException(nameof(heightMapManager));
            PCBManager = pcbManager ?? throw new ArgumentNullException(nameof(pcbManager));

            CreateProjectCommand = new RelayCommand(CreateNewProject);

            SaveProjectCommand = new RelayCommand(() => SaveAsync());
            EditProjectCommand = new RelayCommand(() => EditProject(), () => Project != null);

            OpenEagleBoardCommand = new RelayCommand(OpenEagleBoard);

            OpenBottomMillingFilecommand = new RelayCommand(OpenBottomMillingFile);
            OpenBottomDrillFileCommand = new RelayCommand(OpenBottomDrillFile);
            OpenMillingFilecommand = new RelayCommand(OpenMillingFile);
            OpenDrillFileCommand = new RelayCommand(OpenDrillFile);            
            OpenTopEtchingCommand = new RelayCommand(OpenTopEtching);
            OpenBottomEtchingCommand = new RelayCommand(OpenBottomEtching);
            CenterBoardCommand = new RelayCommand(CenterBoard);

            ShowBoardDrillingGCodeCommand = new RelayCommand(ShowBoardDrillingGCode, () => PCB != null && Project != null && !String.IsNullOrEmpty(Project.DrillFileLocalPath));
            ShowBoardMillingGCodeCommand = new RelayCommand(ShowBoardMillingGCode, () => PCB != null && Project != null && !String.IsNullOrEmpty(Project.MillingFileLocalPath));

            CreateHeightMaCommand = new RelayCommand(CreateHeightMap, () => PCB != null && Project != null);
            
            ShowBoardBottomDrillingGCodeCommand = new RelayCommand(ShowBoardBottomDrillingGCode, () => PCB != null && Project != null && !String.IsNullOrEmpty(Project.DrillBottomFileLocalPath));
            ShowBoardBottomMillingGCodeCommand = new RelayCommand(ShowBoardBottomMillingGCode, () => PCB != null && Project != null && !String.IsNullOrEmpty(Project.MillingBottomFileLocalPath));

            ShowBottomIsolationGCodeCommand = new RelayCommand(ShowBottomIsolationGCode, () => PCB != null && Project != null && !String.IsNullOrEmpty(Project.BottomEtchingFileLocalPath));
            ShowTopIsolutionGCodeCommand = new RelayCommand(ShowTopIsolationGCode, () => PCB != null && Project != null && !String.IsNullOrEmpty(Project.TopEtchingFileLocalPath));
            ShowHoldDownGCodeCommand = new RelayCommand(ShowHoldDownGCode, () => PCB != null && Project != null);
            OpenGCodeFileCommand = new RelayCommand(OpenGCodeFile);
            GenerateIsolationMillingCommand = new RelayCommand(() =>
            {
                _pcb2GCodeService.CreateGCode(Project);
                RaiseCanExecuteChanged();
            }, () => Project != null && !String.IsNullOrEmpty(Project.EagleBRDFileLocalPath));
        }


        void ShowHoldDownGCode()
        {
            var gcode = GCodeEngine.CreateHoldDownGCode(PCB, Project, false);
            GCodeFileManager.SetGCode(gcode);
        }

        async void ShowBoardDrillingGCode()
        {
            if (!String.IsNullOrEmpty(Project.DrillFileLocalPath))
            {
                await GCodeFileManager.OpenFileAsync(Project.DrillFileLocalPath);
            }
            else
            {
                var gcode = GCodeEngine.CreateDrillGCode(PCB, Project);
                GCodeFileManager.SetGCode(gcode);
            }

            GCodeFileManager.ApplyOffset(Project.ScrapSides, Project.ScrapTopBottom, 0);
        }

        async void ShowBoardMillingGCode()
        {
            if (!String.IsNullOrEmpty(Project.MillingFileLocalPath))
            {
                await GCodeFileManager.OpenFileAsync(Project.MillingFileLocalPath);
            }
            else
            {
                var gcode = GCodeEngine.CreateCutoutMill(PCB, Project);
                GCodeFileManager.SetGCode(gcode);
            }

            GCodeFileManager.ApplyOffset(Project.ScrapSides, Project.ScrapTopBottom, 0);
        }


        async void ShowBoardBottomDrillingGCode()
        {
            if (!String.IsNullOrEmpty(Project.DrillBottomFileLocalPath))
            {
                await GCodeFileManager.OpenFileAsync(Project.DrillBottomFileLocalPath);
            }
            else
            {
                var gcode = GCodeEngine.CreateDrillGCode(PCB, Project);
                GCodeFileManager.SetGCode(gcode);
            }

            GCodeFileManager.ApplyOffset(Project.ScrapSides, Project.ScrapTopBottom, 0);
        }

        async void ShowBoardBottomMillingGCode()
        {
            if (!String.IsNullOrEmpty(Project.MillingBottomFileLocalPath))
            {
                await GCodeFileManager.OpenFileAsync(Project.MillingBottomFileLocalPath);
            }
            else
            {
                var gcode = GCodeEngine.CreateCutoutMill(PCB, Project);
                GCodeFileManager.SetGCode(gcode);
            }

            GCodeFileManager.ApplyOffset(Project.ScrapSides + PCB.Width, Project.ScrapTopBottom, 0);
        }

        async void ShowBottomIsolationGCode()
        {
            await GCodeFileManager.OpenFileAsync(Project.BottomEtchingFileLocalPath);
            GCodeFileManager.ApplyOffset(Project.ScrapSides + PCB.Width, Project.ScrapTopBottom, 0);
        }

        async void ShowTopIsolationGCode()
        {
           await GCodeFileManager.OpenFileAsync(Project.TopEtchingFileLocalPath);
            GCodeFileManager.ApplyOffset(Project.ScrapSides, Project.ScrapTopBottom, 0);
        }

        public async void OpenEagleBoard()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    Project.EagleBRDFileLocalPath = result;
                    Project.EagleBRDFile = null;
                    Project.TopEtchingFile = null;
                    Project.BottomEtchingFile = null;
                    Project.DrillFile = null;
                    Project.MillingFile = null;
                    Project.TopEtchingFileLocalPath = null;
                    Project.BottomEtchingFileLocalPath = null;
                    Project.DrillFileLocalPath = null;
                    Project.MillingFileLocalPath = null;

                    var doc = XDocument.Load(Project.EagleBRDFileLocalPath);
                    PCB = EagleParser.ReadPCB(doc);
                    Project.FiducialOptions = PCB.Holes.Where(drl => drl.D > 2).ToList();
                }
                catch
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not open Eagle Board File.");
                }

                OpenEagleBoardCommand = new RelayCommand(OpenEagleBoard);
            }
        }

        public void CreateHeightMap()
        {            
            if (Project != null && PCB != null)
            {
                var heightMap = new HeightMap(null);
                heightMap.Min = new Core.Models.Drawing.Vector2(Project.ScrapSides, Project.ScrapTopBottom);
                heightMap.Max = new Core.Models.Drawing.Vector2(PCB.Width + Project.ScrapSides, PCB.Height + Project.ScrapTopBottom);
                heightMap.GridSize = Project.HeightMapGridSize;
                HeightMapManager.NewHeightMap(heightMap);
            }            
        }

        public async void OpenGCodeFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
                await GCodeFileManager.OpenFileAsync(result);
            }
        }

        public async void OpenDrillFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
                Project.DrillFileLocalPath = result;
                RaiseCanExecuteChanged();
            }
        }

        public async void OpenMillingFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
                Project.MillingFileLocalPath = result;
                RaiseCanExecuteChanged();
            }
        }

        public async void OpenBottomDrillFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
                Project.DrillBottomFileLocalPath = result;
                RaiseCanExecuteChanged();
            }
        }

        public async void OpenBottomMillingFile()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
                Project.MillingBottomFileLocalPath = result;
                RaiseCanExecuteChanged();
            }
        }

        public async void OpenTopEtching()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
                Project.TopEtchingFileLocalPath = result;
                RaiseCanExecuteChanged();
            }
        }

        public async void OpenBottomEtching()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
                Project.BottomEtchingFileLocalPath = result;
                RaiseCanExecuteChanged();
            }
        }

        public void CenterBoard()
        {
            Project.ScrapSides = Math.Round((Project.StockWidth - PCB.Width) / 2, 2);
            Project.ScrapTopBottom = Math.Round((Project.StockHeight - PCB.Height) / 2, 2);
        }

        public override async Task InitAsync()
        {
            var response = await _restClient.GetListResponseAsync<PcbMillingProjectSummary>("/api/mfg/pcb/millings");
            if (response.Successful)
            {
                Projects = new ObservableCollection<PcbMillingProjectSummary>(response.Model);
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, response.ErrorMessage);
            }

            await base.InitAsync();
        }

        public async void EditProject()
        {
            await Popups.ShowModalAsync<IPcbMillingProjectWindow, PcbMillingViewModel>(this);
        }

        public async void SaveAsync()
        {
            if (EntityHeader.IsNullOrEmpty(Project.EagleBRDFile) && !String.IsNullOrEmpty(Project.EagleBRDFileLocalPath))
            {
                var buffer = System.IO.File.ReadAllBytes(Project.EagleBRDFileLocalPath);
                var result = await _restClient.PostFormFileAsync("/api/media/resource/upload", buffer, "eagleboardfile");
                var mediaResult = JsonConvert.DeserializeObject<InvokeResult<MediaResource>>(result.Content);
                if (mediaResult.Successful)
                {
                    Project.EagleBRDFile = mediaResult.Result.ToEntityHeader();
                }
            }

            if (EntityHeader.IsNullOrEmpty(Project.TopEtchingFile) && !String.IsNullOrEmpty(Project.TopEtchingFileLocalPath))
            {
                var buffer = System.IO.File.ReadAllBytes(Project.TopEtchingFileLocalPath);
                var result = await _restClient.PostFormFileAsync("/api/media/resource/upload", buffer, "topetching");
                var mediaResult = JsonConvert.DeserializeObject<InvokeResult<MediaResource>>(result.Content);
                if (mediaResult.Successful)
                {
                    Project.TopEtchingFile = mediaResult.Result.ToEntityHeader();
                }
            }

            if (EntityHeader.IsNullOrEmpty(Project.BottomEtchingFile) && !String.IsNullOrEmpty(Project.BottomEtchingFileLocalPath))
            {
                var buffer = System.IO.File.ReadAllBytes(Project.BottomEtchingFileLocalPath);
                var result = await _restClient.PostFormFileAsync("/api/media/resource/upload", buffer, "bottometching");
                var mediaResult = JsonConvert.DeserializeObject<InvokeResult<MediaResource>>(result.Content);
                if (mediaResult.Successful)
                {
                    Project.BottomEtchingFile = mediaResult.Result.ToEntityHeader();
                }
            }

            if (EntityHeader.IsNullOrEmpty(Project.DrillFile) && !String.IsNullOrEmpty(Project.DrillFileLocalPath))
            {
                var buffer = System.IO.File.ReadAllBytes(Project.DrillFileLocalPath);
                var result = await _restClient.PostFormFileAsync("/api/media/resource/upload", buffer, "drillfile");
                var mediaResult = JsonConvert.DeserializeObject<InvokeResult<MediaResource>>(result.Content);
                if (mediaResult.Successful)
                {
                    Project.DrillFile = mediaResult.Result.ToEntityHeader();
                }
            }

            if (EntityHeader.IsNullOrEmpty(Project.MillingFile) && !String.IsNullOrEmpty(Project.MillingFileLocalPath))
            {
                var buffer = System.IO.File.ReadAllBytes(Project.MillingFileLocalPath);
                var result = await _restClient.PostFormFileAsync("/api/media/resource/upload", buffer, "milling");
                var mediaResult = JsonConvert.DeserializeObject<InvokeResult<MediaResource>>(result.Content);
                if (mediaResult.Successful)
                {
                    Project.MillingFile = mediaResult.Result.ToEntityHeader();
                }
            }

            if (EntityHeader.IsNullOrEmpty(Project.DrillBottomlFile) && !String.IsNullOrEmpty(Project.DrillBottomFileLocalPath))
            {
                var buffer = System.IO.File.ReadAllBytes(Project.DrillBottomFileLocalPath);
                var result = await _restClient.PostFormFileAsync("/api/media/resource/upload", buffer, "bottometching");
                var mediaResult = JsonConvert.DeserializeObject<InvokeResult<MediaResource>>(result.Content);
                if (mediaResult.Successful)
                {
                    Project.DrillBottomlFile = mediaResult.Result.ToEntityHeader();
                }
            }

            if (EntityHeader.IsNullOrEmpty(Project.MillingBottomFile) && !String.IsNullOrEmpty(Project.MillingBottomFileLocalPath))
            {
                var buffer = System.IO.File.ReadAllBytes(Project.MillingBottomFileLocalPath);
                var result = await _restClient.PostFormFileAsync("/api/media/resource/upload", buffer, "bottometching");
                var mediaResult = JsonConvert.DeserializeObject<InvokeResult<MediaResource>>(result.Content);
                if (mediaResult.Successful)
                {
                    Project.MillingBottomFile = mediaResult.Result.ToEntityHeader();
                }
            }

            var json = JsonConvert.SerializeObject(Project);
            var obj = JsonConvert.DeserializeObject<PcbMillingProject>(json);

            Project.Key = "dontcare";

            if (_isEditing)
            {
                var result = await _restClient.PutAsync("/api/mfg/pcb/milling", Project);
                if (!result.Successful)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
                }
                else
                {
//                    await Project.SaveAsync(PCBFilepath);
                }
            }
            else
            {
                var result = await _restClient.PostAsync("/api/mfg/pcb/milling", Project);
                if (!result.Successful)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
                }
                else
                {
                    
                }
            }
        }

        public List<Drill> GetHoldDownDrills(PrintedCircuitBoard board)
        {

            var radius = Project.HoldDownDiameter / 2;
            var drills = new List<Drill>
            {
                new Drill() { X = Project.ScrapSides - (Project.HoldDownBoardOffset + radius), Y =Project.ScrapTopBottom - (Project.HoldDownBoardOffset + radius), D = Project.HoldDownDiameter },
                new Drill() { X = board.Width + Project.ScrapSides + Project.HoldDownBoardOffset + radius, Y = Project.ScrapTopBottom - (Project.HoldDownBoardOffset + radius), D = Project.HoldDownDiameter },
                new Drill() { X = Project.ScrapSides - (Project.HoldDownBoardOffset + radius), Y = board.Height + Project.ScrapTopBottom + Project.HoldDownBoardOffset + radius, D = Project.HoldDownDiameter },
                new Drill() { X = board.Width + Project.ScrapSides + Project.HoldDownBoardOffset + radius, Y = board.Height + Project.ScrapTopBottom + Project.HoldDownBoardOffset + radius, D = Project.HoldDownDiameter }
            };
            return drills;
        }


        async void CreateNewProject()
        {
            var result = await _restClient.GetAsync<DetailResponse<PcbMillingProject>>("/api/mfg/pcb/milling/factory");
            if (result.Successful)
            {
                Project = result.Result.Model;
                await Popups.ShowModalAsync<IPcbMillingProjectWindow, PcbMillingViewModel>(this);
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }

        private async void LoadProjectAsync(PcbMillingProjectSummary project)
        {
            var result = await _restClient.GetAsync<DetailResponse<PcbMillingProject>>($"/api/mfg/pcb/milling/{project.Id}");
            if (result.Successful)
            {
                Project = result.Result.Model;
                _isEditing = true;

                var tempPath = System.IO.Path.GetTempPath();

                if (!EntityHeader.IsNullOrEmpty(this.Project.EagleBRDFile))
                {
                    var url = $"https://www.nuviot.com/api/media/resource/{this.Project.OwnerOrganization.Id}/{this.Project.EagleBRDFile.Id}/download";
                    var response = await _restClient.DownloadFileAsync(url);
                    if (response.Success)
                    {
                        var fullFileName = Path.Combine(tempPath, Project.EagleBRDFile.Id + ".brd");
                        System.IO.File.WriteAllBytes(fullFileName, response.BinaryContent);
                        Project.EagleBRDFileLocalPath = fullFileName;
                    }
                }

                if (!EntityHeader.IsNullOrEmpty(this.Project.BottomEtchingFile))
                {
                    var url = $"https://www.nuviot.com/api/media/resource/{this.Project.OwnerOrganization.Id}/{this.Project.TopEtchingFile.Id}/download";
                    var response = await _restClient.DownloadFileAsync(url);
                    if (response.Success)
                    {
                        var fullFileName = Path.Combine(tempPath, Project.TopEtchingFile.Id + ".gcode");
                        System.IO.File.WriteAllBytes(fullFileName, response.BinaryContent);
                        Project.TopEtchingFileLocalPath = fullFileName;
                    }
                }

                if (!EntityHeader.IsNullOrEmpty(this.Project.BottomEtchingFile))
                {
                    var url = $"https://www.nuviot.com/api/media/resource/{this.Project.OwnerOrganization.Id}/{this.Project.BottomEtchingFile.Id}/download";
                    var response = await _restClient.DownloadFileAsync(url);
                    if (response.Success)
                    {
                        var fullFileName = Path.Combine(tempPath, Project.BottomEtchingFile.Id + ".gcode");
                        System.IO.File.WriteAllBytes(fullFileName, response.BinaryContent);
                        Project.BottomEtchingFileLocalPath = fullFileName;
                    }
                }


                if (!EntityHeader.IsNullOrEmpty(this.Project.MillingFile))
                {
                    var url = $"https://www.nuviot.com/api/media/resource/{this.Project.OwnerOrganization.Id}/{this.Project.MillingFile.Id}/download";
                    var response = await _restClient.DownloadFileAsync(url);
                    if (response.Success)
                    {
                        var fullFileName = Path.Combine(tempPath, Project.MillingFile.Id + ".gcode");
                        System.IO.File.WriteAllBytes(fullFileName, response.BinaryContent);
                        Project.MillingFileLocalPath = fullFileName;
                    }
                }

                if (!EntityHeader.IsNullOrEmpty(this.Project.DrillFile))
                {
                    var url = $"https://www.nuviot.com/api/media/resource/{this.Project.OwnerOrganization.Id}/{this.Project.DrillFile.Id}/download";
                    var response = await _restClient.DownloadFileAsync(url);
                    if (response.Success)
                    {
                        var fullFileName = Path.Combine(tempPath, Project.DrillFile.Id + ".gcode");
                        System.IO.File.WriteAllBytes(fullFileName, response.BinaryContent);
                        Project.DrillFileLocalPath = fullFileName;
                    }
                }

                if (!EntityHeader.IsNullOrEmpty(this.Project.MillingBottomFile))
                {
                    var url = $"https://www.nuviot.com/api/media/resource/{this.Project.OwnerOrganization.Id}/{this.Project.MillingBottomFile.Id}/download";
                    var response = await _restClient.DownloadFileAsync(url);
                    if (response.Success)
                    {
                        var fullFileName = Path.Combine(tempPath, Project.MillingBottomFile.Id + ".gcode");
                        System.IO.File.WriteAllBytes(fullFileName, response.BinaryContent);
                        Project.MillingBottomFileLocalPath = fullFileName;
                    }
                }

                if (!EntityHeader.IsNullOrEmpty(this.Project.DrillBottomlFile))
                {
                    var url = $"https://www.nuviot.com/api/media/resource/{this.Project.OwnerOrganization.Id}/{this.Project.DrillBottomlFile.Id}/download";
                    var response = await _restClient.DownloadFileAsync(url);
                    if (response.Success)
                    {
                        var fullFileName = Path.Combine(tempPath, Project.DrillBottomlFile.Id + ".gcode");
                        System.IO.File.WriteAllBytes(fullFileName, response.BinaryContent);
                        Project.DrillBottomFileLocalPath = fullFileName;
                    }
                }


                if (!String.IsNullOrEmpty(Project.EagleBRDFileLocalPath))
                {
                    var doc = XDocument.Load(Project.EagleBRDFileLocalPath);
                    PCB = EagleParser.ReadPCB(doc);
                    Project.FiducialOptions = PCB.Holes.Where(drl => drl.D > 2).ToList();
                }
            }

            RaiseCanExecuteChanged();
            
        }

        new void RaiseCanExecuteChanged()
        {
            this.EditProjectCommand.RaiseCanExecuteChanged();
            this.ShowHoldDownGCodeCommand.RaiseCanExecuteChanged();
            this.ShowBottomIsolationGCodeCommand.RaiseCanExecuteChanged();
            this.ShowTopIsolutionGCodeCommand.RaiseCanExecuteChanged();
            this.ShowBoardBottomDrillingGCodeCommand.RaiseCanExecuteChanged();
            this.ShowBoardBottomMillingGCodeCommand.RaiseCanExecuteChanged();
            this.ShowBoardDrillingGCodeCommand.RaiseCanExecuteChanged();
            this.ShowBoardMillingGCodeCommand.RaiseCanExecuteChanged();
            this.CreateProjectCommand.RaiseCanExecuteChanged();
            this.CreateHeightMaCommand.RaiseCanExecuteChanged();
        }

        ObservableCollection<PcbMillingProjectSummary> _projects = new ObservableCollection<PcbMillingProjectSummary>();
        public ObservableCollection<PcbMillingProjectSummary> Projects
        {
            get => _projects;
            set => Set(ref _projects, value);
        }        


        public PcbMillingProject _project;
        public PcbMillingProject Project
        {
            get => _project;
            set 
            {
                Set(ref _project, value);
            }
        }

        private PrintedCircuitBoard _pcb;
        public PrintedCircuitBoard PCB
        {
            get { return _pcb; }
            set { Set(ref _pcb, value); }
        }


        PcbMillingProjectSummary _selectedProject;
        public PcbMillingProjectSummary SelectedProject
        {
            get => _selectedProject;
            set
            {
                Set(ref _selectedProject, value);
                if(value != null)
                {
                    LoadProjectAsync(value);
                }
            }
        }

        public RelayCommand OpenGCodeFileCommand { get; }

        public RelayCommand CreateProjectCommand {get;}
        public RelayCommand CenterBoardCommand { get; private set; }
        public RelayCommand OpenProjectCommand { get; }
        public RelayCommand SaveProjectCommand { get; }
        public RelayCommand EditProjectCommand { get; }
        public RelayCommand GenerateIsolationMillingCommand { get; }

        public RelayCommand OpenEagleBoardCommand { get; private set; }
        public RelayCommand OpenTopEtchingCommand { get; private set; }
        public RelayCommand OpenBottomEtchingCommand { get; private set; }
        public RelayCommand OpenMillingFilecommand { get; }
        public RelayCommand OpenDrillFileCommand { get; }
        public RelayCommand OpenBottomMillingFilecommand { get; }
        public RelayCommand OpenBottomDrillFileCommand { get; }

        public RelayCommand ShowBoardMillingGCodeCommand { get; }
        public RelayCommand ShowBoardDrillingGCodeCommand { get; }

        public RelayCommand ShowBoardBottomMillingGCodeCommand { get; }
        public RelayCommand ShowBoardBottomDrillingGCodeCommand { get; }



        public RelayCommand ShowTopIsolutionGCodeCommand { get; }
        public RelayCommand ShowBottomIsolationGCodeCommand { get; }
    
        public RelayCommand ShowHoldDownGCodeCommand { get; }

        public RelayCommand CreateHeightMaCommand { get; }

        public IPCBManager PCBManager  {get;}

        public IHeightMapManager HeightMapManager { get; }

        public IGCodeFileManager GCodeFileManager { get; }
    }
}
