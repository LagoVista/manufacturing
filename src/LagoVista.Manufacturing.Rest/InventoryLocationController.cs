// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ecbecc62ec1204c40457c790c6956d5868a65f1583f0645f0c7a664919c32f6b
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
    public class InventoryLocationController : LagoVistaBaseController
    {
        private readonly IInventoryLocationManager _mgr;

        public InventoryLocationController(UserManager<AppUser> userManager, IAdminLogger logger, IInventoryLocationManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/inventory/location/{id}")]
        public async Task<DetailResponse<InventoryLocation>> GetInventoryLocation(string id)
        {
            return DetailResponse<InventoryLocation>.Create(await _mgr.GetInventoryLocationAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/inventory/location/factory")]
        public DetailResponse<InventoryLocation> CreateInventoryLocation()
        {
            var form = DetailResponse<InventoryLocation>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }
        
        [HttpDelete("/api/mfg/inventory/location/{id}")]
        public async Task<InvokeResult> DeleteInventoryLocation(string id)
        {
            return await _mgr.DeleteCommponentAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/inventory/location")]
        public Task<InvokeResult> AddInventoryLocationPackageAsync([FromBody] InventoryLocation InventoryLocation)
        {
            return _mgr.AddInventoryLocationAsync(InventoryLocation, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/inventory/location")]
        public Task<InvokeResult> UpdateInventoryLocationPackage([FromBody] InventoryLocation InventoryLocation)
        {
            SetUpdatedProperties(InventoryLocation);
            return _mgr.UpdateInventoryLocationAsync(InventoryLocation, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/inventory/locations")]
        public Task<ListResponse<InventoryLocationSummary>> GetEquomentForOrg()
        {
            return _mgr.GetInventoryLocationsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
