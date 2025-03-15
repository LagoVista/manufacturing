using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IPartPackManager
    {
        Task<InvokeResult> AddPartPackAsync(PartPack partPack, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdatePartPackAsync(PartPack partPack, EntityHeader org, EntityHeader user);
        Task<ListResponse<PartPackSummary>> GetPartPacksSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<PartPack> GetPartPackAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user);
    }
}
