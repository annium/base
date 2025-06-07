using System;
using System.Collections.Generic;

namespace Annium.Logging;

/// <summary>
/// Defines the interface for logging functionality, providing methods to log messages and errors.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs a message with the specified parameters.
    /// </summary>
    /// <param name="subject">The subject object being logged.</param>
    /// <param name="file">The source file where the log was generated.</param>
    /// <param name="member">The member (method/property) where the log was generated.</param>
    /// <param name="line">The line number where the log was generated.</param>
    /// <param name="level">The log level of the message.</param>
    /// <param name="message">The message to be logged.</param>
    /// <param name="data">Additional data to be included in the log entry.</param>
    void Log(
        object subject,
        string file,
        string member,
        int line,
        LogLevel level,
        string message,
        IReadOnlyList<object?> data
    );

    /// <summary>
    /// Logs an error with exception details.
    /// </summary>
    /// <param name="subject">The subject object being logged.</param>
    /// <param name="file">The source file where the error occurred.</param>
    /// <param name="member">The member (method/property) where the error occurred.</param>
    /// <param name="line">The line number where the error occurred.</param>
    /// <param name="ex">The exception that was thrown.</param>
    /// <param name="data">Additional data to be included in the log entry.</param>
    void Error(object subject, string file, string member, int line, Exception ex, IReadOnlyList<object?> data);
}
