using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Logging.Shared.Internal;

namespace Annium.Logging.Shared;

/// <summary>
/// Extensions for IServiceContainer to register logging services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds logging services for a specific context type
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="container">The service container</param>
    /// <returns>The service container for chaining</returns>
    public static IServiceContainer AddLogging<TContext>(this IServiceContainer container)
        where TContext : class
    {
        return container.AddLoggingBase<TContext>();
    }

    /// <summary>
    /// Adds logging services using the default log context
    /// </summary>
    /// <param name="container">The service container</param>
    /// <returns>The service container for chaining</returns>
    public static IServiceContainer AddLogging(this IServiceContainer container)
    {
        return container.AddLoggingBase<DefaultLogContext>();
    }

    /// <summary>
    /// Internal method to add base logging services for a context type
    /// </summary>
    /// <typeparam name="TContext">The type of log context</typeparam>
    /// <param name="container">The service container</param>
    /// <returns>The service container for chaining</returns>
    private static IServiceContainer AddLoggingBase<TContext>(this IServiceContainer container)
        where TContext : class
    {
        container
            .Add(new List<ILogScheduler<TContext>>())
            .AsSelf()
            .As<IReadOnlyCollection<ILogScheduler<TContext>>>()
            .Singleton();

        container.Add<TContext>().AsSelf().Scoped();
        container.Add<ILogger, Logger>().Scoped();
        container.Add<ILogBridgeFactory, LogBridgeFactory>().Scoped();
        container.Add<LogRouter<TContext>>().AsSelf().Singleton();
        container.Add<ILogSentry<TContext>, LogSentry<TContext>>().Singleton();
        container.Add<ILogSentryBridge, LogSentryBridge<TContext>>().Scoped();
        container.AddProfile(p =>
        {
            p.Map<LogLevel, string>(x => x.ToString());
            p.Map<string, LogLevel>(x => EnumExtensions.ParseEnum<LogLevel>((string)x));
        });

        container.OnBuild += sp => sp.Resolve<LogRouter<TContext>>();

        return container;
    }
}
