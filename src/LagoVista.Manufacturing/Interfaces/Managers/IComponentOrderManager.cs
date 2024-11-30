using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IComponentOrderManager
    {
        Task<InvokeResult> AddComponentOrderAsync(ComponentOrder componentOrder, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateComponentOrderAsync(ComponentOrder componentOrder, EntityHeader org, EntityHeader user);
        Task<ListResponse<ComponentOrderSummary>> GetComponentOrdersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ComponentOrder> GetComponentOrderAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user);
    }
}
