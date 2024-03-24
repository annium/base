using Annium.Graylog.Logging;
using Annium.Graylog.Logging.Internal;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class LogRouteExtensions
{
    public static LogRoute<TContext> UseGraylog<TContext>(
        this LogRoute<TContext> route,
        GraylogConfiguration configuration
    )
        where TContext : class, ILogContext
    {
        var filter = route.Filter;
        route
            .For(m => m.SubjectType != "HttpRequest" && filter(m))
            .UseAsyncFactory(
                sp =>
                {
                    var httpRequestFactory = sp.Resolve<IHttpRequestFactory>();
                    var serializer = sp.Resolve<ISerializer<string>>();
                    return new GraylogLogHandler<TContext>(httpRequestFactory, serializer, configuration);
                },
                configuration
            );

        return route;
    }
}
