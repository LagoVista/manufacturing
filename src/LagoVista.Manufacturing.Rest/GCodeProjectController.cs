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
    public class GCodeProjectController : LagoVistaBaseController
    {
        private readonly IGCodeProjectManager _mgr;

        public GCodeProjectController(UserManager<AppUser> userManager, IAdminLogger logger, IGCodeProjectManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/gcode/project/{id}")]
        public async Task<DetailResponse<GCodeProject>> GetGCodeProject(string id)
        {
            return DetailResponse<GCodeProject>.Create(await _mgr.GetGCodeProjectAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/gcode/tool/factory")]
        public DetailResponse<GCodeTool> CreateGCodeProjectTool()
        {
            return DetailResponse<GCodeTool>.Create();
        }

        [HttpGet("/api/mfg/gcode/plane/factory")]
        public DetailResponse<GCodePlane> CreateGCodeProjectPlane()
        {
            return DetailResponse<GCodePlane>.Create();
        }


        [HttpGet("/api/mfg/gcode/layer/factory")]
        public DetailResponse<GCodeLayer> CreateGCodeLayer()
        {
            return DetailResponse<GCodeLayer>.Create();
        }

        [HttpGet("/api/mfg/gcode/hole/factory")]
        public DetailResponse<GCodeHole> CreateGCodeProjectHole()
        {
            return DetailResponse<GCodeHole>.Create();
        }

        [HttpGet("/api/mfg/gcode/drill/factory")]
        public DetailResponse<GCodeDrill> CreateGCodeProjecDrill()
        {
            return DetailResponse<GCodeDrill>.Create();
        }

        [HttpGet("/api/mfg/gcode/rect/factory")]
        public DetailResponse<GCodeRectangle> CreateGCodeProjectRect()
        {
            return DetailResponse<GCodeRectangle>.Create();
        }

        [HttpGet("/api/mfg/gcode/polygon/factory")]
        public DetailResponse<GCodePolygon> CreateGCodeProjectPolygon()
        {
            return DetailResponse<GCodePolygon>.Create();
        }

        [HttpGet("/api/mfg/gcode/project/factory")]
        public DetailResponse<GCodeProject> CreateGCodeProject()
        {
            var form = DetailResponse<GCodeProject>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }
        
        [HttpDelete("/api/mfg/gcode/project/{id}")]
        public async Task<InvokeResult> DeleteGCodeProject(string id)
        {
            return await _mgr.DeleteGCodeProjectAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/gcode/project")]
        public Task<InvokeResult> AddGCodeProjectPackageAsync([FromBody] GCodeProject gcodeProject)
        {
            return _mgr.AddGCodeProjectAsync(gcodeProject, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/gcode/project")]
        public Task<InvokeResult> UpdateGCodeProjectPackage([FromBody] GCodeProject gcodeProject)
        {
            SetUpdatedProperties(gcodeProject);
            return _mgr.UpdateGCodeProjectAsync(gcodeProject, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/gcode/projects")]
        public Task<ListResponse<GCodeProjectSummary>> GetEquomentForOrg()
        {
            return _mgr.GetGCodeProjectsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
