using System;
using Annium.Logging.Shared;

namespace Annium.Logging.Console.Internal;

/// <summary>
/// Log handler that writes log messages to the console with optional color formatting.
/// Provides thread-safe console output with color coding based on log levels.
/// </summary>
/// <typeparam name="TContext">The type of the log context</typeparam>
internal class ConsoleLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class
{
    /// <summary>
    /// Function to format log messages for console output.
    /// </summary>
    private readonly Func<LogMessage<TContext>, string> _format;

    /// <summary>
    /// Indicates whether to use color formatting for console output.
    /// </summary>
    private readonly bool _color;

    public ConsoleLogHandler(Func<LogMessage<TContext>, string> format, bool color)
    {
        _format = format;
        _color = color;
    }

    /// <summary>
    /// Handles a log message by writing it to the console with optional color formatting.
    /// </summary>
    /// <param name="msg">The log message to handle</param>
    public void Handle(LogMessage<TContext> msg)
    {
        lock (StaticState.ConsoleLock)
        {
            var currentColor = _color ? System.Console.ForegroundColor : default;
            try
            {
                if (_color)
                    System.Console.ForegroundColor = StaticState.LevelColors[msg.Level];

                System.Console.WriteLine(_format(msg));
            }
            finally
            {
                if (_color)
                    System.Console.ForegroundColor = currentColor;
            }
        }
    }
}
