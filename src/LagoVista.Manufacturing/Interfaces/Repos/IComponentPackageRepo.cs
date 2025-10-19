// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2f5a6858cabcc8746f41a3f8d2c5e7cb36956af9c49c75ce884365e5971e7cd2
// IndexVersion: 0
// --- END CODE INDEX META ---
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
