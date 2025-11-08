// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9d355234e863184de7a092055e0809914c07d15ef1876b5d3af3971838108fbd
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.IoT.Logging.Loggers;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Repos;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class ComponentPackageRepo : DocumentDBRepoBase<ComponentPackage>, IComponentPackageRepo
    {
        private bool _shouldConsolidateCollections;

        public ComponentPackageRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
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

        public async Task<List<ComponentPackage>> GetFullPackagesAsync(string orgId)
        {
            return (await QueryAllAsync(cp=>cp.OwnerOrganization.Id == orgId, ListRequest.CreateForAll())).Model.ToList();
        }

        public Task UpdateComponentPackageAsync(ComponentPackage package)
        {
            return UpsertDocumentAsync(package);
        }
    }
}
