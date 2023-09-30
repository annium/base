using System;
using System.Linq;

namespace Annium.Logging;

public static class LogConfig
{
    public static LogLevel Level { get; private set; }

    static LogConfig()
    {
        var args = Environment.GetCommandLineArgs();

        if (args.Contains("-trace"))
        {
            Level = LogLevel.Trace;
            return;
        }

        if (args.Contains("-debug"))
        {
            Level = LogLevel.Debug;
            return;
        }

        var raw = Environment.GetEnvironmentVariable("ANNIUM_LOG");
        switch (raw?.Trim())
        {
            case "trace":
                Level = LogLevel.Trace;
                break;
            case "debug":
                Level = LogLevel.Debug;
                break;
            default:
                Level = LogLevel.Info;
                break;
        }
    }

    public static void SetLevel(LogLevel level) => Level = level;
}