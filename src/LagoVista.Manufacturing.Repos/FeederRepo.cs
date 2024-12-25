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
    public class FeederRepo : DocumentDBRepoBase<Feeder>, IFeederRepo
    {
        private bool _shouldConsolidateCollections;

        public FeederRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddFeederAsync(Feeder Feeder)
        {
            return CreateDocumentAsync(Feeder);
        }

        public Task DeleteFeederAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<Feeder> GetFeederAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<ListResponse<Feeder>> GetFeedersForMachineAsync(string machineId)
        {
            return  ListResponse<Feeder>.Create(await  base.QueryAsync(fdr => fdr.Machine.Id == machineId));
        }

        public Task<ListResponse<FeederSummary>> GetFeederSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<FeederSummary, Feeder>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateFeederAsync(Feeder Feeder)
        {
            return UpsertDocumentAsync(Feeder);
        }

    }
}
