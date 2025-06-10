using System;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging.Shared;
using Annium.Logging.Xunit.Internal;
using Xunit;
using static Annium.Logging.Shared.LogMessageExtensions;

namespace Annium.Logging.Xunit;

/// <summary>
/// Extensions for LogRoute to configure xUnit test output logging
/// </summary>
public static class LogRouteExtensions
{
    /// <summary>
    /// Configures the route to output log messages to xUnit test output using default formatting
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="route">The log route to configure</param>
    /// <returns>The configured log route</returns>
    public static LogRoute<TContext> UseTestOutput<TContext>(this LogRoute<TContext> route)
        where TContext : class => route.UseTestOutput(DefaultFormat<TContext>(UtcTime));

    /// <summary>
    /// Configures the route to output log messages to xUnit test output using custom formatting
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="route">The log route to configure</param>
    /// <param name="format">The custom format function for log messages</param>
    /// <returns>The configured log route</returns>
    public static LogRoute<TContext> UseTestOutput<TContext>(
        this LogRoute<TContext> route,
        Func<LogMessage<TContext>, string> format
    )
        where TContext : class
    {
        route.UseFactory(
            sp => new XunitLogHandler<TContext>(sp.Resolve<ITestOutputHelper>(), format),
            new LogRouteConfiguration()
        );

        return route;
    }
}
