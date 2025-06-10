using System;
using System.Collections.Generic;

namespace Annium.Logging.Shared;

/// <summary>
/// Interface for bridging log messages from external logging systems to the log sentry
/// </summary>
public interface ILogSentryBridge
{
    /// <summary>
    /// Registers a log message with the sentry
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
    void Register(
        string subjectType,
        string subjectId,
        string file,
        string member,
        int line,
        LogLevel level,
        string messageTemplate,
        Exception? exception,
        IReadOnlyList<object?> dataItems
    );
}
