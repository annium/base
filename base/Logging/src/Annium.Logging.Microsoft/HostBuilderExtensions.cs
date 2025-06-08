using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extensions for IHostBuilder to configure logging bridge
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Configures the logging bridge for the host builder
    /// </summary>
    /// <param name="builder">The host builder to configure</param>
    /// <returns>The configured host builder</returns>
    public static IHostBuilder ConfigureLoggingBridge(this IHostBuilder builder)
    {
        return builder.ConfigureLogging((_, logging) => logging.ConfigureLoggingBridge());
    }
}
