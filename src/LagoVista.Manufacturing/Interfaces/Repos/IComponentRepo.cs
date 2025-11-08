// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4e4ddbd106effd68779541a576f6018f2a95e433780139cc8133e2bd68f505de
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IComponentRepo
    {
        Task AddComponentAsync(Component component);
        Task UpdateComponentAsync(Component component);
        Task<ListResponse<ComponentSummary>> GetComponentSummariesAsync(string id, ListRequest listRequest);

        Task<ListResponse<ComponentSummary>> GetComponentSummariesByTypeAsync(string id, string componentType, ListRequest listRequest);
        Task<Component> GetComponentAsync(string id);
        Task DeleteComponentAsync(string id);
    }
}
