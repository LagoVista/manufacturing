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
using System.Security.Cryptography;

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
        public Task<InvokeResult> UpdatePickAndPlaceJob([FromBody] PickAndPlaceJob PickAndPlaceJob)
        {
            SetUpdatedProperties(PickAndPlaceJob);
            return _mgr.UpdatePickAndPlaceJobAsync(PickAndPlaceJob, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pnpjobs")]
        public Task<ListResponse<PickAndPlaceJobSummary>> GetPnPJobs()
        {
            return _mgr.GetPickAndPlaceJobSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pnpjob/run/{id}")]
        public async Task<DetailResponse<PickAndPlaceJobRun>> GetPickAndPlaceJobRun(string id)
        {
            return DetailResponse<PickAndPlaceJobRun>.Create(await _mgr.GetPickAndPlaceJobRunAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/pnpjob/{jobid}/run/factory")]
        public async Task<DetailResponse<PickAndPlaceJobRun>> CreatePickAndPlaceJobRun(string jobid)
        {
            var job = await _mgr.GetPickAndPlaceJobAsync(jobid, OrgEntityHeader, UserEntityHeader);

            var form = DetailResponse<PickAndPlaceJobRun>.Create();
            form.Model.Job = job.ToEntityHeader();
            form.Model.RanBy = UserEntityHeader;
            form.Model.Cost = job.Cost;
            form.Model.Extended = job.Extended;
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpPost("/api/mfg/pnpjob/run")]
        public Task<InvokeResult> AddPickAndPlaceJobRunAsync([FromBody] PickAndPlaceJobRun run)
        {
            return _mgr.AddPickAndPlaceJobRunAsync(run, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/pnpjob/run")]
        public Task<InvokeResult> UpdatePickAndPlaceJobRun([FromBody] PickAndPlaceJobRun run)
        {
            SetUpdatedProperties(run);
            return _mgr.UpdatePickAndPlaceJobRunAsync(run, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pnpjob/runs")]
        public Task<ListResponse<PickAndPlaceJobSummary>> GetPnPJobRunss()
        {
            return _mgr.GetPickAndPlaceJobSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }
    }
}
