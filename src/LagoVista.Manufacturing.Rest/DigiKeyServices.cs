using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Web.Common.Controllers;
using LagoVista.Manufacturing.Interfaces.Services;
using LagoVista.Manufacturing.Models;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Rest
{

    public class DigiKeyServices : LagoVistaBaseController
    {
        private readonly IDigiKeyLookupService _digiKeyLookupService;

        public DigiKeyServices(UserManager<AppUser> userManager, IDigiKeyLookupService digiKeyLookupService, IAdminLogger logger) : base(userManager, logger)
        {
            _digiKeyLookupService = digiKeyLookupService;
        }

        [HttpPost("/api/mfg/digikey/partlookup")]
        public Task<InvokeResult<Component>> ResolvePart([FromBody] Component component)
        {
            return _digiKeyLookupService.ResolveProductInformation(component);
        }
    }
}
