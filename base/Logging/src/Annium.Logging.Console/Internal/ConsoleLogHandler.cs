using System;
using Annium.Logging.Shared;

namespace Annium.Logging.Console.Internal;

internal class ConsoleLogHandler<TContext> : ILogHandler<TContext>
    where TContext : class, ILogContext
{
    private readonly Func<LogMessage<TContext>, string> _format;
    private readonly bool _color;

    public ConsoleLogHandler(
        Func<LogMessage<TContext>, string> format,
        bool color
    )
    {
        _format = format;
        _color = color;
    }

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