// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2ed9ec08d6caf034a5da899e0bee9d8de88f216b164a3d6e78a3fc12c4ab64a9
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class HomingViewModel : ViewModelBase, IHomingViewModel
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILogger _logger;
        private readonly ILocatorViewModel _locatorViewModel;

        public HomingViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel, ILogger logger)
        {
            _locatorViewModel = locatorViewModel;
            _machineRepo = machineRepo;
            _logger = logger;
        }

        private string _status;
        public string Status
        {
            set => Set(ref _status, value);
            get => _status;
        }

        public Task MachineHomeAsync()
        {
            throw new NotImplementedException();
        }

        public Task WorkSpaceHomeAsync()
        {
            _machineRepo.CurrentMachine.GotoWorkspaceHome();

            _locatorViewModel.SetLocatorState(MVLocatorState.WorkHome);

            Status = "Machine Vision - Origin";


            return Task.CompletedTask;
        }
    }
}
