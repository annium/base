using System;
using System.Collections.Generic;

namespace Annium.Logging.Shared.Internal;

internal class Logger : ILogger
{
    private readonly ILogSentryBridge _sentryBridge;

    public Logger(ILogSentryBridge sentryBridge)
    {
        _sentryBridge = sentryBridge;
    }

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

    public void Error(object subject, string file, string member, int line, Exception ex, IReadOnlyList<object?> data)
    {
        var subjectType = subject is ILogBridge bridge ? bridge.Type : subject.GetType().FriendlyName();
        var subjectId = subject.GetId();

        _sentryBridge.Register(subjectType, subjectId, file, member, line, LogLevel.Error, ex.Message, ex, data);
    }
}
