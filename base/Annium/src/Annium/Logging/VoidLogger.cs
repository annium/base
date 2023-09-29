using System;
using System.Collections.Generic;

namespace Annium.Logging;

public class VoidLogger : ILogger
{
    public static readonly ILogger Instance = new VoidLogger();

    private VoidLogger()
    {
    }

    public void Log(object subject, string file, string member, int line, LogLevel level, string message, IReadOnlyList<object?> data)
    {
    }

    public void Error(object subject, string file, string member, int line, Exception exception, IReadOnlyList<object?> data)
    {
    }
}