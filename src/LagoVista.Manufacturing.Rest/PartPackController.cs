// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 260245a8ff29c69ebf759028e04eebcc5878659250660c8714e1158748e88945
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

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class PartPackController : LagoVistaBaseController
    {
        private readonly IPartPackManager _mgr;

        public PartPackController(UserManager<AppUser> userManager, IAdminLogger logger, IPartPackManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/PartPack/{id}")]
        public async Task<DetailResponse<PartPack>> GetPartPack(string id)
        {
            return DetailResponse<PartPack>.Create(await _mgr.GetPartPackAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/PartPack/factory")]
        public DetailResponse<PartPack> CreatePartPack()
        {
            var form = DetailResponse<PartPack>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }
        
        [HttpDelete("/api/mfg/PartPack/{id}")]
        public async Task<InvokeResult> DeletePartPack(string id)
        {
            return await _mgr.DeleteCommponentAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/PartPack")]
        public Task<InvokeResult> AddPartPackPackageAsync([FromBody] PartPack PartPack)
        {
            return _mgr.AddPartPackAsync(PartPack, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/PartPack")]
        public Task<InvokeResult> UpdatePartPackPackage([FromBody] PartPack partPack)
        {
            SetUpdatedProperties(partPack);
            return _mgr.UpdatePartPackAsync(partPack, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/PartPacks")]
        public Task<ListResponse<PartPackSummary>> GetEquomentForOrg()
        {
            return _mgr.GetPartPacksSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
