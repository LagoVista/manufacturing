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
