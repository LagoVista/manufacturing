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
    public class PcbMillingProjectController : LagoVistaBaseController
    {
        private readonly IPcbMillingProjectManager _mgr;

        public PcbMillingProjectController(UserManager<AppUser> userManager, IAdminLogger logger, IPcbMillingProjectManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/pcb/milling/{id}")]
        public async Task<DetailResponse<PcbMillingProject>> GetPcbMillingProject(string id)
        {
            return DetailResponse<PcbMillingProject>.Create(await _mgr.GetPcbMillingProjectAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/pcb/milling/factory")]
        public DetailResponse<PcbMillingProject> CreatePcbMillingProject()
        {
            var form = DetailResponse<PcbMillingProject>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }
        
        [HttpDelete("/api/mfg/pcb/milling/{id}")]
        public async Task<InvokeResult> DeletePcbMillingProject(string id)
        {
            return await _mgr.DeletePcbMillingProjectAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/pcb/milling")]
        public Task<InvokeResult> AddPcbMillingProjectPackageAsync([FromBody] PcbMillingProject pcbMillingProject)
        {
            return _mgr.AddPcbMillingProjectAsync(pcbMillingProject, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/pcb/milling")]
        public Task<InvokeResult> UpdatePcbMillingProjectPackage([FromBody] PcbMillingProject pcbMillingProject)
        {
            SetUpdatedProperties(pcbMillingProject);
            return _mgr.UpdatePcbMillingProjectAsync(pcbMillingProject, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/pcb/millings")]
        public Task<ListResponse<PcbMillingProjectSummary>> GetMillingProjectsForOrg()
        {
            return _mgr.GetPcbMillingProjectsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
