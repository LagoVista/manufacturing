// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d8908198fa63254d2dbac914b05065c1c11f8dc62b5f96f7f7c1d9a812698ae6
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
    public class PickAndPlaceJobRepo : DocumentDBRepoBase<PickAndPlaceJob>, IPickAndPlaceJobRepo
    {
        private bool _shouldConsolidateCollections;

        public PickAndPlaceJobRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddPickAndPlaceJobAsync(PickAndPlaceJob PickAndPlaceJob)
        {
            return CreateDocumentAsync(PickAndPlaceJob);
        }

        public Task DeletePickAndPlaceJobAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<PickAndPlaceJob> GetPickAndPlaceJobAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<PickAndPlaceJobSummary>> GetPickAndPlaceJobSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<PickAndPlaceJobSummary, PickAndPlaceJob>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdatePickAndPlaceJobAsync(PickAndPlaceJob PickAndPlaceJob)
        {
            return UpsertDocumentAsync(PickAndPlaceJob);
        }
    }
}
