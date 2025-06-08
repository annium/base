using System;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Immediate log scheduler that processes log messages synchronously without queueing
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class ImmediateLogScheduler<TContext> : ILogScheduler<TContext>
    where TContext : class
{
    /// <summary>
    /// Gets the filter function for determining which messages to process
    /// </summary>
    public Func<LogMessage<TContext>, bool> Filter { get; }

    /// <summary>
    /// The log handler for processing messages
    /// </summary>
    private readonly ILogHandler<TContext> _handler;

    public ImmediateLogScheduler(Func<LogMessage<TContext>, bool> filter, ILogHandler<TContext> handler)
    {
        Filter = filter;
        _handler = handler;
    }

    /// <summary>
    /// Handles a log message immediately
    /// </summary>
    /// <param name="message">The log message to handle</param>
    public void Handle(LogMessage<TContext> message)
    {
        _handler.Handle(message);
    }
}
