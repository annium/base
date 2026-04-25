using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NodaTime;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Bridge implementation that converts log registration calls to log messages and forwards them to the sentry
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
internal class LogSentryBridge<TContext> : ILogSentryBridge
    where TContext : class
{
    /// <summary>
    /// Time provider for generating timestamps
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// The log context instance
    /// </summary>
    private readonly TContext _context;

    /// <summary>
    /// The log sentry for receiving log messages
    /// </summary>
    private readonly ILogSentry<TContext> _logSentry;

    /// <summary>
    /// Snapshot of registered schedulers used to evaluate <see cref="IsLevelEnabled"/>.
    /// </summary>
    private readonly IReadOnlyCollection<ILogScheduler<TContext>> _schedulers;

    public LogSentryBridge(
        ITimeProvider timeProvider,
        TContext context,
        ILogSentry<TContext> logSentry,
        IReadOnlyCollection<ILogScheduler<TContext>> schedulers
    )
    {
        _timeProvider = timeProvider;
        _context = context;
        _logSentry = logSentry;
        _schedulers = schedulers;
    }

    /// <summary>
    /// Reports whether any registered scheduler's filter would accept a message at the given level.
    /// </summary>
    /// <param name="level">The level to test</param>
    /// <returns>True if at least one scheduler's filter accepts; false otherwise</returns>
    public bool IsLevelEnabled(LogLevel level)
    {
        if (_schedulers.Count == 0)
            return false;

        // Synthetic message — only Level is meaningful. Filters that key off non-Level fields
        // evaluate against empty/default values (intentional — IsEnabled is an optimization hint).
        var synthetic = new LogMessage<TContext>(
            _context,
            Instant.MinValue,
            string.Empty,
            string.Empty,
            level,
            0,
            string.Empty,
            null,
            string.Empty,
            new Dictionary<string, object?>(),
            string.Empty,
            string.Empty,
            0
        );

        return _schedulers.Any(s => s.Filter(synthetic));
    }

    /// <summary>
    /// Registers a log message by creating a LogMessage and forwarding it to the sentry
    /// </summary>
    /// <param name="subjectType">The type of the logging subject</param>
    /// <param name="subjectId">The identifier of the logging subject</param>
    /// <param name="file">The source file where the log was generated</param>
    /// <param name="member">The member where the log was generated</param>
    /// <param name="line">The line number where the log was generated</param>
    /// <param name="level">The log level</param>
    /// <param name="messageTemplate">The message template</param>
    /// <param name="exception">The exception associated with the log</param>
    /// <param name="dataItems">Additional data items for the log</param>
    public void Register(
        string subjectType,
        string subjectId,
        string file,
        string member,
        int line,
        LogLevel level,
        string messageTemplate,
        Exception? exception,
        IReadOnlyList<object?> dataItems
    )
    {
        var instant = _timeProvider.Now;
        var (message, data) = Helper.Process(messageTemplate, dataItems);

        var msg = new LogMessage<TContext>(
            _context,
            instant,
            subjectType,
            subjectId,
            level,
            Thread.CurrentThread.ManagedThreadId,
            exception is null ? message : LogMessageEnricher.GetExceptionMessage(exception),
            exception?.Demystify(),
            messageTemplate,
            data,
            Path.GetFileNameWithoutExtension(file),
            member,
            line
        );

        _logSentry.Register(msg);
    }
}
