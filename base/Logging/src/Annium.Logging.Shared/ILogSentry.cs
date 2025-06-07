using System;

namespace Annium.Logging.Shared;

/// <summary>
/// Interface for log message registration and handling coordination
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public interface ILogSentry<TContext>
    where TContext : class
{
    /// <summary>
    /// Registers a log message for processing
    /// </summary>
    /// <param name="message">The log message to register</param>
    void Register(LogMessage<TContext> message);

    /// <summary>
    /// Sets the handler for processing registered log messages
    /// </summary>
    /// <param name="handler">The handler action</param>
    void SetHandler(Action<LogMessage<TContext>> handler);
}
