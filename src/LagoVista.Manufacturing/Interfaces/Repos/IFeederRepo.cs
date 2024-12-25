using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IFeederRepo
    {
        Task AddFeederAsync(Feeder feeder);
        Task UpdateFeederAsync(Feeder feeder);
        Task<ListResponse<FeederSummary>> GetFeederSummariesAsync(string id, ListRequest listRequest);
        Task<ListResponse<Feeder>> GetFeedersForMachineAsync(string machineId);
        Task<Feeder> GetFeederAsync(string id);
        Task DeleteFeederAsync(string id);
    }
}
