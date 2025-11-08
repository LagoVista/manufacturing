// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fdbd0ccbc5dd94559f4de3bbd4f18cb1511ec0546c8ae56c2e70803feb2dd1f1
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IMachineRepo
    {
        Task AddMachineAsync(Machine Machine);
        Task UpdateMachineAsync(Machine Machine);
        Task<ListResponse<MachineSummary>> GetMachineSummariesAsync(string id, ListRequest listRequest);
        Task<Machine> GetMachineAsync(string id);
        Task DeleteMachineAsync(string id);
    }
}
