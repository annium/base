using System.Collections.Generic;

namespace Annium.Logging.Shared.Internal;

internal class LogRouter<TContext>
    where TContext : class, ILogContext
{
    private readonly IEnumerable<ILogScheduler<TContext>> _schedulers;

    public LogRouter(
        ILogSentry<TContext> sentry,
        IReadOnlyCollection<ILogScheduler<TContext>> schedulers
    )
    {
        sentry.SetHandler(Send);
        _schedulers = schedulers;
    }

    private void Send(LogMessage<TContext> msg)
    {
        foreach (var scheduler in _schedulers)
            if (scheduler.Filter(msg))
                scheduler.Handle(msg);
    }
}