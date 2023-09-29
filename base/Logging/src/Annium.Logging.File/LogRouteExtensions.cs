using System;
using Annium.Logging.File;
using Annium.Logging.File.Internal;
using Annium.Logging.Shared;
using static Annium.Logging.Shared.LogMessageExtensions;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LogRouteExtensions
{
    public static LogRoute<TContext> UseFile<TContext>(
        this LogRoute<TContext> route,
        FileLoggingConfiguration<TContext> cfg
    )
        where TContext : class, ILogContext
        => route.UseFile(DefaultFormat<TContext>(LocalTime), cfg);

    public static LogRoute<TContext> UseFile<TContext>(
        this LogRoute<TContext> route,
        Func<LogMessage<TContext>, string> format,
        FileLoggingConfiguration<TContext> cfg
    )
        where TContext : class, ILogContext
    {
        route.UseAsyncInstance(new FileLogHandler<TContext>(format, cfg), cfg);

        return route;
    }
}