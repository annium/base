using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureLoggingBridge(
        this IHostBuilder builder
    )
    {
        return builder.ConfigureLogging((_, logging) => logging.ConfigureLoggingBridge());
    }
}