// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3ac0d8b830d4fefffa9b7852c0b09ff4878ed7ce61a6e6b7e589351bdde847e9
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
    public class PartPackRepo : DocumentDBRepoBase<PartPack>, IPartPackRepo
    {
        private bool _shouldConsolidateCollections;

        public PartPackRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddPartPackAsync(PartPack PartPack)
        {
            return CreateDocumentAsync(PartPack);
        }

        public Task DeletePartPackAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<PartPack> GetPartPackAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<PartPackSummary>> GetPartPackSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<PartPackSummary, PartPack>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdatePartPackAsync(PartPack PartPack)
        {
            return UpsertDocumentAsync(PartPack);
        }
    }
}
