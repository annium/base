namespace Annium.Logging.Shared;

public interface ILogHandler<TContext>
    where TContext : class, ILogContext
{
    void Handle(LogMessage<TContext> message);
}