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
