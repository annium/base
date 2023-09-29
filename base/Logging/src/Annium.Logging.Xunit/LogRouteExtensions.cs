using System;
using Annium.Logging.Shared;
using Annium.Logging.Xunit.Internal;
using Xunit.Abstractions;
using static Annium.Logging.Shared.LogMessageExtensions;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LogRouteExtensions
{
    public static LogRoute<TContext> UseTestOutput<TContext>(
        this LogRoute<TContext> route
    )
        where TContext : class, ILogContext
        => route.UseTestOutput(DefaultFormat<TContext>(UtcTime));

    public static LogRoute<TContext> UseTestOutput<TContext>(
        this LogRoute<TContext> route,
        Func<LogMessage<TContext>, string> format
    )
        where TContext : class, ILogContext
    {
        route.UseFactory(sp => new XunitLogHandler<TContext>(sp.Resolve<ITestOutputHelper>(), format), new LogRouteConfiguration());

        return route;
    }
}