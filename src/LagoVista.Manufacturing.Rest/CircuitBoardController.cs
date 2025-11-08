// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: afbb02f741c5798ee418126cb90070bd03da7e332079f7545d99cec6e6bd62db
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using System;
using System.Linq;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class CircuitBoardController : LagoVistaBaseController
    {
        private readonly ICircuitBoardManager _mgr;
        private readonly IPickAndPlaceJobManager _pnpJobManager;

        public CircuitBoardController(UserManager<AppUser> userManager, IAdminLogger logger, ICircuitBoardManager mgr, IPickAndPlaceJobManager pnpJobManager) : base(userManager, logger)
        {
            _mgr = mgr ?? throw new ArgumentNullException(nameof(mgr));
            _pnpJobManager = pnpJobManager ?? throw new ArgumentNullException(nameof(pnpJobManager));
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
        public DetailResponse<CircuitBoardVariant> CreateCircuitVariantBoard()
        {
            return DetailResponse<CircuitBoardVariant>.Create();
        }

        [HttpDelete("/api/mfg/pcb/{id}")]
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

        [HttpGet("/api/mfg/pcbs")]
        public Task<ListResponse<CircuitBoardSummary>> GetEquomentForOrg()
        {
            return _mgr.GetCircuitBoardsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pcb/{id}/revision/{revid}/job")]
        public async Task<InvokeResult<string>> CreatePNPJob(string id, string revid, string name)
        {
            var brd = await _mgr.GetCircuitBoardAsync(id, OrgEntityHeader, UserEntityHeader);
            var rev = brd.Revisions.SingleOrDefault(rev => rev.Id == revid);
            if (rev == null)
                return InvokeResult<string>.FromError("Could not create board revision.");

            var job = new PickAndPlaceJob();
            SetOwnedProperties(job);
            SetAuditProperties(job);
            var stamp = DateTime.Now;
            job.Name = name ?? $"{brd.Name} {brd.Revision} {stamp.Year}-{stamp.Month}-{stamp.Day} {stamp.Hour}:{stamp.Minute}";
            if (brd.Key.Length > 16)
                job.Key = $"{brd.Key.Substring(0, 16)}{stamp.Year}{stamp.Month}{stamp.Day}{stamp.Hour}{stamp.Minute}";
            else
                job.Key = $"{brd.Key}{stamp.Year}{stamp.Month}{stamp.Day}{stamp.Hour}{stamp.Minute}";
          
            job.Board = brd.ToEntityHeader();
            job.BoardRevision = rev;

            await _pnpJobManager.AddPickAndPlaceJobAsync(job, OrgEntityHeader, UserEntityHeader);

            return InvokeResult<string>.Create(job.Id);
        }
    }
}
