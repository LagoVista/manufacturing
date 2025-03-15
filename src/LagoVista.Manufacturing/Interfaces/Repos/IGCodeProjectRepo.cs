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
