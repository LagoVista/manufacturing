// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: daf4d9b8f5551efb64633273f3480b883f2a2ab39cc0c55f35daeb7656dcfef1
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Web.Common.Attributes;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Models;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Rest
{
    [ConfirmedUser]
    [Authorize]
    public class StripFeederTemplateController : LagoVistaBaseController
    {
        private readonly IStripFeederTemplateManager _mgr;

        public StripFeederTemplateController(UserManager<AppUser> userManager, IAdminLogger logger, IStripFeederTemplateManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/stripfeeder/template/{id}")]
        public async Task<DetailResponse<StripFeederTemplate>> GetStripFeederTemplate(string id)
        {
            return DetailResponse<StripFeederTemplate>.Create(await _mgr.GetStripFeederTemplateAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/stripfeeder/template/factory")]
        public DetailResponse<StripFeederTemplate> CreateStripFeederTemplate()
        {
            var form = DetailResponse<StripFeederTemplate>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/stripfeeder/template/{id}")]
        public async Task<InvokeResult> DeleteStripFeederTemplate(string id)
        {
            return await _mgr.DeleteStripFeederTemplateAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/stripfeeder/template")]
        public Task<InvokeResult> AddStripFeederTemplatePackageAsync([FromBody] StripFeederTemplate mapping)
        {
            return _mgr.AddStripFeederTemplateAsync(mapping, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/stripfeeder/template")]
        public Task<InvokeResult> UpdateStripFeederTemplatePackage([FromBody] StripFeederTemplate mapping)
        {
            SetUpdatedProperties(mapping);
            return _mgr.UpdateStripFeederTemplateAsync(mapping, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/stripfeeder/templates")]
        public Task<ListResponse<StripFeederTemplateSummary>> GetStripFeederTemplatesForOrg()
        {
            return _mgr.GetStripFeederTemplateSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
