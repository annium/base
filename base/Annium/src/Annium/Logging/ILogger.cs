using System;
using System.Collections.Generic;

namespace Annium.Logging;

public interface ILogger
{
    void Log(object subject, string file, string member, int line, LogLevel level, string message, IReadOnlyList<object?> data);
    void Error(object subject, string file, string member, int line, Exception exception, IReadOnlyList<object?> data);
}