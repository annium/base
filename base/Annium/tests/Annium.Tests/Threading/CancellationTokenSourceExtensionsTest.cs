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
        public Duration LastDuration { get; private set; }
        private Action? _pending;

        public Action Delay(Action handle, int timeout) => Delay(handle, Duration.FromMilliseconds(timeout));

        public Action Delay(Action handle, Duration timeout)
        {
            LastDuration = timeout;
            _pending = handle;
            return () => _pending = null;
        }

        public Action Interval(Action handle, int interval) => throw new NotSupportedException();

        public Action Interval(Action handle, Duration interval) => throw new NotSupportedException();

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
