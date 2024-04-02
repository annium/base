using System;

namespace Annium.Logging.Shared;

public interface ILogSentry<TContext>
    where TContext : class
{
    void Register(LogMessage<TContext> message);

    void SetHandler(Action<LogMessage<TContext>> handler);
}
