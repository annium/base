using System;
using Annium.Logging.File.Internal;
using Annium.Logging.Shared;
using static Annium.Logging.Shared.LogMessageExtensions;

namespace Annium.Logging.File;

/// <summary>
/// Extension methods for configuring file-based log routing.
/// Provides fluent API for adding file logging to log routes.
/// </summary>
public static class LogRouteExtensions
{
    /// <summary>
    /// Configures the log route to use file output with default formatting.
    /// </summary>
    /// <typeparam name="TContext">The type of the log context</typeparam>
    /// <param name="route">The log route to configure</param>
    /// <param name="cfg">File logging configuration</param>
    /// <returns>The configured log route</returns>
    public static LogRoute<TContext> UseFile<TContext>(
        this LogRoute<TContext> route,
        FileLoggingConfiguration<TContext> cfg
    )
        where TContext : class => route.UseFile(DefaultFormat<TContext>(LocalTime), cfg);

    /// <summary>
    /// Configures the log route to use file output with custom formatting.
    /// </summary>
    /// <typeparam name="TContext">The type of the log context</typeparam>
    /// <param name="route">The log route to configure</param>
    /// <param name="format">Custom formatting function for log messages</param>
    /// <param name="cfg">File logging configuration</param>
    /// <returns>The configured log route</returns>
    public static LogRoute<TContext> UseFile<TContext>(
        this LogRoute<TContext> route,
        Func<LogMessage<TContext>, string> format,
        FileLoggingConfiguration<TContext> cfg
    )
        where TContext : class
    {
        route.UseAsyncInstance(new FileLogHandler<TContext>(format, cfg), cfg);

        return route;
    }
}
