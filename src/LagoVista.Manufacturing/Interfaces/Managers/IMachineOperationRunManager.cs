using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IMachineOperationRunManager
    {
        Task<InvokeResult> SaveSnapshotAsync(MachineOperationRunSnapshot snapshot,
            MachineOperationDefinition definition, EntityHeader org, EntityHeader user);
        Task<MachineOperationRunRecord> GetAsync(string runId, EntityHeader org, EntityHeader user);
        Task<ListResponse<MachineOperationRunSummary>> ListAsync(string machineId, string feederId,
            ListRequest request, EntityHeader org, EntityHeader user);
    }
}
