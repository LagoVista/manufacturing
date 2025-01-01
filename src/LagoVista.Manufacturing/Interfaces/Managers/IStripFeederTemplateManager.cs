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
