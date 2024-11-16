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
    public class StripFeederRepo : DocumentDBRepoBase<StripFeeder>, IStripFeederRepo
    {
        private bool _shouldConsolidateCollections;

        public StripFeederRepo(IDeviceRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.DeviceDocDbStorage.Uri, settings.DeviceDocDbStorage.AccessKey, settings.DeviceDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddStripFeederAsync(StripFeeder StripFeeder)
        {
            return CreateDocumentAsync(StripFeeder);
        }

        public Task DeleteStripFeederAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<StripFeeder> GetStripFeederAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<StripFeederSummary>> GetStripFeederSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<StripFeederSummary, StripFeeder>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateStripFeederAsync(StripFeeder StripFeeder)
        {
            return UpsertDocumentAsync(StripFeeder);
        }

    }
}
