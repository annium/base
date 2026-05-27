using System;
using System.Threading;
using Annium.Testing;
using Annium.Threading;
using NodaTime;
using Xunit;

namespace Annium.Tests.Threading;

/// <summary>
/// Tests for <see cref="CancellationTokenSourceExtensions"/>.
/// </summary>
public class CancellationTokenSourceExtensionsTest
{
    /// <summary>
    /// A minimal <see cref="IActionScheduler"/> for tests that records the delay it was asked
    /// to schedule and invokes the callback synchronously when <see cref="Fire"/> is called.
    /// </summary>
    private sealed class FakeScheduler : IActionScheduler
    {
        /// <summary>Gets the duration that was passed to the most recent <c>Delay</c> call.</summary>
        public Duration LastDuration { get; private set; }

        /// <summary>The callback registered by the last <c>Delay</c> call, cleared when cancelled.</summary>
        private Action? _pending;

        /// <summary>
        /// Schedules <paramref name="handle"/> to fire after <paramref name="timeout"/> milliseconds.
        /// Delegates to the <see cref="Duration"/>-based overload.
        /// </summary>
        /// <param name="handle">The callback to invoke when the delay fires.</param>
        /// <param name="timeout">The delay in milliseconds.</param>
        /// <returns>A cancellation action that clears the pending callback.</returns>
        public Action Delay(Action handle, int timeout) => Delay(handle, Duration.FromMilliseconds(timeout));

        /// <summary>
        /// Records <paramref name="timeout"/> in <see cref="LastDuration"/>, stores <paramref name="handle"/>
        /// as the pending action, and returns a cancellation delegate that clears it.
        /// </summary>
        /// <param name="handle">The callback to invoke when the delay fires.</param>
        /// <param name="timeout">The delay duration to record.</param>
        /// <returns>A cancellation action that clears the pending callback.</returns>
        public Action Delay(Action handle, Duration timeout)
        {
            LastDuration = timeout;
            _pending = handle;
            return () => _pending = null;
        }

        /// <summary>Not supported by this fake; always throws <see cref="NotSupportedException"/>.</summary>
        /// <param name="handle">Unused callback.</param>
        /// <param name="interval">Unused interval in milliseconds.</param>
        /// <returns>Never returns.</returns>
        public Action Interval(Action handle, int interval) => throw new NotSupportedException();

        /// <summary>Not supported by this fake; always throws <see cref="NotSupportedException"/>.</summary>
        /// <param name="handle">Unused callback.</param>
        /// <param name="interval">Unused interval duration.</param>
        /// <returns>Never returns.</returns>
        public Action Interval(Action handle, Duration interval) => throw new NotSupportedException();

        /// <summary>
        /// Synchronously invokes the pending callback registered by the last <c>Delay</c> call,
        /// simulating the scheduler firing.
        /// </summary>
        public void Fire() => _pending?.Invoke();
    }

    /// <summary>
    /// Verifies that <c>CancelAfter(IActionScheduler, Duration)</c> wires the cancellation
    /// through the scheduler — the cts cancels when the scheduler fires the registered delay,
    /// and the duration passed through is preserved verbatim.
    /// </summary>
    [Fact]
    public void CancelAfter_SchedulerOverload_CancelsAfterDuration()
    {
        var cts = new CancellationTokenSource();
        var scheduler = new FakeScheduler();
        var duration = Duration.FromMilliseconds(250);

        cts.CancelAfter(scheduler, duration);

        scheduler.LastDuration.Is(duration);
        cts.IsCancellationRequested.IsFalse();

        scheduler.Fire();

        cts.IsCancellationRequested.IsTrue();
    }
}
