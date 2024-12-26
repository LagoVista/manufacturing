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
    public class GCodeMappingController : LagoVistaBaseController
    {
        private readonly IGCodeMappingManager _mgr;
     
        public GCodeMappingController(UserManager<AppUser> userManager, IAdminLogger logger, IGCodeMappingManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/gcodemapping/{id}")]
        public async Task<DetailResponse<GCodeMapping>> GetGCodeMapping(string id, bool loadcomponent = false)
        {
            return DetailResponse<GCodeMapping>.Create(await _mgr.GetGCodeMappingAsync(id, loadcomponent, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/gcodemapping/factory")]
        public DetailResponse<GCodeMapping> CreateGCodeMapping()
        {
            var form = DetailResponse<GCodeMapping>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/gcodemapping/{id}")]
        public async Task<InvokeResult> DeleteGCodeMapping(string id)
        {
            return await _mgr.DeleteGCodeMappingAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/gcodemapping")]
        public Task<InvokeResult> AddGCodeMappingPackageAsync([FromBody] GCodeMapping mapping)
        {
            return _mgr.AddGCodeMappingAsync(mapping, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/gcodemapping")]
        public Task<InvokeResult> UpdateGCodeMappingPackage([FromBody] GCodeMapping mapping)
        {
            SetUpdatedProperties(mapping);
            return _mgr.UpdateGCodeMappingAsync(mapping, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/gcodemappings")]
        public Task<ListResponse<GCodeMappingSummary>> GetGCodeMappingsForOrg()
        {
            return _mgr.GetGCodeMappingsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
