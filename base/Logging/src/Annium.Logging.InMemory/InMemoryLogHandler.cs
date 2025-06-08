using System.Collections.Generic;
using Annium.Logging.Shared;

namespace Annium.Logging.InMemory;

/// <summary>
/// Log handler that stores log messages in memory for testing and debugging purposes.
/// Provides access to all logged messages through a read-only collection.
/// </summary>
/// <typeparam name="TContext">The type of the log context</typeparam>
public class InMemoryLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Gets the collection of all logged messages.
    /// </summary>
    public IReadOnlyList<LogMessage<TContext>> Logs => _logs;

    /// <summary>
    /// Internal storage for logged messages.
    /// </summary>
    private readonly List<LogMessage<TContext>> _logs = new();

    /// <summary>
    /// Handles a log message by storing it in memory.
    /// </summary>
    /// <param name="message">The log message to store</param>
    public void Handle(LogMessage<TContext> message)
    {
        _logs.Add(message);
    }
}
