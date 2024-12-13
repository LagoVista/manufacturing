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
    public class PnPMachineNozzleTipController : LagoVistaBaseController
    {
        private readonly IPnPMachineNozzleTipManager _mgr;

        public PnPMachineNozzleTipController(UserManager<AppUser> userManager, IAdminLogger logger, IPnPMachineNozzleTipManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/pnp/nozzletip/{id}")]
        public async Task<DetailResponse<PnPMachineNozzleTip>> GetPnPMachineNozzleTip(string id)
        {
            return DetailResponse<PnPMachineNozzleTip>.Create(await _mgr.GetPnPMachineNozzleTipAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/pnp/nozzletip/factory")]
        public DetailResponse<PnPMachineNozzleTip> CreatePnPMachineNozzleTip()
        {
            var form = DetailResponse<PnPMachineNozzleTip>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/pnp/nozzletip/{id}")]
        public async Task<InvokeResult> DeletePnPMachineNozzleTip(string id)
        {
            return await _mgr.DeleteCommponentAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/pnp/nozzletip")]
        public Task<InvokeResult> AddPnPMachineNozzleTipPackageAsync([FromBody] PnPMachineNozzleTip nozzleTip)
        {
            return _mgr.AddPnPMachineNozzleTipAsync(nozzleTip, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/pnp/nozzletip")]
        public Task<InvokeResult> UpdatePnPMachineNozzleTipPackage([FromBody] PnPMachineNozzleTip nozzleTip)
        {
            SetUpdatedProperties(nozzleTip);
            return _mgr.UpdatePnPMachineNozzleTipAsync(nozzleTip, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pnp/nozzletips")]
        public Task<ListResponse<PnPMachineNozzleTipSummary>> GetNozzleTipsForOrg()
        {
            return _mgr.GetPnPMachineNozzleTipsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }
    }
}
