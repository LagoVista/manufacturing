using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing
{
    public interface IPickAndPlaceJobResolverService
    {
        InvokeResult ResolveParts(PickAndPlaceJob job);
        InvokeResult ResolveJobAsync(Machine machine, PickAndPlaceJob job, CircuitBoardRevision boardRevision, IEnumerable<StripFeeder> stripFeeders, IEnumerable<AutoFeeder> autoFeeders);
    }
}
