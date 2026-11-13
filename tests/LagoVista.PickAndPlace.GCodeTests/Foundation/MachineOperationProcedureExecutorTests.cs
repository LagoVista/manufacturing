using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Operations;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class MachineOperationProcedureExecutorTests
    {
        [Test]
        public async Task ExecutesSequentialActionsUntilCompleted()
        {
            var snapshot = new MachineOperationRunSnapshot { Status = MachineOperationRunStatus.Pending };
            var actions = 0;
            var checkpoints = 0;
            var result = await MachineOperationProcedureExecutor.RunAsync(
                () => snapshot,
                token =>
                {
                    actions++;
                    snapshot.Status = actions < 3
                        ? MachineOperationRunStatus.Running
                        : MachineOperationRunStatus.Completed;
                    return Task.FromResult(new MachineOperationActionResult { Successful = true });
                },
                token => { checkpoints++; return Task.FromResult(true); },
                CancellationToken.None);

            Assert.That(result.Completed, Is.True);
            Assert.That(actions, Is.EqualTo(3));
            Assert.That(checkpoints, Is.EqualTo(3));
        }

        [Test]
        public async Task DoesNotExecuteNextMovementAfterFailedCheckpoint()
        {
            var snapshot = new MachineOperationRunSnapshot { Status = MachineOperationRunStatus.Pending };
            var actions = 0;
            var outcome = await MachineOperationProcedureExecutor.RunAsync(
                () => snapshot,
                token =>
                {
                    actions++;
                    snapshot.Status = MachineOperationRunStatus.Running;
                    return Task.FromResult(new MachineOperationActionResult { Successful = true });
                },
                token => Task.FromResult(false), CancellationToken.None);

            Assert.That(outcome.Completed, Is.False);
            Assert.That(actions, Is.EqualTo(1));
        }

        [Test]
        public async Task StopsOnBlockedStepWithoutRetrying()
        {
            var snapshot = new MachineOperationRunSnapshot { Status = MachineOperationRunStatus.Pending };
            var actions = 0;
            var outcome = await MachineOperationProcedureExecutor.RunAsync(
                () => snapshot,
                token =>
                {
                    actions++;
                    snapshot.Status = MachineOperationRunStatus.Blocked;
                    snapshot.LastError = "Missing fresh camera verification";
                    return Task.FromResult(new MachineOperationActionResult { Successful = false, ErrorMessage = snapshot.LastError });
                },
                token => Task.FromResult(true), CancellationToken.None);

            Assert.That(outcome.Completed, Is.False);
            Assert.That(actions, Is.EqualTo(1));
            Assert.That(outcome.Message, Does.Contain("camera"));
        }

        [Test]
        public async Task StopsAtBoundedTransitionLimit()
        {
            var snapshot = new MachineOperationRunSnapshot { Status = MachineOperationRunStatus.Running };
            var count = 0;
            var outcome = await MachineOperationProcedureExecutor.RunAsync(
                () => snapshot,
                token => { count++; return Task.FromResult(new MachineOperationActionResult { Successful = true }); },
                token => Task.FromResult(true), CancellationToken.None);

            Assert.That(outcome.Completed, Is.False);
            Assert.That(count, Is.EqualTo(MachineOperationProcedureExecutor.MaxActions));
        }
    }
}
