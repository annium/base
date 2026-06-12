using System;
using System.Threading;

namespace Annium.Logging;

/// <summary>
/// Provides configuration for logging, including the global log level.
/// </summary>
public static class LogConfig
{
    /// <summary>
    /// Backing storage for <see cref="Level"/>; accessed via <see cref="Volatile"/> for cross-thread visibility.
    /// </summary>
    private static int _level;

    /// <summary>
    /// Gets the current global log level.
    /// </summary>
    public static LogLevel Level => (LogLevel)Volatile.Read(ref _level);

    static LogConfig()
    {
        var args = Environment.GetCommandLineArgs();

        if (args.Contains("-trace"))
        {
            Volatile.Write(ref _level, (int)LogLevel.Trace);
            return;
        }

        if (args.Contains("-debug"))
        {
            Volatile.Write(ref _level, (int)LogLevel.Debug);
            return;
        }

        var raw = Environment.GetEnvironmentVariable("ANNIUM_LOG");
        switch (raw?.Trim())
        {
            case "trace":
                Volatile.Write(ref _level, (int)LogLevel.Trace);
                break;
            case "debug":
                Volatile.Write(ref _level, (int)LogLevel.Debug);
                break;
            default:
                Volatile.Write(ref _level, (int)LogLevel.Info);
                break;
        }
    }

    /// <summary>
    /// Sets the global log level.
    /// </summary>
    /// <param name="level">The log level to set.</param>
    public static void SetLevel(LogLevel level) => Volatile.Write(ref _level, (int)level);
}
