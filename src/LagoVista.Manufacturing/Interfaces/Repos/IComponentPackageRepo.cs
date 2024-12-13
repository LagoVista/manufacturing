using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IComponentPackageRepo
    {
        Task AddComponentPackageAsync(ComponentPackage package);
        Task UpdateComponentPackageAsync(ComponentPackage package);
        Task<ListResponse<ComponentPackageSummary>> GetComponentPackagesSummariesAsync(string id, ListRequest listRequest);
        Task<List<ComponentPackage>> GetFullPackagesAsync(string orgId);
        Task<ComponentPackage> GetComponentPackageAsync(string id);
        Task DeleteCommponentPackageAsync(string id);
    }
}
