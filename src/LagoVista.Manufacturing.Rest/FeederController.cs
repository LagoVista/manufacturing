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
    public class FeederController : LagoVistaBaseController
    {
        private readonly IFeederManager _mgr;

        public FeederController(UserManager<AppUser> userManager, IAdminLogger logger, IFeederManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/Feeder/{id}")]
        public async Task<DetailResponse<Feeder>> GetFeeder(string id, bool loadcomponent = false)
        {
            return DetailResponse<Feeder>.Create(await _mgr.GetFeederAsync(id, loadcomponent, OrgEntityHeader, UserEntityHeader));
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
            return await _mgr.DeleteCommponentAsync(id, OrgEntityHeader, UserEntityHeader);
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

    }
}
