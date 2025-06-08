using System;
using Annium.Logging.Console.Internal;
using Annium.Logging.Shared;
using static Annium.Logging.Shared.LogMessageExtensions;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring console log routing.
/// Provides fluent API for adding console logging to log routes.
/// </summary>
public static class LogRouteExtensions
{
    /// <summary>
    /// Configures the log route to use console output with default formatting.
    /// </summary>
    /// <typeparam name="TContext">The type of the log context</typeparam>
    /// <param name="route">The log route to configure</param>
    /// <param name="color">Whether to enable color formatting</param>
    /// <returns>The configured log route</returns>
    public static LogRoute<TContext> UseConsole<TContext>(this LogRoute<TContext> route, bool color = false)
        where TContext : class => route.UseConsole(DefaultFormat<TContext>(LocalTime), color);

    /// <summary>
    /// Configures the log route to use console output with custom formatting.
    /// </summary>
    /// <typeparam name="TContext">The type of the log context</typeparam>
    /// <param name="route">The log route to configure</param>
    /// <param name="format">Custom formatting function for log messages</param>
    /// <param name="color">Whether to enable color formatting</param>
    /// <returns>The configured log route</returns>
    public static LogRoute<TContext> UseConsole<TContext>(
        this LogRoute<TContext> route,
        Func<LogMessage<TContext>, string> format,
        bool color = false
    )
        where TContext : class
    {
        route.UseInstance(new ConsoleLogHandler<TContext>(format, color), new LogRouteConfiguration());

        return route;
    }
}
