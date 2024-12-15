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
using System;
using System.Linq;
using LagoVista.Core;
using LagoVista.Core.Models;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class ComponentController : LagoVistaBaseController
    {
        private readonly IComponentManager _mgr;
        private readonly IComponentPackageManager _pckMgr;

        public ComponentController(UserManager<AppUser> userManager, IAdminLogger logger, IComponentManager mgr, IComponentPackageManager pckMgr) : base(userManager, logger)
        {
            _mgr = mgr ?? throw new ArgumentNullException(nameof(mgr));
            _pckMgr = pckMgr ?? throw new ArgumentNullException(nameof(pckMgr));
        }

        [HttpGet("/api/mfg/component/{id}")]
        public async Task<DetailResponse<Component>> GetComponent(string id)
        {
            var response = DetailResponse<Component>.Create(await _mgr.GetComponentAsync(id, OrgEntityHeader, UserEntityHeader));
            var pckgs = await _pckMgr.GetComponentPackagesSummariesAsync(ListRequest.CreateForAll(), OrgEntityHeader, UserEntityHeader);
            response.View[nameof(LagoVista.Manufacturing.Models.Component.ComponentPackage).CamelCase()].Options = pckgs.Model.Select(pck => new EnumDescription() { Id = pck.Id, Key = pck.Key, Text = pck.Name, Label = pck.Name, Name = pck.Name }).ToList();
            response.View[nameof(LagoVista.Manufacturing.Models.Component.ComponentPackage).CamelCase()].Options.Insert(0, new EnumDescription() { Id = "-1", Key = "-1", Text = "-select-", Name = "-select-", Label = "-select-" });
            return response;
        }

        [HttpGet("/api/mfg/component/factory")]
        public async Task<DetailResponse<Component>> CreateComponent()
        {
            var form = DetailResponse<Component>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            var pckgs = await  _pckMgr.GetComponentPackagesSummariesAsync(ListRequest.CreateForAll(), OrgEntityHeader, UserEntityHeader);
            form.View[nameof(LagoVista.Manufacturing.Models.Component.ComponentPackage).CamelCase()].Options = pckgs.Model.Select(pck => new EnumDescription() { Id = pck.Id, Key = pck.Key, Text = pck.Name, Label = pck.Name, Name = pck.Name }).ToList();
            form.View[nameof(LagoVista.Manufacturing.Models.Component.ComponentPackage).CamelCase()].Options.Insert(0, new EnumDescription() { Id = "-1", Key = "-1", Text = "-select-", Name = "-select-", Label = "-select-" });
            return form;
        }

        [HttpGet("/api/mfg/component/purchase/factory")]
        public DetailResponse<ComponentPurchase> CreateComponentPurcahse()
        {
            return DetailResponse<ComponentPurchase>.Create();
        }

        [HttpGet("/api/mfg/component/attribute/factory")]
        public DetailResponse<ComponentAttribute> CreateComponentAttribute()
        {
            return DetailResponse<ComponentAttribute>.Create();
        }
   

        [HttpGet("/api/mfg/component/attribute/factory")]
        public DetailResponse<ComponentAttribute> GetComponentByType(string componentType)
        {
            return DetailResponse<ComponentAttribute>.Create();
        }


        [HttpDelete("/api/mfg/component/{id}")]
        public async Task<InvokeResult> DeleteComponent(string id)
        {
            return await _mgr.DeleteCommponentAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/component/{id}/purchase")]
        public async Task<InvokeResult> AddComponentPurchase(string id, [FromBody] ComponentPurchase purchase)
        {
            return await _mgr.AddComponentPurchaseAsync(id, purchase, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/component/{id}/purchase/{orderid}/receive/{qty}")]
        public async Task<InvokeResult> ReceiveComponentPurchase(string id, string orderid, decimal qty)
        {
            return await _mgr.ReceiveComponentPurchaseAsync(id, orderid, qty, OrgEntityHeader, UserEntityHeader);
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
        public Task<ListResponse<ComponentSummary>> GetComponentsForOrg(string componentType)
        {
            return _mgr.GetComponentsSummariesAsync(GetListRequestFromHeader(), componentType, OrgEntityHeader, UserEntityHeader);
        }
    }
}
