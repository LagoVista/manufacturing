using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IComponentPackageRepoProxy
    {
        Task<List<ComponentPackageSummary>> GetPackagesAsync();
        Task<ComponentPackage> GetPackageAsync(string id);
        Task<InvokeResult> AddPackageAsync(ComponentPackage package);
        Task<InvokeResult> UpdatePackageAsync(ComponentPackage package);
    }
}
