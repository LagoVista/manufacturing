// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4e06aa8dce6a221f40747bd9ff69e2ffdfc7066783a0e05d7b3bd159e1f814c8
// IndexVersion: 0
// --- END CODE INDEX META ---
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
