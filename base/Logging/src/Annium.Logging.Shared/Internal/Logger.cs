using System;
using System.Collections.Generic;

namespace Annium.Logging.Shared.Internal;

internal class Logger : ILogger
{
    private readonly ILogSentryBridge _sentryBridge;

    public Logger(
        ILogSentryBridge sentryBridge
    )
    {
        _sentryBridge = sentryBridge;
    }

    public void Log(object subject, string file, string member, int line, LogLevel level, string message, IReadOnlyList<object?> data)
    {
        _sentryBridge.Register(subject.GetType().FriendlyName(), subject.GetId(), file, member, line, level, message, null, data);
    }

    public void Error(object subject, string file, string member, int line, Exception exception, IReadOnlyList<object?> data)
    {
        _sentryBridge.Register(subject.GetType().FriendlyName(), subject.GetId(), file, member, line, LogLevel.Error, exception.Message, exception, data);
    }
}