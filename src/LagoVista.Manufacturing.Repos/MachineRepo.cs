// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a3dab1edffafcb1882d47367df9c72691b9c3b9caa29a473f5eda5c52c078374
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
using LagoVista.IoT.Logging.Utils;
using System;
using LagoVista.Core.Models;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class MachineRepo : DocumentDBRepoBase<Machine>, IMachineRepo
    {
        private bool _shouldConsolidateCollections;

        public MachineRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddMachineAsync(Machine machine)
        {
            Console.WriteLine("==== Adding machine  ==>" + machine.Name);

            return CreateDocumentAsync(machine);
        }

        public Task DeleteMachineAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<Machine> GetMachineAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<MachineSummary>> GetMachineSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<MachineSummary, Machine>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateMachineAsync(Machine machine)
        {
            if (!EntityHeader.IsNullOrEmpty(machine.GcodeMapping))
                machine.GcodeMapping.Value = null;

            return UpsertDocumentAsync(machine);
        }

    }
}
