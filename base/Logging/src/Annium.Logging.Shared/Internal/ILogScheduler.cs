using System;

namespace Annium.Logging.Shared.Internal;

public interface ILogScheduler<TContext>
    where TContext : class
{
    Func<LogMessage<TContext>, bool> Filter { get; }
    void Handle(LogMessage<TContext> message);
}
