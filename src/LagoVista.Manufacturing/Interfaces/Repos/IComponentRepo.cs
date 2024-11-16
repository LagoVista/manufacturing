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
        Task<Component> GetComponentAsync(string id);
        Task DeleteComponentAsync(string id);
    }
}
