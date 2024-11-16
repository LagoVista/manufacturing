using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.CloudRepos;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class FeederRepo : DocumentDBRepoBase<Feeder>, IFeederRepo
    {
        private bool _shouldConsolidateCollections;

        public FeederRepo(IDeviceRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.DeviceDocDbStorage.Uri, settings.DeviceDocDbStorage.AccessKey, settings.DeviceDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
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
