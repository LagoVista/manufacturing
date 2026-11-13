using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Repos
{
    public class MachineOperationRunRepo : DocumentDBRepoBase<MachineOperationRunRecord>, IMachineOperationRunRepo
    {
        public MachineOperationRunRepo(IDocumentCloudCachedServices services) : base(services) { }

        public async Task<MachineOperationRunRecord> FindAsync(string runId)
        {
            var results = await QueryAsync(run => run.Id == runId);
            return results.FirstOrDefault();
        }

        public Task UpsertAsync(MachineOperationRunRecord record) => UpsertDocumentAsync(record);

        public Task<ListResponse<MachineOperationRunSummary>> ListAsync(
            string orgId, string machineId, string feederId, ListRequest request)
        {
            return StringIsBlank(feederId)
                ? QuerySummaryAsync<MachineOperationRunSummary, MachineOperationRunRecord>(
                    run => run.OwnerOrganization.Id == orgId && run.MachineId == machineId,
                    run => run.Name, request)
                : QuerySummaryAsync<MachineOperationRunSummary, MachineOperationRunRecord>(
                    run => run.OwnerOrganization.Id == orgId &&
                        run.MachineId == machineId && run.FeederId == feederId,
                    run => run.Name, request);
        }

        private static bool StringIsBlank(string value) => string.IsNullOrWhiteSpace(value);
    }
}
