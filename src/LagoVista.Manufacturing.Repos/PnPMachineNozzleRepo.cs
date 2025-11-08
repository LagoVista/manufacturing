// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8ac9311de409e8b8c6cc50054e62125bd55588fc2251058702d9b5c61a4285b0
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

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class PnPMachineNozzleTipRepo : DocumentDBRepoBase<PnPMachineNozzleTip>, IPnpMachineNozzleTipReo
    {
        private bool _shouldConsolidateCollections;

        public PnPMachineNozzleTipRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddPnPMachineNozzleTipAsync(PnPMachineNozzleTip pnPMachineNozzleTip)
        {
            return CreateDocumentAsync(pnPMachineNozzleTip);
        }

        public Task DeletePnPMachineNozzleTipAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<PnPMachineNozzleTip> GetPnPMachineNozzleTipAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<PnPMachineNozzleTipSummary>> GetPnPMachineNozzleTipSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<PnPMachineNozzleTipSummary, PnPMachineNozzleTip>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdatePnPMachineNozzleTipAsync(PnPMachineNozzleTip pnPMachineNozzleTip)
        {
            return UpsertDocumentAsync(pnPMachineNozzleTip);
        }

    }
}
