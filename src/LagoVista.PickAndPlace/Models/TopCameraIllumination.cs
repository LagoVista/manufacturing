using System;
using System.Diagnostics;
using System.Threading;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Exclusive, bounded host permission to energize the position-camera LED.
    /// The command callback must be synchronous, thread-safe, and must not
    /// block on the UI dispatcher. A submitted OFF is not physical feedback.
    /// The caller owns command acknowledgement and fresh-frame settling.
    /// </summary>
    public sealed class TopCameraIllumination
    {
        private readonly object _gate = new object();
        private readonly Action<bool> _command;
        private Lease _active;
        private Exception _offFault;

        public TopCameraIllumination(Action<bool> command)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public Lease Begin(bool requested, TimeSpan maximum, CancellationToken cancellation = default)
        {
            if (maximum <= TimeSpan.Zero || maximum.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(maximum));

            Lease lease;
            lock (_gate)
            {
                cancellation.ThrowIfCancellationRequested();
                if (_active != null)
                    throw new InvalidOperationException("Top-camera illumination is already in use.");
                if (_offFault != null)
                    throw new InvalidOperationException("Top-camera OFF previously failed. Explicitly switch OFF before starting another acquisition.", _offFault);

                lease = new Lease(this, maximum, cancellation);
                _active = lease;
                try
                {
                    // The watchdog and ON/OFF commands serialize. An old
                    // timer cannot revoke or energize a newer permission.
                    lease.Timer = new Timer(_ => End(lease, EndReason.Timeout),
                        null, maximum, Timeout.InfiniteTimeSpan);
                    _command(requested);
                }
                catch (Exception ex)
                {
                    End(lease, EndReason.Failed);
                    if (lease.OffError != null)
                        throw new AggregateException("Top-camera light start and cleanup failed.", ex, lease.OffError);
                    throw;
                }
            }

            // Register and dispose outside the lock; a cancellation callback
            // may already be executing and need the same lock to submit OFF.
            try
            {
                lease.Registration = cancellation.Register(() => End(lease, EndReason.Cancelled));
                lease.ThrowIfInvalid();
                return lease;
            }
            catch
            {
                lease.Dispose();
                throw;
            }
        }

        /// <summary>Revoke any permission; also submit OFF when already idle.</summary>
        public void Stop()
        {
            lock (_gate)
            {
                if (_active == null)
                {
                    try
                    {
                        _command(false);
                        _offFault = null;
                    }
                    catch (Exception ex)
                    {
                        _offFault = ex;
                        throw;
                    }
                }
                else
                {
                    var lease = _active;
                    End(lease, EndReason.Stopped);
                    if (lease.OffError != null)
                        throw new InvalidOperationException("Top-camera OFF command failed.", lease.OffError);
                }
            }
        }

        private void End(Lease lease, EndReason reason)
        {
            lock (_gate)
            {
                if (!ReferenceEquals(_active, lease))
                    return;
                lease.Reason = reason;
                lease.Timer?.Dispose();
                try
                {
                    _command(false);
                }
                catch (Exception ex)
                {
                    // A watchdog callback must never crash the process. Retain
                    // the failure for the acquisition owner to report.
                    lease.OffError = ex;
                    _offFault = ex;
                }
                finally
                {
                    _active = null;
                }
            }
        }

        public sealed class Acquisition : IAsyncDisposable
        {
            private readonly Lease _lease;
            private readonly Func<System.Threading.Tasks.Task> _waitForOff;
            private int _disposed;

            public Acquisition(Lease lease, Func<System.Threading.Tasks.Task> waitForOff)
            {
                _lease = lease;
                _waitForOff = waitForOff;
            }

            public void ThrowIfInvalid() => _lease.ThrowIfInvalid();

            public async System.Threading.Tasks.ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
                _lease.Dispose();
                await _waitForOff();
            }
        }

        internal enum EndReason { Active, Completed, Stopped, Cancelled, Timeout, Failed }

        public sealed class Lease : IDisposable
        {
            private readonly TopCameraIllumination _owner;
            private readonly Stopwatch _elapsed = Stopwatch.StartNew();
            private readonly TimeSpan _maximum;
            private readonly CancellationToken _cancellation;
            internal Timer Timer;
            internal CancellationTokenRegistration Registration;
            internal EndReason Reason { get; set; }
            public Exception OffError { get; internal set; }

            internal Lease(TopCameraIllumination owner, TimeSpan maximum, CancellationToken cancellation)
            {
                _owner = owner;
                _maximum = maximum;
                _cancellation = cancellation;
            }

            /// <summary>Check before accepting an observation or issuing a vision-derived move.</summary>
            public void ThrowIfInvalid()
            {
                lock (_owner._gate)
                {
                    if (_cancellation.IsCancellationRequested)
                        _owner.End(this, EndReason.Cancelled);
                    else if (_elapsed.Elapsed >= _maximum)
                        _owner.End(this, EndReason.Timeout);

                    if (OffError != null)
                        throw new InvalidOperationException("Top-camera OFF command failed.", OffError);
                    if (Reason == EndReason.Timeout)
                        throw new TimeoutException("Top-camera illumination window expired; discard the observation.");
                    if (Reason == EndReason.Cancelled)
                        throw new OperationCanceledException(_cancellation);
                    if (Reason != EndReason.Active || !ReferenceEquals(_owner._active, this))
                        throw new InvalidOperationException("Top-camera illumination permission was revoked.");
                }
            }

            public void Dispose()
            {
                _owner.End(this, EndReason.Completed);
                Registration.Dispose();
                if (OffError != null)
                    throw new InvalidOperationException("Top-camera OFF command failed.", OffError);
            }
        }
    }
}
