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
using LagoVista.Core;
using LagoVista.Manufacturing.Managers;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class MachineController : LagoVistaBaseController
    {
        private readonly IMachineManager _machineManager;

        public MachineController(UserManager<AppUser> userManager, IAdminLogger logger, IMachineManager machineManager) : base(userManager, logger)
        {
            _machineManager = machineManager ?? throw new ArgumentNullException(nameof(machineManager));
        }

        [HttpGet("/api/mfg/machine/{id}")]
        public async Task<DetailResponse<Machine>> GetMachine(string id)
        {
            return DetailResponse<Machine>.Create(await _machineManager.GetMachineAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/machine/{id}/stagingplates")]
        public async Task<ListResponse<MachineStagingPlate>> GetStagingPlates(string id)
        {
            var machine = await _machineManager.GetMachineAsync(id, OrgEntityHeader, UserEntityHeader);
            return ListResponse<MachineStagingPlate>.Create(machine.StagingPlates);
        }

        [HttpGet("/api/mfg/machine/factory")]
        public DetailResponse<Machine> CreateMachine()
        {
            var form = DetailResponse<Machine>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpGet("/api/mfg/machine/nozzletip/factory")]
        public DetailResponse<ToolNozzleTip> CreateNozzle()
        {
            return DetailResponse<ToolNozzleTip>.Create();
        }

        [HttpGet("/api/mfg/machine/camera/factory")]
        public DetailResponse<MachineCamera> CreateCamera()
        {
            return DetailResponse<MachineCamera>.Create();
        }


        [HttpGet("/api/mfg/machine/toolhead/factory")]
        public DetailResponse<MachineToolHead> CreateToolHead()
        {
            return DetailResponse<MachineToolHead>.Create();
        }


        [HttpGet("/api/mfg/machine/stagingplate/factory")]
        public DetailResponse<MachineStagingPlate> CreateStagingPlate()
        {
            var form = DetailResponse<MachineStagingPlate>.Create();
            form.View[nameof(MachineStagingPlate.ReferenceHoleColumn1).CamelCase()].Options = MachineManager.GetStagingPlateColumns();
            form.View[nameof(MachineStagingPlate.ReferenceHoleRow1).CamelCase()].Options = MachineManager.GetStagingPlateRows();
            form.View[nameof(MachineStagingPlate.ReferenceHoleColumn2).CamelCase()].Options = MachineManager.GetStagingPlateColumns();
            form.View[nameof(MachineStagingPlate.ReferenceHoleRow2).CamelCase()].Options = MachineManager.GetStagingPlateRows();
            return form;
        }

        [HttpGet("/api/mfg/machine/feederrail/factory")]
        public DetailResponse<MachineFeederRail> CreateFeederRail()
        {
            return DetailResponse<MachineFeederRail>.Create();
        }

        [HttpDelete("/api/mfg/machine/{id}")]
        public async Task<InvokeResult> DeleteMachine(string id)
        {
            return await _machineManager.DeleteMachineAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/machine")]
        public Task<InvokeResult> AddMachineAsync([FromBody] Machine Machine)
        {
            Console.WriteLine("Adding machine here...." + Machine.Name);

            return _machineManager.AddMachineAsync(Machine, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/machine")]
        public Task<InvokeResult> UpdateMachineAsync([FromBody] Machine Machine)
        {
            SetUpdatedProperties(Machine);
            return _machineManager.UpdateMachineAsync(Machine, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/machines")]
        public Task<ListResponse<MachineSummary>> GetMachinesForOrgAsyc()
        {
            return _machineManager.GetMachineSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/machine/{machineid}/revision/testfit")]
        public Task<InvokeResult<PcbJobTestFit>> TestFitRevision(string machineid, [FromBody] CircuitBoardRevision revision)
        {
            return _machineManager.TestFitJobAsync(machineid, revision, OrgEntityHeader, UserEntityHeader);
        }
    }
}
