using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IPcbMillingProjectRepo
    {
        Task AddPcbMillingProjectAsync(PcbMillingProject PcbMillingProject);
        Task UpdatePcbMillingProjectAsync(PcbMillingProject PcbMillingProject);
        Task<ListResponse<PcbMillingProjectSummary>> GetPcbMillingProjectSummariesAsync(string id, ListRequest listRequest);
        Task<PcbMillingProject> GetPcbMillingProjectAsync(string id);
        Task DeletePcbMillingProjectAsync(string id);
    }
}
