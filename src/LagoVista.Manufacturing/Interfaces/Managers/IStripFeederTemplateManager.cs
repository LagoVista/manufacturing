// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 53d84fcf95abc2a140a5fb87559df6102043b735c112dcd8a1e3ba72ba64620c
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IStripFeederTemplateManager
    {
        Task<InvokeResult> AddStripFeederTemplateAsync(StripFeederTemplate stripFeederTemplate, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateStripFeederTemplateAsync(StripFeederTemplate stripFeederTemplate, EntityHeader org, EntityHeader user);
        Task<ListResponse<StripFeederTemplateSummary>> GetStripFeederTemplateSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<StripFeederTemplate> GetStripFeederTemplateAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteStripFeederTemplateAsync(string id, EntityHeader org, EntityHeader user);
    }
}
