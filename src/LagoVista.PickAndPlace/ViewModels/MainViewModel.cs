using System.Threading.Tasks;
using System.Linq;
using LagoVista.PCB.Eagle.Models;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using LagoVista.Client.Core;
using System;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class MainViewModel : GCodeAppViewModelBase
    {

        LagoVista.Client.Core.IRestClient _restClient;
        private string _currentMachineId;

        bool _networkCallInProcess;
        public bool NetworkCallInProcess
        {
            get { return _networkCallInProcess; }
            set { Set(ref _networkCallInProcess, value); }
        }

        public MainViewModel(string currentMachineId) : base()
        {
            Machine = new Machine();

            InitCommands();
            InitChildViewModels();

            _currentMachineId = currentMachineId;

            _restClient = SLWIOC.Get<IRestClient>();
            _restClient.BeginCall += _restClient_BeginCall;
            _restClient.EndCall += _restClient_EndCall;
        }

        private void _restClient_EndCall(object sender, EventArgs e)
        {
            NetworkCallInProcess = false;
        }

        private void _restClient_BeginCall(object sender, EventArgs e)
        {
            NetworkCallInProcess = true;
        }


        public async override Task InitAsync()
        {
            await Machine.InitAsync();
            await base.InitAsync();

            if (!String.IsNullOrEmpty(_currentMachineId))
            {
                var result = await _restClient.GetAsync<DetailResponse<Manufacturing.Models.Machine>>($"/api/mfg/machine/{_currentMachineId}");
                if (result.Successful)
                    Machine.Settings = result.Result.Model;
            }
        }

        private void InitChildViewModels()
        {
            JobControlVM = new JobControlViewModel(Machine);
            MachineControlVM = new MachineControlViewModel(Machine);
        }

        public async Task LoadMRUs()
        {
            MRUs = await Storage.GetAsync<MRUs>("mrus.json");
            if(MRUs == null)
            {
                MRUs = new MRUs();
            }
        }

        public async void AddGCodeFileMRU(string gcodeFile)
        {
            if (gcodeFile == MRUs.GCodeFiles.FirstOrDefault())
            {
                return;
            }

            MRUs.GCodeFiles.Insert(0, gcodeFile);
            if (MRUs.GCodeFiles.Count > 10)
            {
                MRUs.GCodeFiles.RemoveAt(10);
            }

            await SaveMRUsAsync();
        }

        public async void AddPnPJobFile(string pnpJobFile)
        {
            if (pnpJobFile == MRUs.PnPJobs.FirstOrDefault())
            {
                return;
            }

            MRUs.PnPJobs.Insert(0, pnpJobFile);
            if (MRUs.PnPJobs.Count > 10)
            {
                MRUs.PnPJobs.RemoveAt(10);
            }

            await SaveMRUsAsync();

        }

        public async void AddBoardFileMRU(string boardFile)
        {
            if (boardFile == MRUs.BoardFiles.FirstOrDefault())
            {
                return;
            }

            MRUs.BoardFiles.Insert(0, boardFile);
            if (MRUs.BoardFiles.Count > 10)
            {
                MRUs.BoardFiles.RemoveAt(10);
            }

            await SaveMRUsAsync();
        }

        public async void AddProjectFileMRU(string projectFile)
        {
            if(projectFile == MRUs.ProjectFiles.FirstOrDefault())
            {
                return;
            }

            var existingProjectItem = MRUs.ProjectFiles.IndexOf(projectFile);
            if(existingProjectItem > -1)
            {
                MRUs.ProjectFiles.RemoveAt(existingProjectItem);
            }

            MRUs.ProjectFiles.Insert(0, projectFile);
            if(MRUs.ProjectFiles.Count > 10)
            {
                MRUs.ProjectFiles.RemoveAt(10);
            }

            await SaveMRUsAsync();
        }

        public async Task<bool> OpenProjectAsync(string projectFile)
        {
            try
            {
                Project = await PcbProject.OpenAsync(projectFile);
                Machine.PCBManager.ProjectFilePath = projectFile;
                AddProjectFileMRU(projectFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task SaveMRUsAsync()
        {
            await Storage.StoreAsync(this.MRUs, "mrus.json");
        }
    }
}
