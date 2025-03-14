using LagoVista.Core.Commanding;
using LagoVista.GCode;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.GCode;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.GCode
{
    public class GCodeViewModel : MachineViewModelBase, IGCodeViewModel
    {
        private readonly IMruManager _mruManager;
        private readonly IHeightMapManager _heightMapManager;
        private readonly IGCodeFileManager _gcodeFileManager;

        public GCodeViewModel(IMachineRepo machineRepo, IGCodeFileManager gcodeFileManager, IMruManager mruManager, IHeightMapManager heightMapManager) : base(machineRepo)
        {
            _mruManager = mruManager ?? throw new ArgumentNullException(nameof(mruManager));
            _gcodeFileManager = gcodeFileManager ?? throw new ArgumentNullException(nameof(gcodeFileManager));
            _heightMapManager = heightMapManager ?? throw new ArgumentNullException(nameof(heightMapManager));

            OpenGCodeCommand = new RelayCommand(OpenGCodeFile, CanPerformFileOperation);
            ClearGCodeCommand = new RelayCommand(CloseFile, CanPerformFileOperation);
            ArcToLineCommand = new RelayCommand(ArcToLine, () => _gcodeFileManager.HasValidFile);

            SaveModifiedGCodeCommamnd = new RelayCommand(SaveModifiedGCode, () => _gcodeFileManager.HasValidFile); ;            
        }        
       
        private bool CanPerformFileOperation(Object instance)
        {
            return (Machine.Mode == OperatingMode.Manual || Machine.Mode == OperatingMode.Disconnected);
        }





        private void ButtonArcPlane_Click()
        {
            if (Machine.Mode != OperatingMode.Manual)
                return;

            if (Machine.Plane != ArcPlane.XY)
                Machine.SendCommand("G17");
        }



        public async void ArcToLine()
        {
            if (_gcodeFileManager.HasValidFile)
            {
                var result = await Popups.PromptForDoubleAsync("Convert Line to Arch", Machine.Settings.ArcToLineSegmentLength, "Enter Arc Width", true);
                if (result.HasValue)
                    _gcodeFileManager.ArcToLines(result.Value);
            }
        }


        public async void OpenGCodeFile(object instance)
        {
            var file = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!String.IsNullOrEmpty(file))
            {
                if (await _gcodeFileManager.OpenFileAsync(file))
                {
                }
            }
        }


        public void CloseFile(object instance)
        {
            _gcodeFileManager.CloseFileAsync();
        }

    

        public void SaveModifiedGCode()
        {

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

        public Task<bool> OpenProjectAsync(string projectFile)
        {
            try
            {
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult( false);
            }
        }

        public IHeightMapManager HeightMapManager => _heightMapManager;

        public IMruManager MruManager => _mruManager;

        public RelayCommand ArcToLineCommand { get; private set; }
        


        public RelayCommand OpenGCodeCommand { get; private set; }
        public RelayCommand SaveModifiedGCodeCommamnd { get; private set; }
        public RelayCommand ClearGCodeCommand { get; private set; }



    }
}
