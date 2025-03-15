using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IGCodeProjectManager
    {
        Task<InvokeResult> AddGCodeProjectAsync(GCodeProject GCodeProject, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateGCodeProjectAsync(GCodeProject GCodeProject, EntityHeader org, EntityHeader user);
        Task<ListResponse<GCodeProjectSummary>> GetGCodeProjectsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<GCodeProject> GetGCodeProjectAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteGCodeProjectAsync(string id, EntityHeader org, EntityHeader user);
    }
}
