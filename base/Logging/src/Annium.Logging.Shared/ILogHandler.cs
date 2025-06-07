namespace Annium.Logging.Shared;

/// <summary>
/// Interface for synchronous log message handlers
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public interface ILogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Handles a log message synchronously
    /// </summary>
    /// <param name="message">The log message to handle</param>
    void Handle(LogMessage<TContext> message);
}
