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
    public class ComponentController : LagoVistaBaseController
    {
        private readonly IComponentManager _mgr;

        public ComponentController(UserManager<AppUser> userManager, IAdminLogger logger, IComponentManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/component/{id}")]
        public async Task<DetailResponse<Component>> GetComponent(string id)
        {
            return DetailResponse<Component>.Create(await _mgr.GetComponentAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/component/factory")]
        public DetailResponse<Component> CreateComponent()
        {
            var form = DetailResponse<Component>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpGet("/api/mfg/component/purchase/factory")]
        public DetailResponse<ComponentPurchase> CreateComponentPurcahse()
        {
            return DetailResponse<ComponentPurchase>.Create();
        }

        [HttpDelete("/api/mfg/component/{id}")]
        public async Task<InvokeResult> DeleteComponent(string id)
        {
            return await _mgr.DeleteCommponentAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/component")]
        public Task<InvokeResult> AddComponentPackageAsync([FromBody] Component component)
        {
            return _mgr.AddComponentAsync(component, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/component")]
        public Task<InvokeResult> UpdateComponentPackage([FromBody] Component component)
        {
            SetUpdatedProperties(component);
            return _mgr.UpdateComponentAsync(component, OrgEntityHeader, UserEntityHeader);
        }

        [HttpGet("/api/mfg/components")]
        public Task<ListResponse<ComponentSummary>> GetEquomentForOrg()
        {
            return _mgr.GetComponentsSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

    }
}
