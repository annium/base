using System;
using System.Collections.Generic;

namespace Annium.Logging;

/// <summary>
/// Represents a logger that does not perform any logging operations.
/// This class is used as a singleton instance when logging is disabled or not required.
/// </summary>
public class VoidLogger : ILogger
{
    /// <summary>
    /// Gets the singleton instance of the void logger.
    /// </summary>
    public static readonly ILogger Instance = new VoidLogger();

    /// <summary>
    /// Initializes a new instance of the <see cref="VoidLogger"/> class.
    /// This constructor is private to enforce the singleton pattern.
    /// </summary>
    private VoidLogger() { }

    /// <summary>
    /// Logs a message with the specified parameters. This implementation does nothing.
    /// </summary>
    /// <param name="subject">The subject of the log message.</param>
    /// <param name="file">The source file path.</param>
    /// <param name="member">The member name.</param>
    /// <param name="line">The line number.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="data">The data to include in the log message.</param>
    public void Log(
        object subject,
        string file,
        string member,
        int line,
        LogLevel level,
        string message,
        IReadOnlyList<object?> data
    ) { }

    /// <summary>
    /// Logs an error with the specified exception and parameters. This implementation does nothing.
    /// </summary>
    /// <param name="subject">The subject of the log message.</param>
    /// <param name="file">The source file path.</param>
    /// <param name="member">The member name.</param>
    /// <param name="line">The line number.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="data">The data to include in the log message.</param>
    public void Error(
        object subject,
        string file,
        string member,
        int line,
        Exception ex,
        IReadOnlyList<object?> data
    ) { }
}
