// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 52a98da21f862f201de4f9ba09c4f90d04ae06269b8b44beec6b44981e2644da
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Generic;
using LagoVista.PCB.Eagle.Models;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IComponentPackageManager
    {
        Task<InvokeResult> AddComponentPackageAsync(ComponentPackage componentPackage, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateComponentPackageAsync(ComponentPackage componentPackage, EntityHeader org, EntityHeader user);
        Task<ListResponse<ComponentPackageSummary>> GetComponentPackagesSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ComponentPackage> GetComponentPackageAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCommponentPackageAsync(string id, EntityHeader org, EntityHeader user);
        Task<XDocument> GenerateOpenPnPPackagesForAllComponentPackagesAsync(EntityHeader org, EntityHeader user);
        Task<InvokeResult> SetLayoutAsync(string componentId, PcbPackage package, EntityHeader org, EntityHeader user);
    }
}
