using LagoVista.Manufacturing.Models;
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
            if (projectFile == _mruManager.ProjectFiles.FirstOrDefault())
            {
                return;
            }

            var existingProjectItem = _mruManager.ProjectFiles.IndexOf(projectFile);
            if (existingProjectItem > -1)
            {
                _mruManager.ProjectFiles.RemoveAt(existingProjectItem);
            }

            _mruManager.ProjectFiles.Insert(0, projectFile);
            if (_mruManager.ProjectFiles.Count > 10)
            {
                _mruManager.ProjectFiles.RemoveAt(10);
            }

            await _mruManager.SaveAsync();
        }

        public async Task<bool> OpenProjectAsync(string projectFile)
        {
            try
            {
                Project = await PcbMillingProject.OpenAsync(projectFile);
                Machine.PCBManager.ProjectFilePath = projectFile;
                await AddProjectFileMRUAsync(projectFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IMruManager MruManager => _mruManager;
    }
}
