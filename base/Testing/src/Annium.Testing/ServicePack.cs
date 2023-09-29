using System;
using Annium.Core.DependencyInjection;
using Annium.Testing.Executors;

namespace Annium.Testing;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRealTime().SetDefault();

        // components
        container.Add<TestDiscoverer>().AsSelf().Singleton();
        container.Add<TestExecutor>().AsSelf().Singleton();

        // executors
        container.Add<PipelineExecutor>().AsSelf().Singleton();
        container.Add<ITestExecutor, SkippedExecutor>().Singleton();
        container.Add<ITestExecutor, SetupExecutor>().Singleton();
        container.Add<ITestExecutor, BeforeExecutor>().Singleton();
        container.Add<ITestExecutor, BodyExecutor>().Singleton();
        container.Add<ITestExecutor, AfterExecutor>().Singleton();
        container.Add<MethodExecutor>().AsSelf().Singleton();

        // tools
        container.AddLogging();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route =>
        {
            var cfg = provider.Resolve<TestingConfiguration>();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (cfg is null)
                route.UseConsole();
            else
                route.For(m => m.Level >= cfg.LogLevel).UseConsole();
        });
    }
}