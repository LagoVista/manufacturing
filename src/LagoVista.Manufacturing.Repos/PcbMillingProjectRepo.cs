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
    public class PcbMillingProjectRepo : DocumentDBRepoBase<PcbMillingProject>, IPcbMillingProjectRepo
    {
        private bool _shouldConsolidateCollections;

        public PcbMillingProjectRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddPcbMillingProjectAsync(PcbMillingProject PcbMillingProject)
        {
            return CreateDocumentAsync(PcbMillingProject);
        }

        public Task DeletePcbMillingProjectAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<PcbMillingProject> GetPcbMillingProjectAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<PcbMillingProjectSummary>> GetPcbMillingProjectSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<PcbMillingProjectSummary, PcbMillingProject>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdatePcbMillingProjectAsync(PcbMillingProject PcbMillingProject)
        {
            return UpsertDocumentAsync(PcbMillingProject);
        }
    }
}
