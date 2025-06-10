using System;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Logging.Shared;

/// <summary>
/// Represents a log message with context and metadata
/// </summary>
/// <typeparam name="TContext">The type of log context</typeparam>
public record LogMessage<TContext>
    where TContext : class
{
    /// <summary>
    /// Gets the log context
    /// </summary>
    public TContext Context { get; }

    /// <summary>
    /// Gets the timestamp when the log message was created
    /// </summary>
    public Instant Instant { get; }

    /// <summary>
    /// Gets the type name of the logging subject
    /// </summary>
    public string SubjectType { get; }

    /// <summary>
    /// Gets the identifier of the logging subject
    /// </summary>
    public string SubjectId { get; }

    /// <summary>
    /// Gets the log level
    /// </summary>
    public LogLevel Level { get; }

    /// <summary>
    /// Gets the thread ID where the log message was created
    /// </summary>
    public int ThreadId { get; }

    /// <summary>
    /// Gets the formatted log message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the exception associated with the log message, if any
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the message template used for formatting
    /// </summary>
    public string MessageTemplate { get; }

    /// <summary>
    /// Gets the additional data associated with the log message
    /// </summary>
    public IReadOnlyDictionary<string, object?> Data { get; }

    /// <summary>
    /// Gets the source type name where the log was created
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the source member name where the log was created
    /// </summary>
    public string Member { get; }

    /// <summary>
    /// Gets the source line number where the log was created
    /// </summary>
    public int Line { get; }

    internal LogMessage(
        TContext context,
        Instant instant,
        string subjectType,
        string subjectId,
        LogLevel level,
        int threadId,
        string message,
        Exception? exception,
        string messageTemplate,
        IReadOnlyDictionary<string, object?> data,
        string type,
        string member,
        int line
    )
    {
        Context = context;
        Instant = instant;
        SubjectType = subjectType;
        SubjectId = subjectId;
        Level = level;
        ThreadId = threadId;
        Message = message;
        Exception = exception;
        MessageTemplate = messageTemplate;
        Data = data;
        Type = type;
        Member = member;
        Line = line;
    }
}
