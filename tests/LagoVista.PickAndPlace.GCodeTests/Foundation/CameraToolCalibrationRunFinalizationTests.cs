using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    [TestFixture]
    public class CameraToolCalibrationRunFinalizationTests
    {
        [Test]
        public async Task SharedHeadViewModelCannotAdvertiseCompletionWhileHistoryIsPending()
        {
            var upload = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var events = new List<string>();
            var sessionActive = true;
            var running = true;
            var finished = false;

            var operation = CameraToolCalibrationRunFinalization.FinishAsync(
                async () =>
                {
                    events.Add("upload-start");
                    await upload.Task;
                    events.Add("upload-done");
                },
                () => { sessionActive = false; events.Add("session-retired"); },
                () => { running = false; events.Add("idle"); },
                () => { finished = true; events.Add("finished"); });

            Assert.That(operation.IsCompleted, Is.False);
            Assert.That(sessionActive, Is.True, "Head 1 still owns its session during history I/O.");
            Assert.That(running, Is.True, "Head 2 cannot start before Head 1 history upload finishes.");
            Assert.That(finished, Is.False, "Batch sequencing must not auto-advance.");
            Assert.That(events, Is.EqualTo(new[] { "upload-start" }));

            upload.SetResult(true);
            await operation;

            Assert.That(sessionActive, Is.False);
            Assert.That(running, Is.False);
            Assert.That(finished, Is.True);
            Assert.That(events, Is.EqualTo(new[]
            {
                "upload-start", "upload-done", "session-retired", "idle", "finished"
            }));
        }

        [Test]
        public void FailedHistoryUploadStillRetiresSessionBeforeSignalingCompletion()
        {
            var events = new List<string>();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await CameraToolCalibrationRunFinalization.FinishAsync(
                    () => throw new InvalidOperationException("network failure"),
                    () => events.Add("session-retired"),
                    () => events.Add("idle"),
                    () => events.Add("finished")));

            Assert.That(events, Is.EqualTo(new[]
            {
                "session-retired", "idle", "finished"
            }));
        }
    }
}
