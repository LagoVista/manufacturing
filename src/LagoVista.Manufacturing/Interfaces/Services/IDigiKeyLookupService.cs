// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fb47bd1abb4ce26be7f9c27a9bc0009d1432a19fc030f93ea44c34c1ed445e93
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Services
{
    public interface IDigiKeyLookupService
    {
        Task<InvokeResult<Component>> ResolveProductInformation(Component component);
    }
}
