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
    public class GCodeProjectRepo : DocumentDBRepoBase<GCodeProject>, IGCodeProjectRepo
    {
        private bool _shouldConsolidateCollections;

        public GCodeProjectRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddGCodeProjectAsync(GCodeProject GCodeProject)
        {
            return CreateDocumentAsync(GCodeProject);
        }

        public Task DeleteGCodeProjectAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<GCodeProject> GetGCodeProjectAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<GCodeProjectSummary>> GetGCodeProjectSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<GCodeProjectSummary, GCodeProject>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateGCodeProjectAsync(GCodeProject GCodeProject)
        {
            return UpsertDocumentAsync(GCodeProject);
        }
    }
}
