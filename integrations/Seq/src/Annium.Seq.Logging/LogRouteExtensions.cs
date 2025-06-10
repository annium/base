using System.Net.Mime;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Seq.Logging.Internal;
using Annium.Serialization.Abstractions;

namespace Annium.Seq.Logging;

/// <summary>
/// Extension methods for configuring Seq logging integration with the logging pipeline.
/// Provides fluent API for adding Seq as a log destination with CLEF formatting.
/// </summary>
public static class LogRouteExtensions
{
    /// <summary>
    /// Configures the log route to send filtered log events to a Seq server using CLEF format.
    /// Excludes HttpRequest logs and applies the existing route filter before sending to Seq.
    /// </summary>
    /// <typeparam name="TContext">The type of the logging context</typeparam>
    /// <param name="route">The log route to configure for Seq integration</param>
    /// <param name="configuration">Seq server configuration including endpoint, API key, and project settings</param>
    /// <returns>The configured log route for method chaining</returns>
    public static LogRoute<TContext> UseSeq<TContext>(this LogRoute<TContext> route, SeqConfiguration configuration)
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
                    return new SeqLogHandler<TContext>(httpRequestFactory, serializer, configuration);
                },
                configuration
            );

        return route;
    }
}
