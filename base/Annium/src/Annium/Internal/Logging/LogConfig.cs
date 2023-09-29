using System;
using System.Linq;
using Annium.Logging;

namespace Annium.Internal.Logging;

internal static class LogConfig
{
    public static readonly LogLevel Level;

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
}