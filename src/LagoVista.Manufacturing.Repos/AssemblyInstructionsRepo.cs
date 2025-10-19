// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a1eaf3d6135ced4a9d54a26492a9fa0bf9d62977955853f517d19d6dcc777b39
// IndexVersion: 0
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
    public class AssemblyInstructionRepo : DocumentDBRepoBase<AssemblyInstruction>, IAssemblyInstructionRepo
    {
        private bool _shouldConsolidateCollections;

        public AssemblyInstructionRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddAssemblyInstructionAsync(AssemblyInstruction assemblyInstruction)
        {
            return CreateDocumentAsync(assemblyInstruction);
        }

        public Task DeleteAssemblyInstructionAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<AssemblyInstruction> GetAssemblyInstructionAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<AssemblyInstructionSummary>> GetAssemblyInstructionSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<AssemblyInstructionSummary, AssemblyInstruction>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateAssemblyInstructionAsync(AssemblyInstruction assemblyInstruction)
        {
            return UpsertDocumentAsync(assemblyInstruction);
        }
    }
}
