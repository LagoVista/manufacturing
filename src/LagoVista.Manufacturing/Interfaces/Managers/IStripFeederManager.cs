// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 47bac65af2f3183d41ff5ef206efe531be359702df837ad7bea5895f55176a30
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IStripFeederManager
    {
        Task<InvokeResult> AddStripFeederAsync(StripFeeder feeder, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateStripFeederAsync(StripFeeder feeder, EntityHeader org, EntityHeader user);
        Task<ListResponse<StripFeederSummary>> GetStripFeedersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<StripFeeder> GetStripFeederAsync(string id, bool loadComponent, EntityHeader org, EntityHeader user);
        Task<ListResponse<StripFeeder>> GetStripFeedersForMachineAsync(string machineId, bool loadComponent, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteStripFeederAsycn(string id, EntityHeader org, EntityHeader user);
        Task<StripFeeder> CreateFromTemplateAsync(string templateId, EntityHeader org, EntityHeader user);

    }
}
