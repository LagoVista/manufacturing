using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IMachineOperationRunRepo
    {
        Task<MachineOperationRunRecord> FindAsync(string runId);
        Task UpsertAsync(MachineOperationRunRecord record);
        Task<ListResponse<MachineOperationRunSummary>> ListAsync(
            string orgId, string machineId, string feederId, ListRequest request);
    }
}
