using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IMachineRepo
    {
        Task<List<MachineSummary>> GetMachinesAsync();

        Task<Machine> GetMachine(string machineId);

        Task SaveMachineAsync(Machine machine);
    }
}
