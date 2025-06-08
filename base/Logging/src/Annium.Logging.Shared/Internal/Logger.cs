using System;
using System.Collections.Generic;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Internal logger implementation that forwards messages to the log sentry bridge
/// </summary>
internal class Logger : ILogger
{
    /// <summary>
    /// The log sentry bridge for forwarding log messages
    /// </summary>
    private readonly ILogSentryBridge _sentryBridge;

    public Logger(ILogSentryBridge sentryBridge)
    {
        _sentryBridge = sentryBridge;
    }

    /// <summary>
    /// Logs a message with the specified parameters
    /// </summary>
    /// <param name="subject">The logging subject</param>
    /// <param name="file">The source file</param>
    /// <param name="member">The member name</param>
    /// <param name="line">The line number</param>
    /// <param name="level">The log level</param>
    /// <param name="message">The log message</param>
    /// <param name="data">Additional data items</param>
    public void Log(
        object subject,
        string file,
        string member,
        int line,
        LogLevel level,
        string message,
        IReadOnlyList<object?> data
    )
    {
        var subjectType = subject is ILogBridge bridge ? bridge.Type : subject.GetType().FriendlyName();
        var subjectId = subject.GetId();

        _sentryBridge.Register(subjectType, subjectId, file, member, line, level, message, null, data);
    }

    /// <summary>
    /// Logs an error with exception information
    /// </summary>
    /// <param name="subject">The logging subject</param>
    /// <param name="file">The source file</param>
    /// <param name="member">The member name</param>
    /// <param name="line">The line number</param>
    /// <param name="ex">The exception to log</param>
    /// <param name="data">Additional data items</param>
    public void Error(object subject, string file, string member, int line, Exception ex, IReadOnlyList<object?> data)
    {
        var subjectType = subject is ILogBridge bridge ? bridge.Type : subject.GetType().FriendlyName();
        var subjectId = subject.GetId();

        _sentryBridge.Register(subjectType, subjectId, file, member, line, LogLevel.Error, ex.Message, ex, data);
    }
}
