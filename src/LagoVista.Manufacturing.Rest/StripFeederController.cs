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
    public class StripFeederController : LagoVistaBaseController
    {
        private readonly IStripFeederManager _mgr;
        private readonly IMachineManager _machineManager;

        public StripFeederController(UserManager<AppUser> userManager, IAdminLogger logger,  IStripFeederManager mgr, IMachineManager machineManager) : 
            base(userManager, logger)
        {
            _mgr = mgr;
            _machineManager = machineManager;
        }

        [HttpGet("/api/mfg/stripfeeder/{id}")]
        public async Task<DetailResponse<StripFeeder>> GetStripFeeder(string id, bool loadcomponent = false)
        {
            return DetailResponse<StripFeeder>.Create(await _mgr.GetStripFeederAsync(id, loadcomponent, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/stripfeeder/factory")]
        public DetailResponse<StripFeeder> CreateStripFeeder()
        {
            var form = DetailResponse<StripFeeder>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpGet("/api/mfg/stripfeeder/row/factory")]
        public DetailResponse<StripFeederRow> CreateStripFeederFow()
        {
            return DetailResponse<StripFeederRow>.Create();
        }

        [HttpDelete("/api/mfg/stripfeeder/{id}")]
        public async Task<InvokeResult> DeleteStripFeeder(string id)
        {
            return await _mgr.DeleteStripFeederAsycn(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/stripfeeder")]
        public Task<InvokeResult> AddStripFeederPackageAsync([FromBody] StripFeeder feeder)
        {
            return _mgr.AddStripFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/stripfeeder")]
        public Task<InvokeResult> UpdateStripFeederPackage([FromBody] StripFeeder feeder)
        {
            SetUpdatedProperties(feeder);
            return _mgr.UpdateStripFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/stripfeeders")]
        public Task<ListResponse<StripFeederSummary>> GetStripFeedersForOrg()
        {
            return _mgr.GetStripFeedersSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/machine/{machineid}/stripfeeders")]
        public Task<ListResponse<StripFeeder>> GetStripFeedersForMachine(string machineid, bool loadcomponents)
        {
            return _mgr.GetStripFeedersForMachineAsync(machineid, loadcomponents, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/mft/machine/{machineid}/stripfeeder/{feederid}/attach")]
        public async Task<InvokeResult> AttachToMachine(string machineid, string feederid)
        {
            var machine = await _machineManager.GetMachineAsync(machineid, OrgEntityHeader, UserEntityHeader);
            var feeder = await _mgr.GetStripFeederAsync(feederid, false, OrgEntityHeader, UserEntityHeader);
            feeder.Machine = machine.ToEntityHeader();
            return await _mgr.UpdateStripFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }
    }
}
