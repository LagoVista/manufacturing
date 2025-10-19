// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a2d57923d866e5ff69a28df3ced14c4e2838840c1717d99e60bd931fb2b7857d
// IndexVersion: 0
// --- END CODE INDEX META ---
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
        Task<InvokeResult<string[]>> CreateGCode(GCodeProject project);
        Task<InvokeResult<string[]>> GetGCodeForProjectAsync(string id, EntityHeader org, EntityHeader user);

    }
}
