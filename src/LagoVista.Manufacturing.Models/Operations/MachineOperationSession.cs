using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.Manufacturing.Models.Operations
{
    /// <summary>
    /// Pure, deterministic state/evidence reducer. Never moves hardware, selects
    /// a profile or persists a setting. Those responsibilities belong to a
    /// concrete, strongly typed operation runner.
    /// </summary>
    public sealed class MachineOperationSession
    {
        private readonly MachineOperationDefinition _definition;
        private readonly MachineOperationRunSnapshot _snapshot;

        public MachineOperationSession(MachineOperationDefinition definition,
            MachineOperationTarget target, DateTimeOffset createdAtUtc)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (String.IsNullOrWhiteSpace(definition.Key) || definition.Version <= 0 ||
                definition.Steps == null || definition.Steps.Count == 0)
                throw new ArgumentException("Operation definition must have a key, version and steps.", nameof(definition));
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var step in definition.Steps)
            {
                if (step == null || String.IsNullOrWhiteSpace(step.Key) ||
                    !keys.Add(step.Key) || step.RequiredVerification == MachineOperationVerificationKind.None)
                    throw new ArgumentException("Operation steps must have unique keys and verification requirements.", nameof(definition));
            }

            _definition = definition;
            _snapshot = new MachineOperationRunSnapshot
            {
                RunId = Guid.NewGuid().ToString("N"),
                DefinitionKey = definition.Key,
                DefinitionVersion = definition.Version,
                Target = target,
                Status = MachineOperationRunStatus.Pending,
                CreatedAtUtc = createdAtUtc,
                UpdatedAtUtc = createdAtUtc,
                Steps = definition.Steps.Select(step => new MachineOperationStepSnapshot
                {
                    Key = step.Key,
                    Status = MachineOperationStepStatus.Pending
                }).ToList()
            };
        }

        public MachineOperationDefinition Definition => _definition;
        public MachineOperationRunSnapshot Snapshot => _snapshot;

        public IReadOnlyList<MachineOperationAction> AvailableActions
        {
            get
            {
                if (_snapshot.Status == MachineOperationRunStatus.Pending)
                    return new[] { MachineOperationAction.Start };
                if (_snapshot.Status == MachineOperationRunStatus.Blocked)
                    return new[] { MachineOperationAction.Retry, MachineOperationAction.Cancel };
                if (_snapshot.Status != MachineOperationRunStatus.Running)
                    return Array.Empty<MachineOperationAction>();

                var current = CurrentStep();
                var required = DefinitionFor(current.Key).RequiredVerification;
                var actions = new List<MachineOperationAction>();
                if (required == MachineOperationVerificationKind.OperatorConfirmed)
                    actions.Add(MachineOperationAction.Confirm);
                if (HasRequiredEvidence(current))
                    actions.Add(MachineOperationAction.Continue);
                actions.Add(MachineOperationAction.Cancel);
                return actions;
            }
        }

        public void Start(DateTimeOffset now)
        {
            RequireStatus(MachineOperationRunStatus.Pending);
            _snapshot.Status = MachineOperationRunStatus.Running;
            Activate(_snapshot.Steps[0], now);
        }

        public void RecordEvidence(MachineOperationEvidence evidence)
        {
            RequireStatus(MachineOperationRunStatus.Running);
            var current = CurrentStep();
            if (evidence == null || evidence.RunId != _snapshot.RunId ||
                evidence.StepKey != current.Key ||
                evidence.Verification == MachineOperationVerificationKind.None ||
                !current.StartedAtUtc.HasValue ||
                evidence.CapturedAtUtc < current.StartedAtUtc.Value)
                throw new InvalidOperationException("Evidence does not belong to the active step or is stale.");
            current.Evidence.Add(evidence);
            _snapshot.UpdatedAtUtc = evidence.CapturedAtUtc;
        }

        public void CompleteActiveStep(string stepKey, DateTimeOffset now)
        {
            RequireStatus(MachineOperationRunStatus.Running);
            var current = CurrentStep();
            if (current.Key != stepKey)
                throw new InvalidOperationException("Cannot complete an out-of-order step.");
            if (now < current.StartedAtUtc)
                throw new InvalidOperationException("Completion precedes step start.");
            if (!HasRequiredEvidence(current))
                throw new InvalidOperationException("Fresh, matching verification and saved evidence are required.");

            current.Status = MachineOperationStepStatus.Passed;
            current.CompletedAtUtc = now;
            _snapshot.UpdatedAtUtc = now;
            var index = _snapshot.Steps.IndexOf(current);
            if (index + 1 == _snapshot.Steps.Count)
            {
                _snapshot.ActiveStepKey = null;
                _snapshot.Status = MachineOperationRunStatus.Completed;
                _snapshot.CompletedAtUtc = now;
            }
            else
            {
                Activate(_snapshot.Steps[index + 1], now);
            }
        }

        public void BlockActiveStep(string reason, DateTimeOffset now)
        {
            RequireStatus(MachineOperationRunStatus.Running);
            if (String.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("A blocked step requires an explanation.", nameof(reason));
            var step = CurrentStep();
            step.Status = MachineOperationStepStatus.Blocked;
            step.Error = reason;
            _snapshot.LastError = reason;
            _snapshot.Status = MachineOperationRunStatus.Blocked;
            _snapshot.UpdatedAtUtc = now;
        }

        public void Retry(DateTimeOffset now)
        {
            RequireStatus(MachineOperationRunStatus.Blocked);
            var step = CurrentStep();
            step.Evidence.Clear(); // evidence captured before retry is never reusable
            step.Error = null;
            step.CompletedAtUtc = null;
            _snapshot.LastError = null;
            _snapshot.Status = MachineOperationRunStatus.Running;
            Activate(step, now);
        }

        public void Cancel(DateTimeOffset now)
        {
            if (_snapshot.Status != MachineOperationRunStatus.Running &&
                _snapshot.Status != MachineOperationRunStatus.Blocked)
                throw new InvalidOperationException("Only a running or blocked operation can be cancelled.");
            CurrentStep().Status = MachineOperationStepStatus.Cancelled;
            _snapshot.Status = MachineOperationRunStatus.Cancelled;
            _snapshot.ActiveStepKey = null;
            _snapshot.CompletedAtUtc = now;
            _snapshot.UpdatedAtUtc = now;
        }

        private void Activate(MachineOperationStepSnapshot step, DateTimeOffset now)
        {
            step.Status = MachineOperationStepStatus.Active;
            step.StartedAtUtc = now;
            _snapshot.ActiveStepKey = step.Key;
            _snapshot.UpdatedAtUtc = now;
        }

        private MachineOperationStepSnapshot CurrentStep()
        {
            var step = _snapshot.Steps.FirstOrDefault(s => s.Key == _snapshot.ActiveStepKey);
            if (step == null)
                throw new InvalidOperationException("No active operation step.");
            return step;
        }

        private MachineOperationStepDefinition DefinitionFor(string key) =>
            _definition.Steps.Single(step => step.Key == key);

        private bool HasRequiredEvidence(MachineOperationStepSnapshot current)
        {
            var definition = DefinitionFor(current.Key);
            return current.Evidence.Any(e => e.Verification == definition.RequiredVerification &&
                (!definition.RequiresSavedEvidence || e.Saved));
        }

        private void RequireStatus(MachineOperationRunStatus status)
        {
            if (_snapshot.Status != status)
                throw new InvalidOperationException($"Operation state must be {status}; current state is {_snapshot.Status}.");
        }
    }
}
