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
        Task<InvokeResult> DeleteStripFeederAsycn(string id, EntityHeader org, EntityHeader user);

    }
}
