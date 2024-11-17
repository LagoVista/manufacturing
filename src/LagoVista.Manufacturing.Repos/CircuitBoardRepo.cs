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
    public class CircuitBoardRepo : DocumentDBRepoBase<CircuitBoard>, ICircuitBoardRepo
    {
        private bool _shouldConsolidateCollections;

        public CircuitBoardRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddCircuitBoardAsync(CircuitBoard CircuitBoard)
        {
            return CreateDocumentAsync(CircuitBoard);
        }

        public Task DeleteCircuitBoardAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<CircuitBoard> GetCircuitBoardAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<CircuitBoardSummary>> GetCircuitBoardSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<CircuitBoardSummary, CircuitBoard>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateCircuitBoardAsync(CircuitBoard CircuitBoard)
        {
            return UpsertDocumentAsync(CircuitBoard);
        }

    }
}
