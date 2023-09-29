using System;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Logging.Shared;

public record LogMessage<TContext>
    where TContext : ILogContext
{
    public TContext Context { get; }
    public Instant Instant { get; }
    public string SubjectType { get; }
    public string SubjectId { get; }
    public LogLevel Level { get; }
    public int ThreadId { get; }
    public string Message { get; }
    public Exception? Exception { get; }
    public string MessageTemplate { get; }
    public IReadOnlyDictionary<string, object?> Data { get; }
    public string Type { get; }
    public string Member { get; }
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