using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
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
        private readonly IVisionProfileManagerViewModel _visionManagerViewModel;

        public HomingViewModel(IMachineRepo machineRepo, IVisionProfileManagerViewModel visionManagerViewModel, ILocatorViewModel locatorViewModel, ILogger logger)
        {
            _locatorViewModel = locatorViewModel;
            _machineRepo = machineRepo;
            _logger = logger;
            _visionManagerViewModel = visionManagerViewModel;
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
            _machineRepo.CurrentMachine.SendSafeMoveHeight();
            _machineRepo.CurrentMachine.GotoWorkspaceHome();
            _visionManagerViewModel.SelectProfile("mchfiducual");

            _locatorViewModel.SetLocatorState(MVLocatorState.WorkHome);

            Status = "Machine Vision - Origin";


            return Task.CompletedTask;
        }
    }
}
