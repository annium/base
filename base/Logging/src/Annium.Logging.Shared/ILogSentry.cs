using System;

namespace Annium.Logging.Shared;

public interface ILogSentry<TContext>
    where TContext : class, ILogContext
{
    void Register(LogMessage<TContext> message);

    void SetHandler(Action<LogMessage<TContext>> handler);
}