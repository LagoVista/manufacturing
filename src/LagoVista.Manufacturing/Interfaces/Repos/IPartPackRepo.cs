using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IPartPackRepo
    {
        Task AddPartPackAsync(PartPack partPack);
        Task UpdatePartPackAsync(PartPack partPack);
        Task<ListResponse<PartPackSummary>> GetPartPackSummariesAsync(string id, ListRequest listRequest);
        Task<PartPack> GetPartPackAsync(string id);
        Task DeletePartPackAsync(string id);
    }
}
