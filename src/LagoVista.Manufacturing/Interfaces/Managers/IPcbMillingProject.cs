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
