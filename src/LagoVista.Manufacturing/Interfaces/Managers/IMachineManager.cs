using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IMachineManager
    {
        Task<InvokeResult> AddMachineAsync(Machine Machine, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateMachineAsync(Machine Machine, EntityHeader org, EntityHeader user);
        Task<ListResponse<MachineSummary>> GetMachineSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<Machine> GetMachineAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteMachineAsync(string id, EntityHeader org, EntityHeader user);
    }
}
