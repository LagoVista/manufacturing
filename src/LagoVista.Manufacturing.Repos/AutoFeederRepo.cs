// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3e7e52ae341e53ca4e9d0c3cf3762730b6b1cfc8fc933c2176629cd433ea66c6
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Repos;
using System.Linq;
using LagoVista.Core.Exceptions;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class AutoFeederRepo : DocumentDBRepoBase<AutoFeeder>, IAutoFeederRepo
    {
        private bool _shouldConsolidateCollections;

        public AutoFeederRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddFeederAsync(AutoFeeder Feeder)
        {
            return CreateDocumentAsync(Feeder);
        }

        public Task DeleteFeederAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<AutoFeeder> GetFeederAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<AutoFeeder> GetFeederByFeederIdAsync(string feederId)
        {
            var feeders = await base.QueryAsync(fdr => fdr.FeederId == feederId);
           
            if (!feeders.Any())
                throw new RecordNotFoundException(nameof(AutoFeeder), $"FeederId={feederId}");

            if(feeders.Count() > 1)
                throw new RecordNotFoundException(nameof(AutoFeeder), $"Multiple Records for FeederId={feederId}");

            return feeders.Single();
        }

        public async Task<ListResponse<AutoFeeder>> GetFeedersForMachineAsync(string machineId)
        {
            return  ListResponse<AutoFeeder>.Create(await base.QueryAsync(fdr => fdr.Machine.Id == machineId));
        }

        public Task<ListResponse<AutoFeederSummary>> GetFeederSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<AutoFeederSummary, AutoFeeder>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateFeederAsync(AutoFeeder Feeder)
        {
            if (Feeder.Component != null)
                Feeder.Component.Value = null;

            return UpsertDocumentAsync(Feeder);
        }

    }
}
