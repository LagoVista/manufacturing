using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Web.Common.Attributes;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [Authorize]
    [ConfirmedUser]
    public class MachineOperationRunController : LagoVistaBaseController
    {
        private readonly IMachineOperationRunManager _manager;
        public MachineOperationRunController(UserManager<AppUser> userManager,
            IAdminLogger logger, IMachineOperationRunManager manager)
            : base(userManager, logger) => _manager = manager;

        // The authenticated organization/user, not caller-provided record
        // ownership or "Saved" flags, controls durable history access.
        [HttpPut("/api/mfg/calibration/run")]
        public Task<InvokeResult> SaveRun([FromBody] MachineOperationRunWriteRequest request) =>
            _manager.SaveSnapshotAsync(request?.Snapshot, request?.Definition,
                OrgEntityHeader, UserEntityHeader);

        [McpEndpoint("manufacturing-calibration-run-get",
            Description = "Retrieve the recorded steps, evidence, timestamps and errors for a machine calibration run.",
            FamilyId = "manufacturing")]
        [HttpGet("/api/mfg/calibration/run/{runId}")]
        public Task<MachineOperationRunRecord> GetRun(string runId) =>
            _manager.GetAsync(runId, OrgEntityHeader, UserEntityHeader);

        [McpEndpoint("manufacturing-calibration-runs-list",
            Description = "List a bounded page of calibration run history by machine and optional feeder.",
            FamilyId = "manufacturing")]
        [HttpGet("/api/mfg/calibration/runs/machine/{machineId}")]
        public Task<ListResponse<MachineOperationRunSummary>> ListRuns(string machineId, string feederId = null)
            => _manager.ListAsync(machineId, feederId, GetListRequestFromHeader(),
                OrgEntityHeader, UserEntityHeader);
    }

    public class MachineOperationRunWriteRequest
    {
        public MachineOperationRunSnapshot Snapshot { get; set; }
        public MachineOperationDefinition Definition { get; set; }
    }
}
