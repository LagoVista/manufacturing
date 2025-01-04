using LagoVista.Client.Core;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILogger _logger;
        private readonly IRestClient _restClient;

        public HomeViewModel(IMachineRepo machineRepo, ILogger logger, IRestClient restClient)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));

            _restClient.BeginCall += _restClient_BeginCall;
            _restClient.EndCall += _restClient_EndCall;
        }

        private void _restClient_EndCall(object sender, EventArgs e)
        {
            IsBusy = false;
        }

        private void _restClient_BeginCall(object sender, EventArgs e)
        {
            IsBusy = true;
        }

        public IMachineRepo MachineRepo => _machineRepo;
    }
}
