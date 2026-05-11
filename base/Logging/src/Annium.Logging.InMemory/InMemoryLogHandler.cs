using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Shared;

namespace Annium.Logging.InMemory;

/// <summary>
/// Log handler that stores log messages in memory for testing and debugging purposes.
/// Provides access to all logged messages through a read-only collection.
/// Thread-safe for concurrent producers.
/// </summary>
/// <typeparam name="TContext">The type of the log context</typeparam>
public sealed class InMemoryLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Gets a snapshot of all logged messages observed so far.
    /// </summary>
    public IReadOnlyList<LogMessage<TContext>> Logs => _logs.ToArray();

    /// <summary>
    /// Internal thread-safe storage for logged messages.
    /// </summary>
    private readonly ConcurrentQueue<LogMessage<TContext>> _logs = new();

    /// <summary>
    /// Stores each message in the batch into the in-memory queue.
    /// </summary>
    /// <param name="messages">The log messages to store</param>
    /// <param name="ct">Cancellation token (unused — storage is synchronous)</param>
    /// <returns>A completed value task</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<TContext>> messages, CancellationToken ct)
    {
        foreach (var msg in messages)
            _logs.Enqueue(msg);

        return ValueTask.CompletedTask;
    }
}
