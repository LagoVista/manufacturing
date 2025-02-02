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
using LagoVista.Core.Models.Drawing;
using System;
using LagoVista.Core;
using System.Linq;
using LagoVista.Manufacturing.Managers;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class StripFeederController : LagoVistaBaseController
    {
        private readonly IStripFeederManager _mgr;
        private readonly IMachineManager _machineManager;

        public StripFeederController(UserManager<AppUser> userManager, IAdminLogger logger,  IStripFeederManager mgr, IMachineManager machineManager) : 
            base(userManager, logger)
        {
            _mgr = mgr;
            _machineManager = machineManager;
        }

        [HttpGet("/api/mfg/stripfeeder/{id}")]
        public async Task<DetailResponse<StripFeeder>> GetStripFeeder(string id, bool loadcomponent = false)
        {
            var form = DetailResponse<StripFeeder>.Create(await _mgr.GetStripFeederAsync(id, loadcomponent, OrgEntityHeader, UserEntityHeader));
            form.View[nameof(StripFeeder.ReferenceHoleColumn).CamelCase()].Options = MachineManager.GetStagingPlateColumns();
            form.View[nameof(StripFeeder.ReferenceHoleRow).CamelCase()].Options = MachineManager.GetStagingPlateRows();
            return form;
        }

        [HttpGet("/api/mfg/stripfeeder/factory")]
        public DetailResponse<StripFeeder> CreateStripFeeder()
        {
            var form = DetailResponse<StripFeeder>.Create();
            form.Model.Rows.Add(new StripFeederRow()
            {
                Id = Guid.NewGuid().ToId(),
                RowIndex = 1,
                CurrentPartIndex = 1,
                Notes = "",
                FirstTapeHoleOffset = new Point2D<double>(5, 5),
            });

            form.View[nameof(StripFeeder.ReferenceHoleColumn).CamelCase()].Options = MachineManager.GetStagingPlateColumns();
            form.View[nameof(StripFeeder.ReferenceHoleRow).CamelCase()].Options = MachineManager.GetStagingPlateRows();

            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpGet("/api/mfg/stripfeeder/template/{templateid}/factory")]
        public async Task<DetailResponse<StripFeeder>> CreateFromTemplate(string templateid)
        {
            var feeder = await _mgr.CreateFromTemplateAsync(templateid, OrgEntityHeader, UserEntityHeader);
            var form = DetailResponse<StripFeeder>.Create(feeder, false);
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpGet("/api/mfg/stripfeeder/row/factory")]
        public DetailResponse<StripFeederRow> CreateStripFeederFow()
        {
            return DetailResponse<StripFeederRow>.Create();
        }

        [HttpDelete("/api/mfg/stripfeeder/{id}")]
        public async Task<InvokeResult> DeleteStripFeeder(string id)
        {
            return await _mgr.DeleteStripFeederAsycn(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/stripfeeder")]
        public Task<InvokeResult> AddStripFeederPackageAsync([FromBody] StripFeeder feeder)
        {
            return _mgr.AddStripFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/stripfeeder")]
        public Task<InvokeResult> UpdateStripFeederPackage([FromBody] StripFeeder feeder)
        {
            SetUpdatedProperties(feeder);
            return _mgr.UpdateStripFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/stripfeeders")]
        public Task<ListResponse<StripFeederSummary>> GetStripFeedersForOrg()
        {
            return _mgr.GetStripFeedersSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/machine/{machineid}/stripfeeders")]
        public Task<ListResponse<StripFeeder>> GetStripFeedersForMachine(string machineid, bool loadcomponents)
        {
            return _mgr.GetStripFeedersForMachineAsync(machineid, loadcomponents, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/stripfeeder/{id}/row/{rowidx}/partindex/{idx}")]
        public async Task<InvokeResult> UpdatePartIndex(string id, int rowidx, int idx)
        {
            var fdr = await _mgr.GetStripFeederAsync(id,false, OrgEntityHeader, UserEntityHeader);
            var existingRow = fdr.Rows.SingleOrDefault(row => row.RowIndex == idx);
            if(existingRow == null)
                return InvokeResult.FromError("Could not find row.");

            existingRow.CurrentPartIndex = idx;
            return await _mgr.UpdateStripFeederAsync(fdr, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/machine/{machineid}/stagingplate/{plateid}/{row}/{col}/stripfeeder/{feederid}/attach")]
        public async Task<InvokeResult> AttachToMachine(string machineid, string plateid, string row, string col, string feederid)
        {
            var machine = await _machineManager.GetMachineAsync(machineid, OrgEntityHeader, UserEntityHeader);
            var plate = machine.StagingPlates.SingleOrDefault(plt => plt.Id == plateid);
            if (plate == null)
            {
                return InvokeResult.FromError("Could not find staging plate.");
            }

            var rowOption = MachineManager.GetStagingPlateRows().SingleOrDefault(opt => opt.Id == row);
            if (rowOption == null)
            {
                return InvokeResult.FromError($"Could not find row for {row} .");
            }

            var colOption = MachineManager.GetStagingPlateColumns().SingleOrDefault(opt => opt.Id == col);
            if (colOption == null)
            {
                return InvokeResult.FromError($"Could not find row col {col} .");
            }


            var feeder = await _mgr.GetStripFeederAsync(feederid, false, OrgEntityHeader, UserEntityHeader);
            feeder.Machine = machine.ToEntityHeader();
            feeder.StagingPlate = plate.ToEntityHeader();
            feeder.ReferenceHoleRow = rowOption.ToEntityHeader();
            feeder.ReferenceHoleColumn = colOption.ToEntityHeader();
            return await _mgr.UpdateStripFeederAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }
    }
}
