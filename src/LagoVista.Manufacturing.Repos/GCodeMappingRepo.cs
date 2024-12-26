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
    public class GCodeMappingRepo : DocumentDBRepoBase<GCodeMapping>, IGCodeMappingRepo
    {
        private bool _shouldConsolidateCollections;

        public GCodeMappingRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddGCodeMappingAsync(GCodeMapping mapping)
        {
            return CreateDocumentAsync(mapping);
        }

        public Task DeleteGCodeMappingAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<GCodeMapping> GetGCodeMappingAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<GCodeMappingSummary>> GetGCodeMappingSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<GCodeMappingSummary, GCodeMapping>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateGCodeMappingAsync(GCodeMapping mapping)
        {
            return UpsertDocumentAsync(mapping);
        }

    }
}
