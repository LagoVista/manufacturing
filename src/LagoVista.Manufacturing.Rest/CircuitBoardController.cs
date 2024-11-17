using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Web.Common.Attributes;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class CircuitBoardController : LagoVistaBaseController
    {
        private readonly ICircuitBoardManager _mgr;

        public CircuitBoardController(UserManager<AppUser> userManager, IAdminLogger logger, ICircuitBoardManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/pcb/{id}")]
        public async Task<DetailResponse<CircuitBoard>> GetCircuitBoard(string id)
        {
            return DetailResponse<CircuitBoard>.Create(await _mgr.GetCircuitBoardAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/pcb/factory")]
        public DetailResponse<CircuitBoard> CreateCircuitBoard()
        {
            var form = DetailResponse<CircuitBoard>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpGet("/api/mfg/pcb/revision/factory")]
        public DetailResponse<CircuitBoardRevision> CreateCircuitRevisionBoard()
        {
            return DetailResponse<CircuitBoardRevision>.Create();
        }

        [HttpGet("/api/mfg/pcb/variant/factory")]
        public DetailResponse<CircuitBoardVarient> CreateCircuitVariantBoard()
        {
            return DetailResponse<CircuitBoardVarient>.Create();
        }

        [HttpDelete("/api/mfg/CircuitBoard/{id}")]
        public async Task<InvokeResult> DeleteCircuitBoard(string id)
        {
            return await _mgr.DeleteCommponentAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/pcb")]
        public Task<InvokeResult> AddCircuitBoardPackageAsync([FromBody] CircuitBoard CircuitBoard)
        {
            return _mgr.AddCircuitBoardAsync(CircuitBoard, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/pcb")]
        public Task<InvokeResult> UpdateCircuitBoardPackage([FromBody] CircuitBoard CircuitBoard)
        {
            SetUpdatedProperties(CircuitBoard);
            return _mgr.UpdateCircuitBoardAsync(CircuitBoard, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pcb")]
        public Task<ListResponse<CircuitBoardSummary>> GetEquomentForOrg()
        {
            return _mgr.GetCircuitBoardsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
