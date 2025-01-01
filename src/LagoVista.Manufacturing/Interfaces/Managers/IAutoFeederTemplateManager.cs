using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IAutoFeederTemplateManager
    {
        Task<InvokeResult> AddAutoFeederTemplateAsync(AutoFeederTemplate autoFeederTemplate, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateAutoFeederTemplateAsync(AutoFeederTemplate autoFeederTemplate, EntityHeader org, EntityHeader user);
        Task<ListResponse<AutoFeederTemplateSummary>> GetAutoFeederTemplateSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<AutoFeederTemplate> GetAutoFeederTemplateAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteAutoFeederTemplateAsync(string id, EntityHeader org, EntityHeader user);
    }
}
