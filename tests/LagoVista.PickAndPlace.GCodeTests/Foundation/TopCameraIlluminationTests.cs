using System;
using System.Collections.Generic;
using System.Threading;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class TopCameraIlluminationTests
    {
        private static readonly TimeSpan Window = TimeSpan.FromSeconds(5);

        [Test]
        public void IdleAndProfileOffNeverEnergize()
        {
            var commands = new List<bool>();
            var gate = new TopCameraIllumination(commands.Add);
            gate.Stop();
            using (var lease = gate.Begin(false, Window))
                lease.ThrowIfInvalid();
            Assert.That(commands, Is.EqualTo(new[] { false, false, false }));
        }

        [Test]
        public void CompletionAndExceptionBothSubmitOff()
        {
            var commands = new List<bool>();
            var gate = new TopCameraIllumination(commands.Add);
            using (var lease = gate.Begin(true, Window))
                lease.ThrowIfInvalid();
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (gate.Begin(true, Window))
                    throw new InvalidOperationException("Acquisition failed");
            });
            Assert.That(commands, Is.EqualTo(new[] { true, false, true, false }));
        }

        [Test]
        public void CompetingAcquisitionCannotExtendOrInterruptCurrentWindow()
        {
            var commands = new List<bool>();
            var gate = new TopCameraIllumination(commands.Add);
            using (var lease = gate.Begin(true, Window))
            {
                Assert.Throws<InvalidOperationException>(() => gate.Begin(true, Window));
                lease.ThrowIfInvalid();
                Assert.That(commands, Is.EqualTo(new[] { true }));
            }
            Assert.That(commands, Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void CancellationImmediatelyRevokesAndSubmitsOff()
        {
            var commands = new List<bool>();
            var gate = new TopCameraIllumination(commands.Add);
            using (var source = new CancellationTokenSource())
            using (var lease = gate.Begin(true, Window, source.Token))
            {
                source.Cancel();
                Assert.Throws<OperationCanceledException>(() => lease.ThrowIfInvalid());
                Assert.That(commands, Is.EqualTo(new[] { true, false }));
            }
        }

        [Test]
        public void ExpirySubmitsOffWithoutAnObservationOrDisposal()
        {
            using (var off = new ManualResetEventSlim())
            {
                var gate = new TopCameraIllumination(on => { if (!on) off.Set(); });
                using (var lease = gate.Begin(true, TimeSpan.FromMilliseconds(100)))
                {
                    Assert.That(off.Wait(TimeSpan.FromSeconds(3)), Is.True);
                    Assert.Throws<TimeoutException>(() => lease.ThrowIfInvalid());
                }
            }
        }

        [Test]
        public void ExplicitStopInvalidatesOldLeaseAndOldDisposalCannotStopNewLease()
        {
            var commands = new List<bool>();
            var gate = new TopCameraIllumination(commands.Add);
            var old = gate.Begin(true, Window);
            gate.Stop();
            using (var current = gate.Begin(true, Window))
            {
                old.Dispose();
                Assert.Throws<InvalidOperationException>(() => old.ThrowIfInvalid());
                current.ThrowIfInvalid();
                Assert.That(commands, Is.EqualTo(new[] { true, false, true }));
            }
            Assert.That(commands, Is.EqualTo(new[] { true, false, true, false }));
        }

        [Test]
        public void CancelledOldLeaseCannotAffectNewPermission()
        {
            var commands = new List<bool>();
            var gate = new TopCameraIllumination(commands.Add);
            using (var cancellation = new CancellationTokenSource())
            using (var old = gate.Begin(true, Window, cancellation.Token))
            {
                gate.Stop();
                using (var current = gate.Begin(true, Window))
                {
                    cancellation.Cancel();
                    current.ThrowIfInvalid();
                    Assert.That(commands, Is.EqualTo(new[] { true, false, true }));
                }
            }
        }

        [Test]
        public void FailedOnAttemptsOffAndAllowsSubsequentPermission()
        {
            var commands = new List<bool>();
            var fail = true;
            var gate = new TopCameraIllumination(on =>
            {
                commands.Add(on);
                if (on && fail) throw new InvalidOperationException("ON failed");
            });
            Assert.Throws<InvalidOperationException>(() => gate.Begin(true, Window));
            fail = false;
            using (gate.Begin(true, Window)) { }
            Assert.That(commands, Is.EqualTo(new[] { true, false, true, false }));
        }

        [Test]
        public void OffFailureIsReportedToOwner()
        {
            var gate = new TopCameraIllumination(on =>
            {
                if (!on) throw new InvalidOperationException("OFF failed");
            });
            var lease = gate.Begin(true, Window);
            Assert.Throws<InvalidOperationException>(() => lease.Dispose());
            Assert.That(lease.OffError, Is.Not.Null);
        }

        [Test]
        public void PreCancelledOperationNeverSubmitsOn()
        {
            var commands = new List<bool>();
            var gate = new TopCameraIllumination(commands.Add);
            using (var source = new CancellationTokenSource())
            {
                source.Cancel();
                Assert.Throws<OperationCanceledException>(() => gate.Begin(true, Window, source.Token));
            }
            Assert.That(commands, Is.Empty);
        }
    }
}
