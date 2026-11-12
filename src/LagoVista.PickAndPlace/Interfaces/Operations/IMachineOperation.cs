using LagoVista.Manufacturing.Models.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.Operations
{
    /// <summary>
    /// Concrete C# implementations own motion and device interactions.
    /// The WPF surface observes data-ready snapshots and available actions.
    /// </summary>
    public interface IMachineOperation
    {
        MachineOperationDefinition Definition { get; }
        MachineOperationRunSnapshot Current { get; }
        IReadOnlyList<MachineOperationAction> AvailableActions { get; }
        Task<MachineOperationActionResult> ExecuteNextAsync(CancellationToken cancellationToken);
        Task<MachineOperationActionResult> ConfirmAsync(CancellationToken cancellationToken);
        Task<MachineOperationActionResult> RetryAsync(CancellationToken cancellationToken);
        Task<MachineOperationActionResult> CancelAsync(CancellationToken cancellationToken);
    }
}
