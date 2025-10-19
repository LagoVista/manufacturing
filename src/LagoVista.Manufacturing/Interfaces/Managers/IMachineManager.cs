// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b0b926ffdd691cc8e455f465e0cff2dc3b7775a83df47445db51067b281f7a66
// IndexVersion: 0
// --- END CODE INDEX META ---
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
        Task<InvokeResult<PcbJobTestFit>> TestFitJobAsync(string machineId, CircuitBoardRevision revision, EntityHeader org, EntityHeader user);

    }
}
