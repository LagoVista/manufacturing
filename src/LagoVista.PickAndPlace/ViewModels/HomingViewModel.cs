using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class HomingViewModel : ViewModelBase, IHomingViewModel
    {
        private readonly IMachine _machine;
        private readonly ILocatorViewModel _locatorViewModel;
        private readonly IVisionProfileManagerViewModel _visionManagerViewModel;

        public HomingViewModel(IMachine machine, IVisionProfileManagerViewModel visionManagerViewModel, ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
            _machine = machine;
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
            _machine.SendSafeMoveHeight();
            _machine.GotoWorkspaceHome();
            _visionManagerViewModel.SelectProfile("mchfiducual");

            _locatorViewModel.SetLocatorState(MVLocatorState.WorkHome);

            Status = "Machine Vision - Origin";


            return Task.CompletedTask;
        }
    }
}
