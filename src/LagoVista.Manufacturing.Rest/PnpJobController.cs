// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d23caba2c4c38d67477de522af65ba09189d9235e0e1ed6c285ee87ac960ba59
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using LagoVista.Core.Interfaces;
using System;

namespace LagoVista.Manufacturing.Rest.Controllers
{

    [Authorize]
    public class PickAndPlaceJobController : LagoVistaBaseController
    {
        private readonly IPickAndPlaceJobManager _pnpJobManager;
        private readonly ISerialNumberManager _serialNumberManager;

        public PickAndPlaceJobController(UserManager<AppUser> userManager, ISerialNumberManager serialNumberManager, IAdminLogger logger, IPickAndPlaceJobManager pnpJobManager) : base(userManager, logger)
        {
            _pnpJobManager = pnpJobManager ?? throw new ArgumentNullException(nameof(pnpJobManager));
            _serialNumberManager = serialNumberManager ?? throw new ArgumentNullException(nameof(serialNumberManager));
        }

        [HttpGet("/api/mfg/pnpjob/{id}")]
        public async Task<DetailResponse<PickAndPlaceJob>> GetPickAndPlaceJob(string id)
        {
            return DetailResponse<PickAndPlaceJob>.Create(await _pnpJobManager.GetPickAndPlaceJobAsync(id, OrgEntityHeader, UserEntityHeader));
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
            return await _pnpJobManager.DeletePickAndPlaceJobAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/pnpjob")]
        public Task<InvokeResult> AddPickAndPlaceJobAsync([FromBody] PickAndPlaceJob PickAndPlaceJob)
        {
            return _pnpJobManager.AddPickAndPlaceJobAsync(PickAndPlaceJob, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/pnpjob")]
        public Task<InvokeResult> UpdatePickAndPlaceJob([FromBody] PickAndPlaceJob PickAndPlaceJob)
        {
            SetUpdatedProperties(PickAndPlaceJob);
            return _pnpJobManager.UpdatePickAndPlaceJobAsync(PickAndPlaceJob, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pnpjobs")]
        public Task<ListResponse<PickAndPlaceJobSummary>> GetPnPJobs()
        {
            return _pnpJobManager.GetPickAndPlaceJobSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pnpjob/run/{id}")]
        public async Task<DetailResponse<PickAndPlaceJobRun>> GetPickAndPlaceJobRun(string id)
        {
            return DetailResponse<PickAndPlaceJobRun>.Create(await _pnpJobManager.GetPickAndPlaceJobRunAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/pnpjob/{jobid}/run/factory")]
        public async Task<DetailResponse<PickAndPlaceJobRun>> CreatePickAndPlaceJobRun(string jobid)
        {
            var job = await _pnpJobManager.GetPickAndPlaceJobAsync(jobid, OrgEntityHeader, UserEntityHeader);

            var form = DetailResponse<PickAndPlaceJobRun>.Create();
            form.Model.SerialNumber = await _serialNumberManager.GenerateSerialNumber(OrgEntityHeader.Id, job.Key);
            form.Model.Name = $"{job.Name} ({form.Model.SerialNumber})";
            form.Model.Key = $"{job.Key}{form.Model.SerialNumber}";
            form.Model.Job = job.ToEntityHeader();
            form.Model.RanBy = UserEntityHeader;
            form.Model.Cost = job.Cost;
            form.Model.Extended = job.Extended;
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            await _pnpJobManager.AddPickAndPlaceJobRunAsync(form.Model, OrgEntityHeader, UserEntityHeader);
            return form;
        }

        [HttpPost("/api/mfg/pnpjob/run")]
        public Task<InvokeResult> AddPickAndPlaceJobRunAsync([FromBody] PickAndPlaceJobRun run)
        {
            return _pnpJobManager.AddPickAndPlaceJobRunAsync(run, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/pnpjob/run")]
        public Task<InvokeResult> UpdatePickAndPlaceJobRun([FromBody] PickAndPlaceJobRun run)
        {
            SetUpdatedProperties(run);
            return _pnpJobManager.UpdatePickAndPlaceJobRunAsync(run, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/pmpjob/run/{id}/placement")]
        public Task<InvokeResult> UpdatePickAndPlaceJobRun(string id, [FromBody] PickAndPlaceJobRunPlacement placement)
        {
            return _pnpJobManager.UpdatePickAndPlaceJobRunPlacementAsync(id, placement, OrgEntityHeader, UserEntityHeader);
        }


        [HttpGet("/api/mfg/pnpjob/runs")]
        public Task<ListResponse<PickAndPlaceJobSummary>> GetPnPJobRunss()
        {
            return _pnpJobManager.GetPickAndPlaceJobSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }
    }
}
