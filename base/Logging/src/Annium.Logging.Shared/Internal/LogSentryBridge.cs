using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Annium.Logging.Shared.Internal;

internal class LogSentryBridge<TContext> : ILogSentryBridge
    where TContext : class, ILogContext
{
    private readonly ITimeProvider _timeProvider;
    private readonly TContext _context;
    private readonly ILogSentry<TContext> _logSentry;

    public LogSentryBridge(
        ITimeProvider timeProvider,
        TContext context,
        ILogSentry<TContext> logSentry
    )
    {
        _timeProvider = timeProvider;
        _context = context;
        _logSentry = logSentry;
    }

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