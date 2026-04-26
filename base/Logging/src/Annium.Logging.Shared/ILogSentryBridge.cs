using System;
using System.Collections.Generic;

namespace Annium.Logging.Shared;

/// <summary>
/// Interface for bridging log messages from external logging systems to the log sentry
/// </summary>
public interface ILogSentryBridge
{
    /// <summary>
    /// Reports whether any registered route would accept a message at the given level.
    /// Used by external-bridge implementations (e.g., the Microsoft.Extensions.Logging bridge)
    /// to short-circuit log construction when no sink is interested. Filters that key off
    /// non-Level fields evaluate against an empty synthetic message — this is an optimization
    /// hint, not a strict gate; the actual filter still runs on real messages in the router.
    /// </summary>
    /// <param name="level">The level to test</param>
    /// <returns>True if at least one route accepts a message of this level; false otherwise</returns>
    bool IsLevelEnabled(LogLevel level);

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
