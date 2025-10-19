// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e6d293c5eed8db15cfea61a9a9c5b03ba8637734f20176cb885a20b1c7985964
// IndexVersion: 0
// --- END CODE INDEX META ---
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
