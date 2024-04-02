using System.Collections.Generic;
using Annium.Logging;
using Annium.Logging.Shared;
using Annium.Logging.Shared.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddLogging<TContext>(this IServiceContainer container)
        where TContext : class
    {
        return container.AddLoggingBase<TContext>();
    }

    public static IServiceContainer AddLogging(this IServiceContainer container)
    {
        return container.AddLoggingBase<DefaultLogContext>();
    }

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
            p.Map<string, LogLevel>(x => x.ParseEnum<LogLevel>());
        });

        container.OnBuild += sp => sp.Resolve<LogRouter<TContext>>();

        return container;
    }
}
