using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models.Operations;
using NUnit.Framework;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Mfg.ServicesTests
{
    [TestFixture]
    public class MachineOperationRunProjectionTests
    {
        private static readonly EntityHeader Org = EntityHeader.Create("org-1", "Test Org");
        private static readonly EntityHeader User = EntityHeader.Create("person-1", "Operator");
        private static readonly DateTimeOffset StartedAt =
            new DateTimeOffset(2026, 11, 13, 18, 0, 0, TimeSpan.Zero);

        private static MachineOperationSession Run()
        {
            return new MachineOperationSession(StripFeederCalibrationDefinition.Create(),
                new MachineOperationTarget
                {
                    MachineId = "lumen-pnp",
                    FeederId = "feeder-1",
                    RowIndex = 1,
                    StartingPartIndex = 2
                }, StartedAt);
        }

        private static MachineOperationEvidence Proof(MachineOperationSession run,
            MachineOperationVerificationKind kind, DateTimeOffset when, bool saved = false)
        {
            return new MachineOperationEvidence
            {
                RunId = run.Snapshot.RunId,
                StepKey = run.Snapshot.ActiveStepKey,
                Name = "centering proof",
                Value = "X 0.01 Y -0.02",
                Unit = "mm",
                Source = "Position camera",
                CapturedAtUtc = when,
                Verification = kind,
                Saved = saved,
                VisionProfileKey = "feederorigin",
                PixelsPerMm = 90.9
            };
        }

        [Test]
        public void FirstSnapshotPersistsDefinitionIdentityAndMachineTarget()
        {
            var session = Run();
            session.Start(StartedAt.AddSeconds(1));
            var record = MachineOperationRunProjection.Project(
                session.Snapshot, session.Definition, Org, User);

            Assert.That(record.Id, Is.EqualTo(session.Snapshot.RunId.ToUpperInvariant()));
            Assert.That(record.OwnerOrganization.Id, Is.EqualTo(Org.Id));
            Assert.That(record.MachineId, Is.EqualTo("lumen-pnp"));
            Assert.That(record.FeederId, Is.EqualTo("feeder-1"));
            Assert.That(record.RowIndex, Is.EqualTo(1));
            Assert.That(record.Definition.Version, Is.EqualTo(1));
            Assert.That(record.Snapshot.Steps.Count, Is.EqualTo(7));
        }

        [Test]
        public void SerializedHistoryPreservesSavedVersusMeasuredAndEvidenceSource()
        {
            var session = Run();
            session.Start(StartedAt.AddSeconds(1));
            session.RecordEvidence(Proof(session,
                MachineOperationVerificationKind.MachineObserved, StartedAt.AddSeconds(2)));
            session.CompleteActiveStep("approach-origin", StartedAt.AddSeconds(3));
            session.RecordEvidence(Proof(session,
                MachineOperationVerificationKind.VisionVerified, StartedAt.AddSeconds(4)));
            session.CompleteActiveStep("center-fiducial", StartedAt.AddSeconds(5));
            session.RecordEvidence(Proof(session,
                MachineOperationVerificationKind.OperatorConfirmed, StartedAt.AddSeconds(6), true));
            session.CompleteActiveStep("save-origin", StartedAt.AddSeconds(7));

            var record = MachineOperationRunProjection.Project(
                session.Snapshot, session.Definition, Org, User);
            var copy = JsonConvert.DeserializeObject<LagoVista.Manufacturing.Models.MachineOperationRunRecord>(
                JsonConvert.SerializeObject(record));
            Assert.That(copy.Snapshot.DefinitionVersion, Is.EqualTo(1));
            Assert.That(copy.Snapshot.Steps[2].Evidence[0].Saved, Is.True);
            Assert.That(copy.Snapshot.Steps[1].Evidence[0].Saved, Is.False);
            Assert.That(copy.Snapshot.Steps[1].Evidence[0].Verification,
                Is.EqualTo(MachineOperationVerificationKind.VisionVerified));
            Assert.That(copy.Snapshot.Steps[1].Evidence[0].PixelsPerMm, Is.EqualTo(90.9));
            Assert.That(copy.Snapshot.Steps[2].Status, Is.EqualTo(MachineOperationStepStatus.Passed));
        }

        [Test]
        public void RepeatedDeliveryWithSameTimestampIsIdempotent()
        {
            var session = Run();
            session.Start(StartedAt.AddSeconds(1));
            var record = MachineOperationRunProjection.Project(
                session.Snapshot, session.Definition, Org, User);
            var repeated = MachineOperationRunProjection.Project(
                session.Snapshot, session.Definition, Org, User, record);
            Assert.That(repeated, Is.SameAs(record));
        }

        [Test]
        public void OlderDeliveryCannotRollBackNewerRecord()
        {
            var session = Run();
            session.Start(StartedAt.AddSeconds(1));
            var older = JsonConvert.DeserializeObject<MachineOperationRunSnapshot>(
                JsonConvert.SerializeObject(session.Snapshot));
            session.RecordEvidence(Proof(session,
                MachineOperationVerificationKind.MachineObserved, StartedAt.AddSeconds(2)));
            session.CompleteActiveStep("approach-origin", StartedAt.AddSeconds(3));
            var record = MachineOperationRunProjection.Project(
                session.Snapshot, session.Definition, Org, User);
            Assert.That(MachineOperationRunProjection.Project(
                older, session.Definition, Org, User, record), Is.SameAs(record));
        }

        [Test]
        public void ARunCannotBeOverwrittenByAnotherOrganization()
        {
            var session = Run();
            session.Start(StartedAt.AddSeconds(1));
            var record = MachineOperationRunProjection.Project(
                session.Snapshot, session.Definition, Org, User);
            Assert.Throws<InvalidOperationException>(() =>
                MachineOperationRunProjection.Project(session.Snapshot, session.Definition,
                    EntityHeader.Create("org-2", "Another Org"), User, record));
        }

        [Test]
        public void MerelyClaimingASavedStepPassedWithoutEvidenceIsRejected()
        {
            var session = Run();
            session.Start(StartedAt.AddSeconds(1));
            session.Snapshot.Steps[0].Status = MachineOperationStepStatus.Passed;
            Assert.Throws<ArgumentException>(() =>
                MachineOperationRunProjection.Project(session.Snapshot, session.Definition, Org, User));
        }

        [Test]
        public void FailedSaveDoesNotSynthesizeSavedEvidence()
        {
            var session = Run();
            session.Start(StartedAt.AddSeconds(1));
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.MachineObserved,
                StartedAt.AddSeconds(2)));
            session.CompleteActiveStep("approach-origin", StartedAt.AddSeconds(3));
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.VisionVerified,
                StartedAt.AddSeconds(4)));
            session.CompleteActiveStep("center-fiducial", StartedAt.AddSeconds(5));
            session.RecordEvidence(Proof(session, MachineOperationVerificationKind.OperatorConfirmed,
                StartedAt.AddSeconds(6), saved: false));
            session.BlockActiveStep("Manufacturing PUT rejected the update", StartedAt.AddSeconds(7));

            var record = MachineOperationRunProjection.Project(
                session.Snapshot, session.Definition, Org, User);
            Assert.That(record.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Blocked));
            Assert.That(record.Snapshot.Steps[2].Evidence.All(e => !e.Saved), Is.True);
            Assert.That(record.Snapshot.LastError, Does.Contain("Manufacturing PUT"));
        }
    }
}
