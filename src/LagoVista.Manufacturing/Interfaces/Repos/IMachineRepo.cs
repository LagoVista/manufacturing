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
