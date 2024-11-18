using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public class MachinesRepo : IMachineRepoProxy
    {
        private readonly IRestClient _restClient;
        private readonly IStorageService _storageService;

        public MachinesRepo(IRestClient restClient, IStorageService storageService)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public Task<List<MachineSummary>> GetMachinesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Machine> GetMachine(string machineId)
        {
            throw new NotImplementedException();
        }

        public Task AddMachineAsync(Machine machine)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMachineAsync(Machine machine)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCurrentMachineAsync()
        {
            throw new NotImplementedException();
        }

        public Task SetCurrentMachineAsync(string id)
        {        
            throw new NotImplementedException();
        }
    }
}
