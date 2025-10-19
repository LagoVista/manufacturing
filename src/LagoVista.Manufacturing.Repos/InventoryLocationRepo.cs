// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0516773a551a277fd63417536744fc8e6de868ea861c32a32a90e724675dad49
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Repos;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class InventoryLocationRepo : DocumentDBRepoBase<InventoryLocation>, IInventoryLocationRepo
    {
        private bool _shouldConsolidateCollections;

        public InventoryLocationRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddInventoryLocationAsync(InventoryLocation inventoryLocation)
        {
            return CreateDocumentAsync(inventoryLocation);
        }

        public Task DeleteInventoryLocationAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<InventoryLocation> GetInventoryLocationAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<InventoryLocationSummary>> GetInventoryLocationSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<InventoryLocationSummary, InventoryLocation>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateInventoryLocationAsync(InventoryLocation inventoryLocation)
        {
            return UpsertDocumentAsync(inventoryLocation);
        }
    }
}
