using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using System;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Managers
{
    /// <summary>
    /// Durable evidence only. Neither this manager nor its repository can
    /// write StripFeeder, StripFeederRow, or machine calibration settings.
    /// </summary>
    public class MachineOperationRunManager : ManagerBase, IMachineOperationRunManager
    {
        private readonly IMachineOperationRunRepo _repo;
        public MachineOperationRunManager(IMachineOperationRunRepo repo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depManager, ISecurity security)
            : base(logger, appConfig, depManager, security)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<InvokeResult> SaveSnapshotAsync(MachineOperationRunSnapshot snapshot,
            MachineOperationDefinition definition, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(MachineOperationRunRecord));
            var existing = await _repo.FindAsync(snapshot?.RunId);
            var record = MachineOperationRunProjection.Project(snapshot, definition, org, user, existing);
            if (ReferenceEquals(record, existing))
                return InvokeResult.Success;

            // If this write fails, it fails only history recording; it cannot
            // claim to have committed a physical feeder setting.
            await _repo.UpsertAsync(record);
            return InvokeResult.Success;
        }

        public async Task<MachineOperationRunRecord> GetAsync(string runId,
            EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(MachineOperationRunRecord));
            var record = await _repo.FindAsync(runId);
            if (record == null || record.OwnerOrganization?.Id != org.Id)
                throw new InvalidOperationException("Operation run not found for organization.");
            return record;
        }

        public async Task<ListResponse<MachineOperationRunSummary>> ListAsync(
            string machineId, string feederId, ListRequest request, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(MachineOperationRunRecord));
            if (String.IsNullOrWhiteSpace(machineId))
                throw new ArgumentException("Machine id is required.", nameof(machineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            request.PageSize = Math.Max(1, Math.Min(request.PageSize, 50));
            return await _repo.ListAsync(org.Id, machineId, feederId, request);
        }
    }
}
