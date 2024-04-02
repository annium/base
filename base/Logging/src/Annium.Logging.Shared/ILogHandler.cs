namespace Annium.Logging.Shared;

public interface ILogHandler<TContext>
    where TContext : class
{
    void Handle(LogMessage<TContext> message);
}
