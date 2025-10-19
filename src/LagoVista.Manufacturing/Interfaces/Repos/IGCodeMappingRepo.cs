// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0168e80ce462d2bf0f4afb9e39b50f19ff3bfdbed1e92ecf2d7c2afc7e1af054
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IGCodeMappingRepo
    {
        Task AddGCodeMappingAsync(GCodeMapping mapping);
        Task UpdateGCodeMappingAsync(GCodeMapping mapping);
        Task<ListResponse<GCodeMappingSummary>> GetGCodeMappingSummariesAsync(string id, ListRequest listRequest);
        Task<GCodeMapping> GetGCodeMappingAsync(string id);
        Task DeleteGCodeMappingAsync(string id);
    }
}
