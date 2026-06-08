using System;
using System.Threading.Tasks;
using Annium.Logging.Shared.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Tests for <see cref="BackgroundLogScheduler{TContext}"/> disposal behavior:
/// idempotent sequential double-dispose, concurrent double-dispose safety, and
/// Handle-after-dispose throws.
/// </summary>
public class BackgroundLogSchedulerDisposalTests
{
    /// <summary>
    /// Sequential double-DisposeAsync is a safe no-op — the second call must complete
    /// without throwing.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwiceSequentially_SecondCallIsNoOp()
    {
        var scheduler = BuildScheduler();

        await scheduler.DisposeAsync();

        // second call must not throw
        await scheduler.DisposeAsync();
    }

    /// <summary>
    /// Concurrent double-DisposeAsync — two tasks both awaiting DisposeAsync simultaneously —
    /// both complete without throwing.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_CalledConcurrently_BothCompleteWithoutThrowing()
    {
        var scheduler = BuildScheduler();

        var t1 = scheduler.DisposeAsync().AsTask();
        var t2 = scheduler.DisposeAsync().AsTask();

        // both tasks must complete without exception
        await Task.WhenAll(t1, t2);
    }

    /// <summary>
    /// Handle called after DisposeAsync must throw <see cref="InvalidOperationException"/>
    /// with the canonical "Log scheduler is already disposed" message.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Handle_AfterDisposeAsync_ThrowsInvalidOperationException()
    {
        var scheduler = BuildScheduler();
        await scheduler.DisposeAsync();

        Wrap.It(() => scheduler.Handle(LoggingTestHelpers.BuildMessage(0))).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Constructs a scheduler with a minimal configuration (fast buffer flush)
    /// and a no-op handler so the pump task can drain immediately.
    /// </summary>
    /// <returns>A new <see cref="BackgroundLogScheduler{TContext}"/> instance ready for use in tests.</returns>
    private static BackgroundLogScheduler<DefaultLogContext> BuildScheduler() =>
        new(
            _ => true,
            new NoOpSink(),
            new LogRouteConfiguration { BufferTime = TimeSpan.FromMilliseconds(10), BufferCount = 1 }
        );
}
