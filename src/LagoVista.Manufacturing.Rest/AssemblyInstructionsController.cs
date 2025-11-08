// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b3a641eaa4aa8ea51b23a04809ef2659a54d3fa4064ed9146aa300dc6b350f74
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
    public class AssemblyInstructionController : LagoVistaBaseController
    {
        private readonly IAssemblyInstructionManager _mgr;

        public AssemblyInstructionController(UserManager<AppUser> userManager, IAdminLogger logger, IAssemblyInstructionManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/assembly/instruction/{id}")]
        public async Task<DetailResponse<AssemblyInstruction>> GetAssemblyInstruction(string id)
        {
            return DetailResponse<AssemblyInstruction>.Create(await _mgr.GetAssemblyInstructionAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/assembly/instruction/factory")]
        public DetailResponse<AssemblyInstruction> CreateAssemblyInstruction()
        {
            var form = DetailResponse<AssemblyInstruction>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpGet("/api/mfg/assembly/instructions/step/factory")]
        public DetailResponse<AssemblyInstructionStep> CreateAssemblyInstructionStep()
        {
            return DetailResponse<AssemblyInstructionStep>.Create();
        }

        [HttpDelete("/api/mfg/assembly/instruction/{id}")]
        public async Task<InvokeResult> DeleteAssemblyInstruction(string id)
        {
            return await _mgr.DeleteAssemblyInstructionAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/assembly/instruction")]
        public Task<InvokeResult> AddAssemblyInstructionPackageAsync([FromBody] AssemblyInstruction AssemblyInstruction)
        {
            return _mgr.AddAssemblyInstructionAsync(AssemblyInstruction, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/assembly/instruction")]
        public Task<InvokeResult> UpdateAssemblyInstructionPackage([FromBody] AssemblyInstruction AssemblyInstruction)
        {
            SetUpdatedProperties(AssemblyInstruction);
            return _mgr.UpdateAssemblyInstructionAsync(AssemblyInstruction, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/assembly/instructions")]
        public Task<ListResponse<AssemblyInstructionSummary>> GetAssemblyInstructionsForOrg()
        {
            return _mgr.GetAssemblyInstructionsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
