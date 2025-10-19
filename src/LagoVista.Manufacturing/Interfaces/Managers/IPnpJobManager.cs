// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: bd16f17e0c5d13b9c0326a7035296c5381534d0a3d754047cc33b6027721dbe5
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IPickAndPlaceJobManager
    {
        Task<InvokeResult> AddPickAndPlaceJobAsync(PickAndPlaceJob PickAndPlaceJob, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdatePickAndPlaceJobAsync(PickAndPlaceJob PickAndPlaceJob, EntityHeader org, EntityHeader user);
        Task<ListResponse<PickAndPlaceJobSummary>> GetPickAndPlaceJobSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<PickAndPlaceJob> GetPickAndPlaceJobAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeletePickAndPlaceJobAsync(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<PickAndPlaceJobRunSummary>> GetPickAndPlaceJobRunsAsync(string jobId, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<PickAndPlaceJobRun> GetPickAndPlaceJobRunAsync(string jobRunId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdatePickAndPlaceJobRunAsync(PickAndPlaceJobRun jobRun, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdatePickAndPlaceJobRunPlacementAsync(string id, PickAndPlaceJobRunPlacement placement, EntityHeader org, EntityHeader user);
        Task<InvokeResult> AddPickAndPlaceJobRunAsync(PickAndPlaceJobRun jobRun, EntityHeader org, EntityHeader user);
    }
}
