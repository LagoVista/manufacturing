using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IStripFeederRepo
    {
        Task AddStripFeederAsync(StripFeeder feeder);
        Task UpdateStripFeederAsync(StripFeeder feeder);
        Task<ListResponse<StripFeederSummary>> GetStripFeederSummariesAsync(string id, ListRequest listRequest);
        Task<StripFeeder> GetStripFeederAsync(string id);
        Task<ListResponse<StripFeeder>> GetStripFeedersForMachineAsync(string machineId);
        Task DeleteStripFeederAsync(string id);
    }
}
