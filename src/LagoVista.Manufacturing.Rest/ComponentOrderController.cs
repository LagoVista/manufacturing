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
    public class ComponentOrderController : LagoVistaBaseController
    {
        private readonly IComponentOrderManager _mgr;

        public ComponentOrderController(UserManager<AppUser> userManager, IAdminLogger logger, IComponentOrderManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/order/{id}")]
        public async Task<DetailResponse<ComponentOrder>> GetComponentOrder(string id)
        {
            return DetailResponse<ComponentOrder>.Create(await _mgr.GetComponentOrderAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/order/factory")]
        public DetailResponse<ComponentOrder> CreateComponentOrder()
        {
            var form = DetailResponse<ComponentOrder>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/order/{id}")]
        public async Task<InvokeResult> DeleteComponentOrder(string id)
        {
            return await _mgr.DeleteCommponentAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/order")]
        public Task<InvokeResult> AddComponentOrderPackageAsync([FromBody] ComponentOrder feeder)
        {
            return _mgr.AddComponentOrderAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/order")]
        public Task<InvokeResult> UpdateComponentOrderPackage([FromBody] ComponentOrder feeder)
        {
            SetUpdatedProperties(feeder);
            return _mgr.UpdateComponentOrderAsync(feeder, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/orders")]
        public Task<ListResponse<ComponentOrderSummary>> GetEquomentForOrg()
        {
            return _mgr.GetComponentOrdersSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }
    }
}
