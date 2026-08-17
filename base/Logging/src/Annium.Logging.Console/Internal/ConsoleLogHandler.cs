using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLogHandler{TContext}"/> class.
    /// </summary>
    /// <param name="format">Formatter turning a log message into the console line.</param>
    /// <param name="color">Whether lines are colored per log level.</param>
    public ConsoleLogHandler(Func<LogMessage<TContext>, string> format, bool color)
    {
        _format = format;
        _color = color;
    }

    /// <summary>
    /// Writes the batch of log messages to the console under the shared console lock.
    /// </summary>
    /// <param name="messages">The log messages to write</param>
    /// <param name="ct">Cancellation token (unused — console writes are synchronous)</param>
    /// <returns>A completed value task</returns>
    public ValueTask HandleAsync(IReadOnlyList<LogMessage<TContext>> messages, CancellationToken ct)
    {
        lock (StaticState.ConsoleLock)
        {
            var currentColor = _color ? System.Console.ForegroundColor : default;
            try
            {
                foreach (var msg in messages)
                {
                    if (_color)
                        // fall back to a neutral color for any level absent from the map (e.g. LogLevel.None)
                        System.Console.ForegroundColor = StaticState.LevelColors.TryGetValue(msg.Level, out var c)
                            ? c
                            : ConsoleColor.White;

                    System.Console.WriteLine(_format(msg));
                }
            }
            finally
            {
                if (_color)
                    System.Console.ForegroundColor = currentColor;
            }
        }

        return ValueTask.CompletedTask;
    }
}
