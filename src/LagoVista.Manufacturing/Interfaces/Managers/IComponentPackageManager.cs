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
