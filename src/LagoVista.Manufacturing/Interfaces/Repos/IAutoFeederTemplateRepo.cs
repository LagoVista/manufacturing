// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 74bec72b361c1c3d9c54a10211370d603dfc8a7b1146cb4e4942b2164121d5a7
// IndexVersion: 2
// --- END CODE INDEX META ---
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
