// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 93a81fcfca0ba01f7f441409992e32bc6aa2c5772d73d67dfea7b89121836489
// IndexVersion: 0
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
using LagoVista.Manufacturing.Managers;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class AutoFeederController : LagoVistaBaseController
    {
        private readonly IAutoFeederManager _mgr;
        private readonly IMachineManager _machineManager;

        public AutoFeederController(UserManager<AppUser> userManager, IAdminLogger logger, IMachineManager machineManager, IAutoFeederManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
            _machineManager = machineManager;
        }

        [HttpGet("/api/mfg/autofeeder/{id}")]
        public async Task<DetailResponse<AutoFeeder>> GetFeeder(string id, bool loadcomponent = false)
        {
            return DetailResponse<AutoFeeder>.Create(await _mgr.GetFeederAsync(id, loadcomponent, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/autofeeder/feederid/{id}")]
        public async Task<DetailResponse<AutoFeeder>> GetFeederByFeederId(string id, bool loadcomponent = false)
        {
            return DetailResponse<AutoFeeder>.Create(await _mgr.GetFeederByFeederIdAsync(id, loadcomponent, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/autofeeder/factory")]
        public DetailResponse<AutoFeeder> CreateFeeder()
        {
            var form = DetailResponse<AutoFeeder>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpGet("/api/mfg/autofeeder/template/{templateid}/factory")]
        public async Task<DetailResponse<AutoFeeder>> CreateFromTemplate(string templateid)
        {
            var feeder = await _mgr.CreateFromTemplateAsync(templateid, OrgEntityHeader, UserEntityHeader);
            var form = DetailResponse<AutoFeeder>.Create(feeder, false);
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/autofeeder/{id}")]
        public async Task<InvokeResult> DeleteFeeder(string id)
        {
            return await _mgr.DeleteFeederAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/autofeeder")]
        public Task<InvokeResult> AddFeederPackageAsync([FromBody] AutoFeeder feeder)
        {
            return _mgr.AddFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/autofeeder")]
        public Task<InvokeResult> UpdateFeederPackage([FromBody] AutoFeeder feeder)
        {
            SetUpdatedProperties(feeder);
            return _mgr.UpdateFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/autofeeders")]
        public Task<ListResponse<AutoFeederSummary>> GetFeedersForOrg()
        {
            return _mgr.GetFeedersSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/machine/{machineid}/autofeeders")]
        public Task<ListResponse<AutoFeeder>> GetFeedersForMachine(string machineid, bool loadcomponents)
        {
            return _mgr.GetFeedersForMachineAsync(machineid, loadcomponents, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/autofeeder/{id}/partcount/{partcount}")]
        public async Task<InvokeResult> UpdatePartCount(string id, int partcount)
        {
            var feeder = await _mgr.GetFeederAsync(id, false, OrgEntityHeader, UserEntityHeader);
            feeder.PartCount = partcount;
            return await _mgr.UpdateFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/mft/machine/{machineid}/autofeeder/{feederid}/attach")]
        public async Task<InvokeResult> AttachToMachine(string machineid, string feederid)
        {
            var machine = await _machineManager.GetMachineAsync(machineid, OrgEntityHeader, UserEntityHeader);
            var feeder = await _mgr.GetFeederAsync(feederid, false, OrgEntityHeader, UserEntityHeader);
            feeder.Machine = machine.ToEntityHeader();
            return await _mgr.UpdateFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

    }
}
