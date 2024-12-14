using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Rest.Controllers
{

    [Authorize]
    public class PickAndPlaceJobController : LagoVistaBaseController
    {
        private readonly IPickAndPlaceJobManager _mgr;

        public PickAndPlaceJobController(UserManager<AppUser> userManager, IAdminLogger logger, IPickAndPlaceJobManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/pnpjob/{id}")]
        public async Task<DetailResponse<PickAndPlaceJob>> GetPickAndPlaceJob(string id)
        {
            return DetailResponse<PickAndPlaceJob>.Create(await _mgr.GetPickAndPlaceJobAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/pnpjob/factory")]
        public DetailResponse<PickAndPlaceJob> CreatePickAndPlaceJob()
        {
            var form = DetailResponse<PickAndPlaceJob>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/pnpjob/{id}")]
        public async Task<InvokeResult> DeletePickAndPlaceJob(string id)
        {
            return await _mgr.DeletePickAndPlaceJobAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/pnpjob")]
        public Task<InvokeResult> AddPickAndPlaceJobAsync([FromBody] PickAndPlaceJob PickAndPlaceJob)
        {
            return _mgr.AddPickAndPlaceJobAsync(PickAndPlaceJob, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/pnpjob")]
        public Task<InvokeResult> UpdatePartPackPackage([FromBody] PickAndPlaceJob PickAndPlaceJob)
        {
            SetUpdatedProperties(PickAndPlaceJob);
            return _mgr.UpdatePickAndPlaceJobAsync(PickAndPlaceJob, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pnpjobs")]
        public Task<ListResponse<PickAndPlaceJobSummary>> GetPnPJobs()
        {
            return _mgr.GetPickAndPlaceJobSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }
    }
}
