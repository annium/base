using System;
using System.Collections.Generic;

namespace Annium.Logging.Shared;

public interface ILogSentryBridge
{
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
    );
}