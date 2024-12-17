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
    public class StripFeederRepo : DocumentDBRepoBase<StripFeeder>, IStripFeederRepo
    {
        private bool _shouldConsolidateCollections;

        public StripFeederRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddStripFeederAsync(StripFeeder stripFeeder)
        {
            if (stripFeeder.Component != null) stripFeeder.Component.Value = null;
            if (stripFeeder.PcbComponent != null) stripFeeder.PcbComponent.Value = null;
            if (stripFeeder.Package != null) stripFeeder.Package = null;

            return CreateDocumentAsync(stripFeeder);
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

        public Task UpdateStripFeederAsync(StripFeeder stripFeeder)
        {
            if (stripFeeder.Component != null) stripFeeder.Component.Value = null;
            if (stripFeeder.PcbComponent != null) stripFeeder.PcbComponent.Value = null;
            if (stripFeeder.Package != null) stripFeeder.Package = null;

            return UpsertDocumentAsync(stripFeeder);
        }
    }
}
