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
using LagoVista.IoT.Logging.Utils;
using System;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class MachineController : LagoVistaBaseController
    {
        private readonly IMachineManager _mgr;

        public MachineController(UserManager<AppUser> userManager, IAdminLogger logger, IMachineManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/machine/{id}")]
        public async Task<DetailResponse<Machine>> GetMachine(string id)
        {
            return DetailResponse<Machine>.Create(await _mgr.GetMachineAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/machine/factory")]
        public DetailResponse<Machine> CreateMachine()
        {
            var form = DetailResponse<Machine>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/machine/{id}")]
        public async Task<InvokeResult> DeleteMachine(string id)
        {
            return await _mgr.DeleteMachineAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/machine")]
        public Task<InvokeResult> AddMachineAsync([FromBody] Machine Machine)
        {
            Console.WriteLine("Adding machine here...." + Machine.Name);

            return _mgr.AddMachineAsync(Machine, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/machine")]
        public Task<InvokeResult> UpdateMachineAsync([FromBody] Machine Machine)
        {
            SetUpdatedProperties(Machine);
            return _mgr.UpdateMachineAsync(Machine, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/machines")]
        public Task<ListResponse<MachineSummary>> GetMachinesForOrgAsyc()
        {
            return _mgr.GetMachineSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
