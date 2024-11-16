using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.CloudRepos;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.IoT.Logging.Loggers;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Models;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class ComponentPackageRepo : DocumentDBRepoBase<ComponentPackage>, IComponentPackageRepo
    {
        private bool _shouldConsolidateCollections;

        public ComponentPackageRepo(IDeviceRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.DeviceDocDbStorage.Uri, settings.DeviceDocDbStorage.AccessKey, settings.DeviceDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }
        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddComponentPackageAsync(ComponentPackage package)
        {
            return CreateDocumentAsync(package);
        }

        public Task DeleteCommponentPackageAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<ComponentPackage> GetComponentPackageAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<ComponentPackageSummary>> GetComponentPackagesSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<ComponentPackageSummary, ComponentPackage>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateComponentPackageAsync(ComponentPackage package)
        {
            return UpsertDocumentAsync(package);
        }
    }
}
