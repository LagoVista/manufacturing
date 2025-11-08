// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6d285861eb0850e4f9076f3c2383a5ff6ca1ca3dfb29fbdd80b533d85a5b5b50
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IPcbMillingProjectManager
    {
        Task<InvokeResult> AddPcbMillingProjectAsync(PcbMillingProject PcbMillingProject, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdatePcbMillingProjectAsync(PcbMillingProject PcbMillingProject, EntityHeader org, EntityHeader user);
        Task<ListResponse<PcbMillingProjectSummary>> GetPcbMillingProjectsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<PcbMillingProject> GetPcbMillingProjectAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeletePcbMillingProjectAsync(string id, EntityHeader org, EntityHeader user);
    }
}
