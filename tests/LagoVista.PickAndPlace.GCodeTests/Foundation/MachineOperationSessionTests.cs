using LagoVista.Manufacturing.Models.Operations;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class MachineOperationSessionTests
    {
        private static readonly DateTimeOffset T0 =
            new DateTimeOffset(2026, 11, 12, 20, 0, 0, TimeSpan.Zero);

        private static MachineOperationSession NewSession()
        {
            return new MachineOperationSession(StripFeederCalibrationDefinition.Create(),
                new MachineOperationTarget
                {
                    MachineId = "lumen-jr",
                    FeederId = "feeder-a",
                    RowIndex = 1,
                    StartingPartIndex = 24
                }, T0);
        }

        private static MachineOperationEvidence Proof(MachineOperationSession session,
            MachineOperationVerificationKind kind, DateTimeOffset when, bool saved = false)
        {
            return new MachineOperationEvidence
            {
                RunId = session.Snapshot.RunId,
                StepKey = session.Snapshot.ActiveStepKey,
                Verification = kind,
                Name = "camera-or-machine-observation",
                Value = "verified",
                Unit = "mm",
                Source = "test",
                CapturedAtUtc = when,
                Saved = saved,
                VisionProfileKey = "feederorigin",
                PixelsPerMm = 90.9
            };
        }

        [Test]
        public void FeederDefinitionContainsTheSevenOrderedAndTypedSteps()
        {
            var definition = StripFeederCalibrationDefinition.Create();
            Assert.That(definition.Key, Is.EqualTo("strip-feeder-calibration"));
            Assert.That(definition.Version, Is.EqualTo(1));
            Assert.That(definition.Steps.Select(x => x.Key), Is.EqualTo(new[]
            {
                "approach-origin", "center-fiducial", "save-origin",
                "first-tape-hole", "last-tape-hole",
                "verify-current-part", "verify-previous-empty"
            }));
            Assert.That(definition.Steps[2].RequiresSavedEvidence, Is.True);
            Assert.That(definition.Steps[4].RequiresSavedEvidence, Is.True);
            Assert.That(definition.Steps[5].RequiredVerification,
                Is.EqualTo(MachineOperationVerificationKind.OperatorConfirmed));
        }

        [Test]
        public void CannotSkipStepOrCompleteWithoutFreshMatchingEvidence()
        {
            var session = NewSession();
            Assert.That(session.AvailableActions, Does.Contain(MachineOperationAction.Start));
            session.Start(T0.AddSeconds(1));
            Assert.Throws<InvalidOperationException>(() =>
                session.CompleteActiveStep("save-origin", T0.AddSeconds(3)));
            Assert.Throws<InvalidOperationException>(() =>
                session.CompleteActiveStep("approach-origin", T0.AddSeconds(3)));
            Assert.Throws<InvalidOperationException>(() =>
                session.RecordEvidence(Proof(session, MachineOperationVerificationKind.MachineObserved, T0)));
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.MachineObserved, T0.AddSeconds(2)));
            session.CompleteActiveStep("approach-origin", T0.AddSeconds(3));
            Assert.That(session.Snapshot.ActiveStepKey, Is.EqualTo("center-fiducial"));
            Assert.That(session.Snapshot.Steps[0].Status, Is.EqualTo(MachineOperationStepStatus.Passed));
        }

        [Test]
        public void WrongRunAndWrongStepEvidenceAreRejected()
        {
            var session = NewSession();
            session.Start(T0);
            var evidence = Proof(session, MachineOperationVerificationKind.MachineObserved, T0.AddSeconds(1));
            evidence.RunId = "other-run";
            Assert.Throws<InvalidOperationException>(() => session.RecordEvidence(evidence));
            evidence.RunId = session.Snapshot.RunId;
            evidence.StepKey = "save-origin";
            Assert.Throws<InvalidOperationException>(() => session.RecordEvidence(evidence));
        }

        [Test]
        public void BlockAndRetryDiscardPriorEvidenceAndRejectStaleObservations()
        {
            var session = NewSession();
            session.Start(T0);
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.MachineObserved, T0.AddSeconds(1)));
            session.BlockActiveStep("Camera disconnected", T0.AddSeconds(2));
            Assert.That(session.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Blocked));
            Assert.That(session.AvailableActions, Does.Contain(MachineOperationAction.Retry));
            session.Retry(T0.AddSeconds(3));
            Assert.That(session.Snapshot.LastError, Is.Null);
            Assert.That(session.Snapshot.Steps[0].Evidence, Is.Empty);
            Assert.Throws<InvalidOperationException>(() =>
                session.RecordEvidence(Proof(session, MachineOperationVerificationKind.MachineObserved, T0.AddSeconds(1))));
            Assert.Throws<InvalidOperationException>(() =>
                session.CompleteActiveStep("approach-origin", T0.AddSeconds(4)));
        }

        [Test]
        public void OriginStepRequiresOperatorProofThatWasActuallySaved()
        {
            var session = NewSession();
            session.Start(T0);
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.MachineObserved, T0.AddSeconds(1)));
            session.CompleteActiveStep("approach-origin", T0.AddSeconds(2));
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.VisionVerified, T0.AddSeconds(3)));
            session.CompleteActiveStep("center-fiducial", T0.AddSeconds(4));
            Assert.That(session.Snapshot.ActiveStepKey, Is.EqualTo("save-origin"));
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.VisionVerified, T0.AddSeconds(5), true));
            Assert.Throws<InvalidOperationException>(() =>
                session.CompleteActiveStep("save-origin", T0.AddSeconds(6)));
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.OperatorConfirmed, T0.AddSeconds(5), false));
            Assert.Throws<InvalidOperationException>(() =>
                session.CompleteActiveStep("save-origin", T0.AddSeconds(6)));
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.OperatorConfirmed, T0.AddSeconds(6), true));
            session.CompleteActiveStep("save-origin", T0.AddSeconds(7));
            Assert.That(session.Snapshot.ActiveStepKey, Is.EqualTo("first-tape-hole"));
            Assert.That(session.Snapshot.Target.StartingPartIndex, Is.EqualTo(24));
        }

        [Test]
        public void SessionCompletesOnlyAfterAllSevenStepProofs()
        {
            var session = NewSession();
            var definition = session.Definition;
            session.Start(T0);
            for (var i = 0; i < definition.Steps.Count; i++)
            {
                var step = definition.Steps[i];
                var at = T0.AddSeconds(i * 2 + 1);
                session.RecordEvidence(Proof(session, step.RequiredVerification,
                    at, step.RequiresSavedEvidence));
                session.CompleteActiveStep(step.Key, at.AddSeconds(1));
            }
            Assert.That(session.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Completed));
            Assert.That(session.Snapshot.ActiveStepKey, Is.Null);
            Assert.That(session.Snapshot.Steps.All(step => step.Status == MachineOperationStepStatus.Passed), Is.True);
            Assert.That(session.AvailableActions, Is.Empty);
            Assert.Throws<InvalidOperationException>(() => session.Cancel(T0.AddMinutes(1)));
        }

        [Test]
        public void CancellationDoesNotAdvanceOrClaimCompletion()
        {
            var session = NewSession();
            session.Start(T0);
            session.Cancel(T0.AddSeconds(1));
            Assert.That(session.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Cancelled));
            Assert.That(session.Snapshot.Steps[0].Status, Is.EqualTo(MachineOperationStepStatus.Cancelled));
            Assert.That(session.Snapshot.Steps[1].Status, Is.EqualTo(MachineOperationStepStatus.Pending));
            Assert.Throws<InvalidOperationException>(() =>
                session.RecordEvidence(Proof(session, MachineOperationVerificationKind.MachineObserved, T0.AddSeconds(2))));
        }

        [Test]
        public void DuplicateStepKeysAreRejected()
        {
            var definition = StripFeederCalibrationDefinition.Create();
            definition.Steps[1].Key = definition.Steps[0].Key;
            Assert.Throws<ArgumentException>(() =>
                new MachineOperationSession(definition, new MachineOperationTarget(), T0));
        }
    }
}
