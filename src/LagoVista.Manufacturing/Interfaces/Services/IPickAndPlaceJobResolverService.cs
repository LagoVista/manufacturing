// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8b0dfcc0d17d67369ae4dbf0be9cced47b8977105e9a3877f2073288e6dc59f6
// IndexVersion: 0
// --- END CODE INDEX META ---
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
