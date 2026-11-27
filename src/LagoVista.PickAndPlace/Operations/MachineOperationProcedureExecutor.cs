using LagoVista.Manufacturing.Models.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Operations
{
    /// <summary>
    /// Executes a finite, ordered MachineOperationDefinition through its existing
    /// strongly typed runner. MachineOperationSession owns step state and evidence;
    /// this coordinator owns only continuation and checkpoint failure policy.
    /// Reusable for staging-plate, tool offset and nozzle procedures.
    /// </summary>
    public static class MachineOperationProcedureExecutor
    {
        public const int MaxActions = 24;

        public sealed class Outcome
        {
            public bool Completed { get; set; }
            public string Message { get; set; }
            public int Actions { get; set; }
        }

        public static async Task<Outcome> RunAsync(
            Func<MachineOperationRunSnapshot> snapshot,
            Func<CancellationToken, Task<MachineOperationActionResult>> executeNext,
            Func<CancellationToken, Task<bool>> persistCheckpoint,
            CancellationToken token)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (executeNext == null) throw new ArgumentNullException(nameof(executeNext));
            if (persistCheckpoint == null) throw new ArgumentNullException(nameof(persistCheckpoint));

            for (var actionCount = 0; actionCount < MaxActions; actionCount++)
            {
                token.ThrowIfCancellationRequested();
                var current = snapshot();
                if (current == null)
                    return new Outcome { Message = "The procedure lost its operation snapshot.", Actions = actionCount };
                if (current.Status == MachineOperationRunStatus.Completed)
                    return new Outcome { Completed = true, Message = "Procedure completed.", Actions = actionCount };
                if (current.Status != MachineOperationRunStatus.Pending &&
                    current.Status != MachineOperationRunStatus.Running)
                    return new Outcome { Message = current.LastError ?? "Procedure stopped at " + current.ActiveStepKey, Actions = actionCount };

                var result = await executeNext(token);
                if (result == null || !result.Successful)
                    return new Outcome
                    {
                        Message = result?.ErrorMessage ?? snapshot()?.LastError ?? "Procedure step failed.",
                        Actions = actionCount + 1
                    };

                // Saving evidence after every transition is a checkpoint.
                // Do not continue physical motion if the audit record fails.
                if (!await persistCheckpoint(token))
                    return new Outcome { Message = "Procedure history checkpoint did not save.", Actions = actionCount + 1 };
            }

            return new Outcome
            {
                Message = "Procedure reached its bounded transition limit; reset and inspect before retry.",
                Actions = MaxActions
            };
        }
    }
}
