// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ee393f5aaf07c63f948426702977bc92e2e1d877a742f0e2efe829caeb7924bf
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Client.Core;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILogger _logger;
        private readonly IRestClient _restClient;
        private readonly IMruManager _mruManager;

        public HomeViewModel(IMachineRepo machineRepo, IMruManager mruManager, ILogger logger, IRestClient restClient)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));

            _mruManager = mruManager ?? throw new ArgumentNullException(nameof(mruManager));

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
