// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0145d6fcdee84d0855af3c5c60ff144bab726b820b846e5f05936445ba43c931
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Repos
{
    public class ComponentOrderRepo : DocumentDBRepoBase<ComponentOrder>, IComponentOrderRepo
    {
        private bool _shouldConsolidateCollections;

        public ComponentOrderRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddComponentOrderAsync(ComponentOrder componentOrder)
        {
            return CreateDocumentAsync(componentOrder);
        }

        public Task DeleteComponentOrderAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<ComponentOrder> GetComponentOrderAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<ComponentOrderSummary>> GetComponentOrderSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<ComponentOrderSummary, ComponentOrder>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateComponentOrderAsync(ComponentOrder componentOrder)
        {
            return UpsertDocumentAsync(componentOrder);
        }

    }
}
