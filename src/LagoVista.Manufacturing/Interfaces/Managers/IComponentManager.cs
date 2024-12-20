using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;
using System.IO;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IComponentManager
    {
        Task<InvokeResult> AddComponentAsync(Component component, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateComponentAsync(Component component, EntityHeader org, EntityHeader user);
        Task<ListResponse<ComponentSummary>> GetComponentsSummariesAsync(ListRequest listRequest, string componentType, EntityHeader org, EntityHeader user);
        Task<Component> GetComponentAsync(string id, bool loadPackage, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> AddComponentPurchaseAsync(string componentId, ComponentPurchase purchase, EntityHeader org, EntityHeader user);
        Task<InvokeResult<Stream>> GenerateLabelAsync(string compoentId, int row, int col, EntityHeader org, EntityHeader user);
        Task<InvokeResult<Stream>> GenerateLabelsAsync(string[] compoentId, int row, int col, EntityHeader org, EntityHeader user);
        Task<InvokeResult> ReceiveComponentPurchaseAsync(string componentId, string orderId, decimal qty, EntityHeader org, EntityHeader user);

    }
}
