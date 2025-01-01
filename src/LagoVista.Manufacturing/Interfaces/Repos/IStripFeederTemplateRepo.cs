using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IStripFeederTemplateRepo
    {
        Task AddFeederAsync(StripFeederTemplate feeder);
        Task UpdateFeederAsync(StripFeederTemplate feeder);
        Task<ListResponse<StripFeederTemplateSummary>> GetFeederSummariesAsync(string id, ListRequest listRequest);
        Task<StripFeederTemplate> GetFeederAsync(string id);
        Task DeleteFeederAsync(string id);
    }
}
