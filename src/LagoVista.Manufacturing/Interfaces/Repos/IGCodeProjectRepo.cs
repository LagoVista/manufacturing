// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6b047a3d74ddd39e9369f93108ceec5ab84d8c707ac507ffe325e56005d62251
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IGCodeProjectRepo
    {
        Task AddGCodeProjectAsync(GCodeProject GCodeProject);
        Task UpdateGCodeProjectAsync(GCodeProject GCodeProject);
        Task<ListResponse<GCodeProjectSummary>> GetGCodeProjectSummariesAsync(string id, ListRequest listRequest);
        Task<GCodeProject> GetGCodeProjectAsync(string id);
        Task DeleteGCodeProjectAsync(string id);
    }
}
