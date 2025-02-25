using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IInventoryLocationRepo
    {
        Task AddInventoryLocationAsync(InventoryLocation inventoryLocation);
        Task UpdateInventoryLocationAsync(InventoryLocation inventoryLocation);
        Task<ListResponse<InventoryLocationSummary>> GetInventoryLocationSummariesAsync(string id, ListRequest listRequest);
        Task<InventoryLocation> GetInventoryLocationAsync(string id);
        Task DeleteInventoryLocationAsync(string id);
    }
}
