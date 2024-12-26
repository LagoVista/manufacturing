using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IFeederManager
    {
        Task<InvokeResult> AddFeederAsync(Feeder feeder, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateFeederAsync(Feeder feeder, EntityHeader org, EntityHeader user);
        Task<ListResponse<FeederSummary>> GetFeedersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<Feeder> GetFeederAsync(string id, bool loadComponent, EntityHeader org, EntityHeader user);
        Task<Feeder> GetFeederByFeederIdAsync(string feederId, bool loadComponent, EntityHeader org, EntityHeader user);
        Task<ListResponse<Feeder>> GetFeedersForMachineAsync(string machineId, bool loadComponent, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteFeederAsync(string id, EntityHeader org, EntityHeader user);

    }
}
