using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IStripFeederTemplateRepo
    {
        Task AddStripFeederTemplateAsync(StripFeederTemplate feeder);
        Task UpdateStripFeederTemplateAsync(StripFeederTemplate feeder);
        Task<ListResponse<StripFeederTemplateSummary>> GetStripFeederTemplateSummariesAsync(string id, ListRequest listRequest);
        Task<StripFeederTemplate> GetStripFeederTemplateAsync(string id);
        Task DeleteStripFeederTemplateAsync(string id);
    }
}
