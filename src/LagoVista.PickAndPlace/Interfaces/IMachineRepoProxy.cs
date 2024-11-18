using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IMachineRepoProxy
    {
        Task<string> GetCurrentMachineAsync();
        Task SetCurrentMachineAsync(string machineId);

        Task<List<MachineSummary>> GetMachinesAsync();

        Task<Machine> GetMachine(string machineId);

        Task AddMachineAsync(Machine machine);
        
        Task UpdateMachineAsync(Machine machine);
    }
}
