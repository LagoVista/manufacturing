// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 97464941295f2005ab33dda32e3ea8c8c0cb671cbf3cd3ed6a70c18653d93375
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Models;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Rest
{
    public class AutoFeederTemplateController : LagoVistaBaseController
    {
        private readonly IAutoFeederTemplateManager _mgr;

    public AutoFeederTemplateController(UserManager<AppUser> userManager, IAdminLogger logger, IAutoFeederTemplateManager mgr) : base(userManager, logger)
    {
        _mgr = mgr;
    }

    [HttpGet("/api/mfg/autofeeder/template/{id}")]
    public async Task<DetailResponse<AutoFeederTemplate>> GetAutoFeederTemplate(string id)
    {
        return DetailResponse<AutoFeederTemplate>.Create(await _mgr.GetAutoFeederTemplateAsync(id, OrgEntityHeader, UserEntityHeader));
    }

    [HttpGet("/api/mfg/autofeeder/template/factory")]
    public DetailResponse<AutoFeederTemplate> CreateAutoFeederTemplate()
    {
        var form = DetailResponse<AutoFeederTemplate>.Create();
        SetAuditProperties(form.Model);
        SetOwnedProperties(form.Model);
        return form;
    }

    [HttpDelete("/api/mfg/autofeeder/template/{id}")]
    public async Task<InvokeResult> DeleteAutoFeederTemplate(string id)
    {
        return await _mgr.DeleteAutoFeederTemplateAsync(id, OrgEntityHeader, UserEntityHeader);
    }

    [HttpPost("/api/mfg/autofeeder/template")]
    public Task<InvokeResult> AddAutoFeederTemplatePackageAsync([FromBody] AutoFeederTemplate mapping)
    {
        return _mgr.AddAutoFeederTemplateAsync(mapping, OrgEntityHeader, UserEntityHeader);
    }

    [HttpPut("/api/mfg/autofeeder/template")]
    public Task<InvokeResult> UpdateAutoFeederTemplatePackage([FromBody] AutoFeederTemplate mapping)
    {
        SetUpdatedProperties(mapping);
        return _mgr.UpdateAutoFeederTemplateAsync(mapping, OrgEntityHeader, UserEntityHeader);
    }

    [HttpGet("/api/mfg/autofeeder/templates")]
    public Task<ListResponse<AutoFeederTemplateSummary>> GetAutoFeederTemplatesForOrg()
    {
        return _mgr.GetAutoFeederTemplateSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
    }

}
}
