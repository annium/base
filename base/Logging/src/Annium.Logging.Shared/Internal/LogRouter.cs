using System.Collections.Generic;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Routes log messages to appropriate schedulers based on their filters
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class LogRouter<TContext>
    where TContext : class
{
    /// <summary>
    /// The collection of log schedulers to route messages to
    /// </summary>
    private readonly IEnumerable<ILogScheduler<TContext>> _schedulers;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogRouter{TContext}"/> class and subscribes it to the sentry.
    /// </summary>
    /// <param name="sentry">Sentry whose messages are routed.</param>
    /// <param name="schedulers">Schedulers a routed message is offered to.</param>
    public LogRouter(ILogSentry<TContext> sentry, IReadOnlyCollection<ILogScheduler<TContext>> schedulers)
    {
        sentry.SetHandler(Send);
        _schedulers = schedulers;
    }

    /// <summary>
    /// Sends a log message to all applicable schedulers
    /// </summary>
    /// <param name="msg">The log message to send</param>
    private void Send(LogMessage<TContext> msg)
    {
        foreach (var scheduler in _schedulers)
            if (scheduler.Filter(msg))
                scheduler.Handle(msg);
    }
}
