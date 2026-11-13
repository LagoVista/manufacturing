using LagoVista.Core;
using LagoVista.Core.Models;
using System;
using System.Linq;

namespace LagoVista.Manufacturing.Models.Operations
{
    /// <summary>
    /// Validates an operation snapshot before recording it. Saving history
    /// NEVER edits the feeder or marks a camera measurement as persisted.
    /// Retries use the same RunId and are idempotent by snapshot timestamp.
    /// </summary>
    public static class MachineOperationRunProjection
    {
        public static MachineOperationRunRecord Project(
            MachineOperationRunSnapshot snapshot, MachineOperationDefinition definition,
            EntityHeader org, EntityHeader user, MachineOperationRunRecord previous = null)
        {
            if (snapshot == null || definition == null || snapshot.Target == null ||
                String.IsNullOrWhiteSpace(snapshot.RunId) ||
                String.IsNullOrWhiteSpace(snapshot.Target.MachineId) ||
                definition.Version <= 0 || definition.Key != snapshot.DefinitionKey ||
                definition.Version != snapshot.DefinitionVersion ||
                snapshot.SchemaVersion <= 0 || snapshot.Steps == null ||
                snapshot.Steps.Count != definition.Steps.Count)
                throw new ArgumentException("Invalid or inconsistent machine-operation snapshot.");

            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new ArgumentException("An authenticated organization is required.", nameof(org));
            if (user == null || String.IsNullOrWhiteSpace(user.Id))
                throw new ArgumentException("An authenticated operator is required.", nameof(user));
            if (snapshot.UpdatedAtUtc < snapshot.CreatedAtUtc)
                throw new ArgumentException("Operation update predates its creation.");

            for (var i = 0; i < snapshot.Steps.Count; ++i)
            {
                var step = snapshot.Steps[i];
                var required = definition.Steps[i];
                if (step == null || required == null || step.Key != required.Key ||
                    step.Evidence == null)
                    throw new ArgumentException("The snapshot must retain the ordered step definition.");
                if (step.Evidence.Any(e => e == null || e.RunId != snapshot.RunId ||
                    e.StepKey != step.Key || e.CapturedAtUtc < snapshot.CreatedAtUtc))
                    throw new ArgumentException("Evidence must belong to the run and its step.");
                if (step.Status == MachineOperationStepStatus.Passed &&
                    !step.Evidence.Any(e => e.Verification == required.RequiredVerification &&
                       (!required.RequiresSavedEvidence || e.Saved)))
                    throw new ArgumentException("Passed step has no matching required evidence.");
            }
            if (snapshot.Status == MachineOperationRunStatus.Completed &&
                snapshot.Steps.Any(x => x.Status != MachineOperationStepStatus.Passed))
                throw new ArgumentException("A completed operation must pass every step.");

            if (previous != null)
            {
                if (previous.Id != snapshot.RunId ||
                    previous.OwnerOrganization?.Id != org.Id ||
                    previous.MachineId != snapshot.Target.MachineId ||
                    previous.FeederId != snapshot.Target.FeederId ||
                    previous.RowIndex != snapshot.Target.RowIndex ||
                    previous.Snapshot?.CreatedAtUtc != snapshot.CreatedAtUtc)
                    throw new InvalidOperationException("Cannot overwrite another operation or organization.");

                // A delayed duplicate or retry must not roll back a newer run.
                if (snapshot.UpdatedAtUtc <= previous.RunUpdatedAtUtc)
                    return previous;

                if (previous.Snapshot?.Status == MachineOperationRunStatus.Completed &&
                    snapshot.Status != MachineOperationRunStatus.Completed)
                    throw new InvalidOperationException("A completed operation cannot resume.");

                for (var i = 0; i < snapshot.Steps.Count; ++i)
                {
                    var oldStep = previous.Snapshot.Steps[i];
                    var nextStep = snapshot.Steps[i];
                    if (oldStep.Status == MachineOperationStepStatus.Passed &&
                        nextStep.Status != MachineOperationStepStatus.Passed)
                        throw new InvalidOperationException("Previously passed steps cannot be undone.");
                    if (oldStep.Evidence.Any(e => e.Saved) &&
                        !nextStep.Evidence.Any(e => e.Saved &&
                            oldStep.Evidence.Any(p => p.Name == e.Name &&
                                p.Value == e.Value && p.StepKey == e.StepKey)))
                        throw new InvalidOperationException("Persisted calibration evidence cannot be removed.");
                }
            }

            var now = UtcTimestamp.Now;
            var record = previous ?? new MachineOperationRunRecord
            {
                Id = snapshot.RunId,
                Key = snapshot.RunId,
                CreationDate = now,
                CreatedBy = user,
                OwnerOrganization = org
            };
            record.Name = $"Machine operation {snapshot.DefinitionKey} ({snapshot.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC)";
            record.Description = "Operator and camera evidence; calibration settings remain on the source entity.";
            record.Definition = definition;
            record.Snapshot = snapshot;
            record.MachineId = snapshot.Target.MachineId;
            record.FeederId = snapshot.Target.FeederId;
            record.RowIndex = snapshot.Target.RowIndex;
            record.RunUpdatedAtUtc = snapshot.UpdatedAtUtc;
            record.LastUpdatedBy = user;
            record.LastUpdatedDate = now;
            return record;
        }
    }
}
