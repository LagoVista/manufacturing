using System;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Finalizes a T014 run on the shared singleton view model. The caller
    /// must not advertise idle/completed before its asynchronous history
    /// upload settles and the old MachineOperationSession has been retired.
    /// The callback order is required for safe Head 1 -> Head 2 sequencing.
    /// </summary>
    public static class CameraToolCalibrationRunFinalization
    {
        public static async Task FinishAsync(
            Func<Task> publishSettledHistory,
            Action retireSession,
            Action markIdle,
            Action markFinished)
        {
            if (publishSettledHistory == null ||
                retireSession == null ||
                markIdle == null ||
                markFinished == null)
                throw new ArgumentNullException(nameof(publishSettledHistory),
                    "Finalization requires history publication and all three state transitions.");

            try
            {
                await publishSettledHistory();
            }
            finally
            {
                // Retire before releasing the singleton for the next head.
                // The Finished notification is last: the batch UI watches it
                // and may immediately schedule Head 2 on this same instance.
                retireSession();
                markIdle();
                markFinished();
            }
        }
    }
}
