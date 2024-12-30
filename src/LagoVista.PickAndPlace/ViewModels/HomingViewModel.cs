using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class HomingViewModel : ViewModelBase
    {
        private readonly IMachine _machine;
        private readonly ILocatorViewModel _locatorViewModel;
        private readonly IVisionManagerViewModel _visionManagerViewModel;

        public HomingViewModel(IMachine machine, IVisionManagerViewModel visionManagerViewModel, ILocatorViewModel locatorViewModel)
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
            _machine.SendCommand(SafeHeightGCodeGCode());
            _machine.GotoWorkspaceHome();
            _visionManagerViewModel.SelectProfile("mchfiducual");

            _locatorViewModel.SetLocatorState(MVLocatorState.WorkHome);

            Status = "Machine Vision - Origin";

        }
    }
}
