// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0f899c410e15b74c63c4ee756e8b25c07a018aba73cf8e7e12923434b213af03
// IndexVersion: 0
// --- END CODE INDEX META ---
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
