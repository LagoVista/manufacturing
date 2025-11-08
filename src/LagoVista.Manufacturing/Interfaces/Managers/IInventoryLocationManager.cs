// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 293087584369a149ef430704150fabba0bc754e3c7fc93a9d243f18df879b7ac
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IInventoryLocationManager
    {
        Task<InvokeResult> AddInventoryLocationAsync(InventoryLocation inventoryLocation, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateInventoryLocationAsync(InventoryLocation inventoryLocation, EntityHeader org, EntityHeader user);
        Task<ListResponse<InventoryLocationSummary>> GetInventoryLocationsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<InventoryLocation> GetInventoryLocationAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user);
    }
}
