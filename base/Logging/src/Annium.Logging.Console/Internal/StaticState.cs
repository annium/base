using System;
using System.Collections.Generic;

namespace Annium.Logging.Console.Internal;

/// <summary>
/// Static state container for console logging functionality.
/// Provides shared console lock and color mappings for log levels.
/// </summary>
internal class StaticState
{
    /// <summary>
    /// Synchronization lock for thread-safe console output.
    /// </summary>
    public static readonly object ConsoleLock = new();

    /// <summary>
    /// Mapping of log levels to console colors for colored output.
    /// </summary>
    public static readonly IReadOnlyDictionary<LogLevel, ConsoleColor> LevelColors;

    static StaticState()
    {
        var colors = new Dictionary<LogLevel, ConsoleColor>
        {
            [LogLevel.Trace] = ConsoleColor.DarkGray,
            [LogLevel.Debug] = ConsoleColor.Gray,
            [LogLevel.Info] = ConsoleColor.White,
            [LogLevel.Warn] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red,
        };
        LevelColors = colors;
    }
}
