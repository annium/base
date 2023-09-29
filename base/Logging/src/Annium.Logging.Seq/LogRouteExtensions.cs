using Annium.Logging.Seq;
using Annium.Logging.Seq.Internal;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LogRouteExtensions
{
    public static LogRoute<TContext> UseSeq<TContext>(
        this LogRoute<TContext> route,
        SeqConfiguration configuration
    )
        where TContext : class, ILogContext
    {
        route.UseAsyncFactory(sp =>
        {
            var httpRequestFactory = sp.Resolve<IHttpRequestFactory>();
            var serializer = sp.Resolve<ISerializer<string>>();
            return new SeqLogHandler<TContext>(httpRequestFactory, serializer, configuration);
        }, configuration);

        return route;
    }
}