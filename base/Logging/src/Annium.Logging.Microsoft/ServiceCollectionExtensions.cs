using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft;

/// <summary>
/// Extensions for IServiceCollection to register logging bridge services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the logging bridge services to the service collection
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLoggingBridge(this IServiceCollection services)
    {
        services.AddScoped<ILoggerProvider, LoggingBridgeProvider>();

        return services;
    }
}
