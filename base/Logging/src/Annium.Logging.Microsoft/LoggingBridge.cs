using System;
using Annium.Logging.Shared;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using MicrosoftEventId = Microsoft.Extensions.Logging.EventId;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Annium.Logging.Microsoft;

/// <summary>
/// Bridge implementation that adapts Microsoft.Extensions.Logging to Annium logging system
/// </summary>
internal class LoggingBridge : IMicrosoftLogger
{
    /// <summary>
    /// The log sentry bridge for forwarding log messages
    /// </summary>
    private readonly ILogSentryBridge _sentryBridge;

    /// <summary>
    /// The source identifier for log messages
    /// </summary>
    private readonly string _source;

    public LoggingBridge(ILogSentryBridge sentryBridge, string source)
    {
        _sentryBridge = sentryBridge;
        _source = source;
    }

    /// <summary>
    /// Begins a logical operation scope
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for</typeparam>
    /// <param name="state">The identifier for the scope</param>
    /// <returns>An IDisposable that ends the logical operation scope on dispose</returns>
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull => Disposable.Empty;

    /// <summary>
    /// Checks if the given logLevel is enabled
    /// </summary>
    /// <param name="logLevel">Level to be checked</param>
    /// <returns>True if enabled</returns>
    public bool IsEnabled(MicrosoftLogLevel logLevel) => true;

    /// <summary>
    /// Writes a log entry
    /// </summary>
    /// <typeparam name="TState">The type of the object to be written</typeparam>
    /// <param name="logLevel">Entry will be written on this level</param>
    /// <param name="eventId">Id of the event</param>
    /// <param name="state">The entry to be written. Can be also an object</param>
    /// <param name="exception">The exception related to this entry</param>
    /// <param name="formatter">Function to create a string message of the state and exception</param>
    public void Log<TState>(
        MicrosoftLogLevel logLevel,
        MicrosoftEventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        _sentryBridge.Register(
            _source,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            Map(logLevel),
            formatter(state, exception),
            exception,
            Array.Empty<object>()
        );
    }

    /// <summary>
    /// Maps Microsoft log level to Annium log level
    /// </summary>
    /// <param name="level">The Microsoft log level to map</param>
    /// <returns>The corresponding Annium log level</returns>
    private LogLevel Map(MicrosoftLogLevel level)
    {
        switch (level)
        {
            case MicrosoftLogLevel.Trace:
                return LogLevel.Trace;
            case MicrosoftLogLevel.Debug:
                return LogLevel.Debug;
            case MicrosoftLogLevel.Information:
                return LogLevel.Info;
            case MicrosoftLogLevel.Warning:
                return LogLevel.Warn;
            case MicrosoftLogLevel.Error:
            case MicrosoftLogLevel.Critical:
                return LogLevel.Error;
            default:
                return LogLevel.None;
        }
    }
}
