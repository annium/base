using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Shared test helpers used across multiple test classes in Annium.Logging.Shared.Tests.
/// </summary>
internal static class LoggingTestHelpers
{
    /// <summary>
    /// Constructs a synthetic <see cref="LogMessage{TContext}"/> with the given sequence number
    /// used as both the message discriminator and the instant ticks value.
    /// </summary>
    /// <param name="seq">Sequence number used as instant ticks and embedded in the message text.</param>
    /// <returns>A <see cref="LogMessage{DefaultLogContext}"/> suitable for scheduler/sentry plumbing tests.</returns>
    internal static LogMessage<DefaultLogContext> BuildMessage(int seq) =>
        new(
            new DefaultLogContext(),
            Instant.FromUnixTimeTicks(seq),
            "test",
            "id",
            LogLevel.Info,
            0,
            $"msg-{seq}",
            null,
            string.Empty,
            new Dictionary<string, object?>(),
            "type",
            "member",
            0
        );
}

/// <summary>
/// Minimal <see cref="ILogHandler{TContext}"/> that completes immediately without processing
/// any messages. Used by constructor-guard tests and disposal tests where no handler
/// invocation is expected.
/// </summary>
internal sealed class NoOpSink : ILogHandler<DefaultLogContext>
{
    /// <summary>
    /// Immediately completes without processing the batch.
    /// </summary>
    /// <param name="messages">The batch of log messages (ignored).</param>
    /// <param name="ct">Cancellation token (ignored).</param>
    /// <returns>A completed value task.</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct) =>
        ValueTask.CompletedTask;
}

/// <summary>
/// Minimal non-buffering <see cref="ILogHandler{TContext}"/> that completes immediately.
/// Used as a placeholder handler for route registration and scheduler-selection assertions
/// where the handler body is never invoked.
/// </summary>
internal sealed class SyncSink : ILogHandler<DefaultLogContext>
{
    /// <summary>
    /// Immediately completes without processing the batch — used only for route registration
    /// and scheduler selection assertions.
    /// </summary>
    /// <param name="messages">The batch of log messages (ignored).</param>
    /// <param name="ct">Cancellation token (ignored).</param>
    /// <returns>A completed value task.</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct) =>
        ValueTask.CompletedTask;
}

/// <summary>
/// Spy handler implementing only <see cref="IDisposable"/>. Counts Dispose calls.
/// Shared across <c>ImmediateLogSchedulerDisposalTests</c> and <c>ProviderOnDisposedChainTests</c>.
/// </summary>
internal sealed class DisposableSink : ILogHandler<DefaultLogContext>, IDisposable
{
    /// <summary>Number of times Dispose was called.</summary>
    private int _disposeCount;

    /// <summary>Thread-safe snapshot of Dispose invocations.</summary>
    public int DisposeCount => Volatile.Read(ref _disposeCount);

    /// <summary>Records a disposal by incrementing the dispose count.</summary>
    public void Dispose() => Interlocked.Increment(ref _disposeCount);

    /// <summary>Completes immediately — handler body is not exercised by disposal tests.</summary>
    /// <param name="messages">The batch of log messages (ignored).</param>
    /// <param name="ct">Cancellation token (ignored).</param>
    /// <returns>A completed value task.</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct) =>
        ValueTask.CompletedTask;
}

/// <summary>
/// Spy handler implementing <see cref="IAsyncDisposable"/> (and <see cref="ILogHandler{TContext}"/>).
/// Counts DisposeAsync calls. The IAsyncDisposable arm has priority over IDisposable in the scheduler.
/// Shared across <c>ImmediateLogSchedulerDisposalTests</c> and <c>ProviderOnDisposedChainTests</c>.
/// </summary>
internal sealed class AsyncDisposableSink : ILogHandler<DefaultLogContext>, IAsyncDisposable
{
    /// <summary>Number of times DisposeAsync was called.</summary>
    private int _disposeCount;

    /// <summary>Thread-safe snapshot of DisposeAsync invocations.</summary>
    public int DisposeCount => Volatile.Read(ref _disposeCount);

    /// <summary>Records an async disposal by incrementing the dispose count.</summary>
    /// <returns>A completed value task.</returns>
    public ValueTask DisposeAsync()
    {
        Interlocked.Increment(ref _disposeCount);
        return ValueTask.CompletedTask;
    }

    /// <summary>Completes immediately — handler body is not exercised by disposal tests.</summary>
    /// <param name="messages">The batch of log messages (ignored).</param>
    /// <param name="ct">Cancellation token (ignored).</param>
    /// <returns>A completed value task.</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<DefaultLogContext>> messages, CancellationToken ct) =>
        ValueTask.CompletedTask;
}
