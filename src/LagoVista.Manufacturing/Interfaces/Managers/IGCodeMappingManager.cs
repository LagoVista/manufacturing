// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b7863108d9f7bdbb3f59898cd0c659af43aee0d0530b1183b73e07e91ce56a52
// IndexVersion: 2
// --- END CODE INDEX META ---
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
