using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IAutoFeederRepo
    {
        Task AddFeederAsync(AutoFeeder feeder);
        Task UpdateFeederAsync(AutoFeeder feeder);
        Task<ListResponse<AutoFeederSummary>> GetFeederSummariesAsync(string id, ListRequest listRequest);
        Task<ListResponse<AutoFeeder>> GetFeedersForMachineAsync(string machineId);
        Task<AutoFeeder> GetFeederAsync(string id);
        Task<AutoFeeder> GetFeederByFeederIdAsync(string feederId);
        Task DeleteFeederAsync(string id);
    }
}
