using System;
using System.Collections.Generic;

namespace Annium.Logging.Console.Internal;

internal class StaticState
{
    public static readonly object ConsoleLock = new();
    public static readonly IReadOnlyDictionary<LogLevel, ConsoleColor> LevelColors;

    static StaticState()
    {
        var colors = new Dictionary<LogLevel, ConsoleColor>
        {
            [LogLevel.Trace] = ConsoleColor.DarkGray,
            [LogLevel.Debug] = ConsoleColor.Gray,
            [LogLevel.Info] = ConsoleColor.White,
            [LogLevel.Warn] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red
        };
        LevelColors = colors;
    }
}