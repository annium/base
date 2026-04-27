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

    /// <summary>
    /// Initializes a new instance of <see cref="LoggingBridge"/>.
    /// </summary>
    /// <param name="sentryBridge">The Annium log sentry bridge to forward log records to.</param>
    /// <param name="source">The source identifier attached to every dispatched log record.</param>
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
    /// Checks if the given logLevel is enabled by consulting the sentry's level gate, which
    /// evaluates the configured route filters. Returns false directly for
    /// <see cref="MicrosoftLogLevel.None"/> per Microsoft.Extensions.Logging convention
    /// (None means "no log"). Returns false when no route would accept a message at this level —
    /// letting Microsoft.Extensions.Logging short-circuit log construction.
    /// </summary>
    /// <param name="logLevel">Level to be checked</param>
    /// <returns>True if any registered route accepts this level; false for <see cref="MicrosoftLogLevel.None"/></returns>
    public bool IsEnabled(MicrosoftLogLevel logLevel)
    {
        if (logLevel == MicrosoftLogLevel.None)
            return false;
        return _sentryBridge.IsLevelEnabled(Map(logLevel));
    }

    /// <summary>
    /// Writes a log entry. Returns immediately without dispatching to the sentry when
    /// <paramref name="logLevel"/> is <see cref="MicrosoftLogLevel.None"/>, per
    /// Microsoft.Extensions.Logging convention (None means "no log").
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
        if (logLevel == MicrosoftLogLevel.None)
            return;

        _sentryBridge.Register(
            _source,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            Map(logLevel),
            formatter(state, exception),
            exception,
            Array.Empty<object?>()
        );
    }

    /// <summary>
    /// Maps Microsoft log level to Annium log level
    /// </summary>
    /// <param name="level">The Microsoft log level to map</param>
    /// <returns>The corresponding Annium log level</returns>
    private static LogLevel Map(MicrosoftLogLevel level) =>
        level switch
        {
            MicrosoftLogLevel.Trace => LogLevel.Trace,
            MicrosoftLogLevel.Debug => LogLevel.Debug,
            MicrosoftLogLevel.Information => LogLevel.Info,
            MicrosoftLogLevel.Warning => LogLevel.Warn,
            MicrosoftLogLevel.Error => LogLevel.Error,
            MicrosoftLogLevel.Critical => LogLevel.Error,
            _ => LogLevel.None,
        };
}
