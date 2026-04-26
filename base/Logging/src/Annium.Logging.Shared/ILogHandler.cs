using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Logging.Shared;

/// <summary>
/// Canonical log handler contract — every sink implements this single interface.
/// Synchronous sinks return <see cref="ValueTask.CompletedTask"/> after performing
/// their work; naturally async sinks (file/network) inherit <see cref="BufferingLogHandler{TContext}"/>
/// which implements the interface for them.
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public interface ILogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Handles a batch of log messages. The scheduler chooses the dispatch shape: the
    /// <c>ImmediateLogScheduler</c> calls this with one-message batches synchronously,
    /// while the <c>BackgroundLogScheduler</c> buffers messages and calls this with
    /// larger batches from a background pump.
    /// </summary>
    /// <param name="messages">The log messages to handle</param>
    /// <param name="ct">Cancellation token signalled when the scheduler is shutting down</param>
    /// <returns>A value task that completes when handling is done</returns>
    ValueTask HandleAsync(IReadOnlyList<LogMessage<TContext>> messages, CancellationToken ct);
}
