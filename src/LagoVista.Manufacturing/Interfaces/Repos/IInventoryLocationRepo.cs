// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 71f43a18f55d3143aa3fca17612f144a59c5e834795c3de02b395a4d5c2c15f1
// IndexVersion: 2
// --- END CODE INDEX META ---
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
