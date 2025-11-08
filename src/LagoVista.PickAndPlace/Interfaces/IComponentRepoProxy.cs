// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3da3e244992cd381d520c77624c931f554e0324e4a8f34f0c5fbbe9943c3d3a7
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IComponentRepoProxy
    {
        Task<List<ComponentPackageSummary>> GetPackagesAsync();
        Task<ComponentPackage> GetPackageAsync(string id);
        Task<InvokeResult> AddPackageAsync(ComponentPackage package);
        Task<InvokeResult> UpdatePackageAsync(ComponentPackage package);
    }
}
