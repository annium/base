using System;

namespace Annium.Logging.Shared.Internal;

internal class ImmediateLogScheduler<TContext> : ILogScheduler<TContext>
    where TContext : class, ILogContext
{
    public Func<LogMessage<TContext>, bool> Filter { get; }
    private readonly ILogHandler<TContext> _handler;

    public ImmediateLogScheduler(
        Func<LogMessage<TContext>, bool> filter,
        ILogHandler<TContext> handler
    )
    {
        Filter = filter;
        _handler = handler;
    }

    public void Handle(LogMessage<TContext> message)
    {
        _handler.Handle(message);
    }
}