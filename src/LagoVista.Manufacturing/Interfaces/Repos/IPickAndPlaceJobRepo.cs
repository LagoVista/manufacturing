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
