using System;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Interface for log schedulers that handle and filter log messages
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public interface ILogScheduler<TContext>
    where TContext : class
{
    /// <summary>
    /// Gets the filter function for determining which messages to process
    /// </summary>
    Func<LogMessage<TContext>, bool> Filter { get; }

    /// <summary>
    /// Handles a log message
    /// </summary>
    /// <param name="message">The log message to handle</param>
    void Handle(LogMessage<TContext> message);
}
