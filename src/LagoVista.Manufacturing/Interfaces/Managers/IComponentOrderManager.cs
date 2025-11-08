// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 87e51a3b7ea5b214f9b05448dd09c09959b2ffea38e44bc5fd31c422a0b04e8b
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;
using System.IO;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IComponentOrderManager
    {
        Task<InvokeResult> AddComponentOrderAsync(ComponentOrder componentOrder, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateComponentOrderAsync(ComponentOrder componentOrder, EntityHeader org, EntityHeader user);
        Task<ListResponse<ComponentOrderSummary>> GetComponentOrdersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ComponentOrder> GetComponentOrderAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult<Stream>> GenerateLabelsAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user);
    }
}
