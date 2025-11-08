// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d826f5935aca73aa4c1dbde5e069a2791312a675afdb99d6f91271e3b9d42868
// IndexVersion: 2
// --- END CODE INDEX META ---
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
