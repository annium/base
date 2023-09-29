using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Logging.Seq;

namespace Demo.Logging;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();

        container.AddLogging();
        container.AddMapper();
        container.AddArguments();
        container.AddSerializers().WithJson(isDefault: true);
        container.AddHttpRequestFactory(true);

        // commands
        container.AddAll(GetType().Assembly)
            .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
            .AsSelf()
            .Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        // provider.UseLogging(route => route.UseConsole());
        provider.UseLogging(route => route
                .For(m => m.Level == LogLevel.Info).UseSeq(
                    new SeqConfiguration
                    {
                        Endpoint = new Uri("http://localhost:5341"),
                        ApiKey = "rtLlglmGD5ffTOujuROD",
                        Project = "logging-demo",
                        BufferTime = TimeSpan.FromMilliseconds(50),
                        BufferCount = 1
                    }
                )
#if LOG_DEBUG
            .For(m => m.Level == LogLevel.Debug).UseConsole()
#endif
#if LOG_TRACE
            .For(m => m.Level == LogLevel.Trace).UseConsole()
            .For(m => m.Level == LogLevel.Trace).UseInMemory()
#endif
        );
    }
}