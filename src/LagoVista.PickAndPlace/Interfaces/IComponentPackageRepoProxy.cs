// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b3e7d1d2dc9e298122f849adc3f551b824771f179c7505b1c2fd7882b914301b
// IndexVersion: 0
// --- END CODE INDEX META ---
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
