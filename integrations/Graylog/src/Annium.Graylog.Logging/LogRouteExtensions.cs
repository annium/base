using System.Net.Mime;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Graylog.Logging.Internal;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;

namespace Annium.Graylog.Logging;

/// <summary>
/// Extension methods for configuring Graylog logging integration within log routes.
/// </summary>
public static class LogRouteExtensions
{
    /// <summary>
    /// Configures the log route to send log messages to Graylog using GELF format, excluding HTTP request logs and applying the specified configuration.
    /// </summary>
    /// <typeparam name="TContext">The type of the logging context that provides additional contextual information for log messages.</typeparam>
    /// <param name="route">The log route to be configured with Graylog integration.</param>
    /// <param name="configuration">The Graylog configuration containing endpoint, project, and enablement settings.</param>
    /// <returns>The configured log route with Graylog handler attached, or the original route if Graylog is disabled.</returns>
    public static LogRoute<TContext> UseGraylog<TContext>(
        this LogRoute<TContext> route,
        GraylogConfiguration configuration
    )
        where TContext : class
    {
        if (!configuration.IsEnabled)
            return route;

        var filter = route.Filter;
        route
            .For(m => m.SubjectType != "HttpRequest" && filter(m))
            .UseAsyncFactory(
                sp =>
                {
                    var httpRequestFactory = sp.Resolve<IHttpRequestFactory>();
                    var serializer = sp.ResolveKeyed<ISerializer<string>>(
                        SerializerKey.CreateDefault(MediaTypeNames.Application.Json)
                    );
                    return new GraylogLogHandler<TContext>(httpRequestFactory, serializer, configuration);
                },
                configuration
            );

        return route;
    }
}
