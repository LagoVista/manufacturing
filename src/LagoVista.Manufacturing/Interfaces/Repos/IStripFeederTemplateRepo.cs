// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 170a2912d7d34a8eb960edf6f1ac1d30f38683740a8e77fe29339e72d3194bda
// IndexVersion: 0
// --- END CODE INDEX META ---
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
