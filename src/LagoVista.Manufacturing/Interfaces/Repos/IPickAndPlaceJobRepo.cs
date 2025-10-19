// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 194e7a07ced50f0f9c300fecc9fb0fc1b0c7db0d571ce698d78dae3392102451
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IPickAndPlaceJobRepo
    {
        Task AddPickAndPlaceJobAsync(PickAndPlaceJob PickAndPlaceJob);
        Task UpdatePickAndPlaceJobAsync(PickAndPlaceJob PickAndPlaceJob);
        Task<ListResponse<PickAndPlaceJobSummary>> GetPickAndPlaceJobSummariesAsync(string id, ListRequest listRequest);
        Task<PickAndPlaceJob> GetPickAndPlaceJobAsync(string id);
        Task DeletePickAndPlaceJobAsync(string id);
    }
}
