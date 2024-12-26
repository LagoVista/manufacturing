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
    public class FeederController : LagoVistaBaseController
    {
        private readonly IFeederManager _mgr;
        private readonly IMachineManager _machineManager;

        public FeederController(UserManager<AppUser> userManager, IAdminLogger logger, IMachineManager machineManager, IFeederManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
            _machineManager = machineManager;
        }

        [HttpGet("/api/mfg/feeder/{id}")]
        public async Task<DetailResponse<Feeder>> GetFeeder(string id, bool loadcomponent = false)
        {
            return DetailResponse<Feeder>.Create(await _mgr.GetFeederAsync(id, loadcomponent, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/feeder/feederid/{id}")]
        public async Task<DetailResponse<Feeder>> GetFeederByFeederId(string id, bool loadcomponent = false)
        {
            return DetailResponse<Feeder>.Create(await _mgr.GetFeederByFeederIdAsync(id, loadcomponent, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/Feeder/factory")]
        public DetailResponse<Feeder> CreateFeeder()
        {
            var form = DetailResponse<Feeder>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/Feeder/{id}")]
        public async Task<InvokeResult> DeleteFeeder(string id)
        {
            return await _mgr.DeleteFeederAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/Feeder")]
        public Task<InvokeResult> AddFeederPackageAsync([FromBody] Feeder feeder)
        {
            return _mgr.AddFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/Feeder")]
        public Task<InvokeResult> UpdateFeederPackage([FromBody] Feeder feeder)
        {
            SetUpdatedProperties(feeder);
            return _mgr.UpdateFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/Feeders")]
        public Task<ListResponse<FeederSummary>> GetFeedersForOrg()
        {
            return _mgr.GetFeedersSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/machine/{machineid}/feeders")]
        public Task<ListResponse<Feeder>> GetFeedersForMachine(string machineid, bool loadcomponents)
        {
            return _mgr.GetFeedersForMachineAsync(machineid, loadcomponents, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/mft/machine/{machineid}/feeder/{feederid}/attach")]
        public async Task<InvokeResult> AttachToMachine(string machineid, string feederid)
        {
            var machine = await _machineManager.GetMachineAsync(machineid, OrgEntityHeader, UserEntityHeader);
            var feeder = await _mgr.GetFeederAsync(feederid, false, OrgEntityHeader, UserEntityHeader);
            feeder.Machine = machine.ToEntityHeader();
            return await _mgr.UpdateFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

    }
}
