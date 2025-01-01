using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IGCodeMappingManager
    {
        Task<InvokeResult> AddGCodeMappingAsync(GCodeMapping mapping, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateGCodeMappingAsync(GCodeMapping mapping, EntityHeader org, EntityHeader user);
        Task<ListResponse<GCodeMappingSummary>> GetGCodeMappingsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<GCodeMapping> GetGCodeMappingAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteGCodeMappingAsync(string id, EntityHeader org, EntityHeader user);
    }
}
