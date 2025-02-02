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
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.Manufacturing.Rest.Controllers
{
    [ConfirmedUser]
    [Authorize]
    public class ComponentPackageController : LagoVistaBaseController
    {
        private readonly IComponentPackageManager _mgr;

        public ComponentPackageController(UserManager<AppUser> userManager, IAdminLogger logger, IComponentPackageManager mgr) : base(userManager, logger)
        {
            _mgr = mgr;
        }

        [HttpGet("/api/mfg/component/package/{id}")]
        public async Task<DetailResponse<ComponentPackage>> GetComponentPackage(string id)
        {
            return DetailResponse<ComponentPackage>.Create( await _mgr.GetComponentPackageAsync(id, OrgEntityHeader, UserEntityHeader));
        }

        [HttpGet("/api/mfg/component/package/factory")]
        public DetailResponse<ComponentPackage> CreateComponentPackage()
        {
            var form = DetailResponse<ComponentPackage>.Create();
            SetAuditProperties(form.Model);
            SetOwnedProperties(form.Model);
            return form;
        }

        [HttpDelete("/api/mfg/component/package/{id}")]
        public async Task<InvokeResult> DeleteComponentPackage(string id)
        {
            return await _mgr.DeleteCommponentPackageAsync(id, OrgEntityHeader, UserEntityHeader);
        }

        /// <summary>
        /// ComponentPackage - Add
        /// </summary>
        /// <param name="ComponentPackage"></param>
        [HttpPost("/api/mfg/component/package")]
        public Task<InvokeResult> AddComponentPackageAsync([FromBody] ComponentPackage ComponentPackage)
        {
            return _mgr.AddComponentPackageAsync(ComponentPackage, OrgEntityHeader, UserEntityHeader);
        }

        /// <summary>
        /// ComponentPackage - Update
        /// </summary>
        /// <param name="ComponentPackage"></param>
        /// <returns></returns>
        [HttpPut("/api/mfg/component/package")]
        public Task<InvokeResult> UpdateComponentPackage([FromBody] ComponentPackage ComponentPackage)
        {
            SetUpdatedProperties(ComponentPackage);
            return _mgr.UpdateComponentPackageAsync(ComponentPackage, OrgEntityHeader, UserEntityHeader);
        }

        /// <summary>
        /// ComponentPackage - Get for Current Org
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/mfg/component/packages")]
        public Task<ListResponse<ComponentPackageSummary>> GetEquomentForOrg()
        {
            return _mgr.GetComponentPackagesSummariesAsync(GetListRequestFromHeader(), OrgEntityHeader, UserEntityHeader);
        }

        [HttpPost("/api/mfg/component/package/{id}/layout")]
        public Task<InvokeResult> SetPads(string id, [FromBody] PcbPackage layout)
        {
            return _mgr.SetLayoutAsync(id, layout, OrgEntityHeader, UserEntityHeader);
        }

        [HttpPut("/api/mfg/component/package/{id}/visionprofile/{type}")]
        public async Task<InvokeResult> UpdateVisionProfile(string id, string type, [FromBody] VisionProfile profile)
        {
            var componentPackage = await _mgr.GetComponentPackageAsync(id, OrgEntityHeader, UserEntityHeader);
            switch(type.ToLower())
            {
                case "inspection":
                    componentPackage.PartInspectionVisionProfile = profile;
                    break;
                case "partonboard":
                    componentPackage.PartOnBoardVisionProfile = profile;
                    break;
                case "partintape":
                    componentPackage.PartInTapeVisionProfile = profile;
                    break;
                default:
                    return InvokeResult.FromError($"{type} is not a valid vision profile type for a component package.");
            }

            return await _mgr.UpdateComponentPackageAsync(componentPackage, OrgEntityHeader, UserEntityHeader);    
        }
    }
}
