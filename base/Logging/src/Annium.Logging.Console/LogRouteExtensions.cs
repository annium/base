using System;
using Annium.Logging.Console.Internal;
using Annium.Logging.Shared;
using static Annium.Logging.Shared.LogMessageExtensions;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LogRouteExtensions
{
    public static LogRoute<TContext> UseConsole<TContext>(
        this LogRoute<TContext> route,
        bool color = false
    )
        where TContext : class, ILogContext
        => route.UseConsole(DefaultFormat<TContext>(LocalTime), color);

    public static LogRoute<TContext> UseConsole<TContext>(
        this LogRoute<TContext> route,
        Func<LogMessage<TContext>, string> format,
        bool color = false
    )
        where TContext : class, ILogContext
    {
        route.UseInstance(new ConsoleLogHandler<TContext>(format, color), new LogRouteConfiguration());

        return route;
    }
}