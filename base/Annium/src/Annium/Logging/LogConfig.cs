using System;
using System.Linq;

namespace Annium.Logging;

/// <summary>
/// Provides configuration for logging, including the global log level.
/// </summary>
public static class LogConfig
{
    /// <summary>
    /// Gets or sets the current global log level.
    /// </summary>
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

    /// <summary>
    /// Sets the global log level.
    /// </summary>
    /// <param name="level">The log level to set.</param>
    public static void SetLevel(LogLevel level) => Level = level;
}
