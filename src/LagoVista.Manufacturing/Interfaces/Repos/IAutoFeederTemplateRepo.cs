using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IAutoFeederTemplateRepo
    {
        Task AddAutoFeederTemplateAsync(AutoFeederTemplate feeder);
        Task UpdateAutoFeederTemplateAsync(AutoFeederTemplate feeder);
        Task<ListResponse<AutoFeederTemplateSummary>> GetAutoFeederTemplateSummariesAsync(string id, ListRequest listRequest);
        Task<AutoFeederTemplate> GetAutoFeederTemplateAsync(string id);
        Task DeleteAutoFeederTemplateAsync(string id);
    }
}
